using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Application.DTO;
using WMS.Domain.ExceptionControl;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class StockService
    {
        private readonly IComponentRepository _componentRepo;
        private readonly IStockRepository _stockRepo;
        private readonly IHistoryRepository _historyRepo;
        private readonly ILogger<StockService> _logger;
        private readonly IUnitOfWork _uow;
        public StockService(
            IComponentRepository componentRepo,
            IStockRepository stockRepo,
            IHistoryRepository operationRepo,
            ILogger<StockService> logger,
            IUnitOfWork uow)
        {
            _componentRepo = componentRepo;
            _stockRepo = stockRepo;
            _historyRepo = operationRepo;
            _logger = logger;
            _uow = uow;
        }

        public async Task IssueAsync(
            IReadOnlyCollection<IssueItemDto> items,
            string operatorName,
            string? comment = null)
        {
            if (items.Count == 0)
                throw new BusinessException("Список выдачи пуст");

            var operation = History.Create(OperationType.Issue, operatorName, comment);

            foreach (var item in items)
            {
                var stock = await _stockRepo.GetByIdAsync(item.StockId);

                if (stock is null)
                    throw new BusinessException("Товар в указанной ячейке не найден");

                var before = stock.Quantity;

                stock.Issue(item.Quantity);

                _stockRepo.Update(stock);

                if (stock.Quantity == 0)
                    _stockRepo.Delet(stock);

                operation.AddItem(
                    stock.ComponentId,
                    stock.RackId,
                    stock.CellId,
                    before,
                    stock.Quantity);
            }

            operation.Validate();
            _historyRepo.Add(operation);
        }

        public async Task ReceiveAsync(
            ReceiptItemDto item,
            string operatorName,
            string? comment = null,
            CancellationToken ct = default)
        {
            if (item is null)
            {
                _logger.LogWarning("ReceiveAsync called with null item by {Operator}", operatorName);
                throw new ArgumentNullException(nameof(item));
            }

            _logger.LogInformation(
                "Starting receipt. Component:{ComponentId}, Rack:{RackId}, Cell:{CellId}, Qty:{Quantity}, Operator:{Operator}",
                item.ComponentId, item.RackId, item.CellId, item.Quantity, operatorName);

            try
            {
                var component = await _uow.Components.GetByIdAsync(item.ComponentId, ct);

                if (component is null)
                {
                    _logger.LogWarning("Component {ComponentId} not found", item.ComponentId);

                    throw new BusinessException($"Компонент {item.ComponentId} не найден");
                }

                var stock = await _uow.Stocks.GetByLocationAsync(item.ComponentId, item.RackId, item.CellId, ct);

                int before;
                int after;

                if (stock is null)
                {
                    _logger.LogInformation(
                        "Creating new stock at Rack:{RackId} Cell:{CellId}",
                        item.RackId, item.CellId);

                    stock = Stock.Create(item.ComponentId, item.RackId, item.CellId, 0);
                    before = 0;
                    stock.Receive(item.Quantity);
                    after = stock.Quantity;
                    _uow.Stocks.Add(stock, ct);
                }
                else
                {
                    before = stock.Quantity;
                    stock.Receive(item.Quantity);
                    after = stock.Quantity;

                    _logger.LogInformation(
                        "Updating stock {StockId}: {Before} -> {After}",
                        stock.Id, before, after);

                    _uow.Stocks.Update(stock, ct);
                }

                var operation = History.Create(OperationType.Receipt, operatorName, comment);
                operation.AddItem(item.ComponentId, item.RackId, item.CellId, before, after);
                operation.Validate();

                _uow.History.Add(operation, ct);

                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Receipt completed. Component:{ComponentId}, Change:{Before}->{After}",
                    item.ComponentId, before, after);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Receipt FAILED for Component:{ComponentId}, Operator:{Operator}",
                    item.ComponentId, operatorName);

                throw;
            }
        }

        public async Task<IReadOnlyList<ViewItemDTO>> GetFilteredStockOrComponentsAsync(string searchText, int maxCount, CancellationToken token)
        {
            var stocks = await _stockRepo.SearchAsync(searchText, maxCount);
            var components = await _componentRepo.GetFilteredComponentAsync(searchText, maxCount, token);

            var result = new List<ViewItemDTO>();

            foreach (var stock in stocks)
            {
                result.Add(MappingExtensions.ToViewItemDto(stock));
            }
            var existingComponentIds = result.Select(x => x.ComponentId).ToHashSet();

            foreach (var component in components)
            {
                if (existingComponentIds.Contains(component.Id))
                    continue; // Пропускаем, так как Stock для этого компонента уже добавлен

                result.Add(MappingExtensions.ToViewItemDto(component));
            }

            return result.Take(maxCount).ToList();
        }

        public async Task<IReadOnlyList<ViewItemDTO>> GetFilteredStockAsync(string searchText, int maxCount)
        {
            var stocks = await _stockRepo.SearchAsync(searchText, maxCount);

            return stocks.Select(MappingExtensions.ToViewItemDto).ToList();
        }

        public async Task<IReadOnlyList<ViewItemDTO>> GetStocksInRackAsync(Guid rackId)
        {
            var result = await _stockRepo.GetByRackAsync(rackId);

            return result.Select(MappingExtensions.ToViewItemDto).ToList();
        }

        public async Task<ViewItemDTO> GetStockByIdAsync(Guid stokId)
        {
            var result = await _stockRepo.GetByIdAsync(stokId);

            return MappingExtensions.ToViewItemDto(result);
        }
    }
}
