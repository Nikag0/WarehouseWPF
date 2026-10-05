using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Application.DTO;
using WMS.Domain;
using WMS.Domain.ExceptionControl;

namespace WMS.Application.Services
{
    public class StockService : IStockService
    {
        private readonly IComponentRepository _componentRepo;
        private readonly IStockRepository _stockRepo;
        private readonly ILogger<StockService> _logger;
        private readonly IUnitOfWork _uow;
        public StockService(
            IComponentRepository componentRepo,
            IStockRepository stockRepo,
            ILogger<StockService> logger,
            IUnitOfWork uow)
        {
            _componentRepo = componentRepo;
            _stockRepo = stockRepo;
            _logger = logger;
            _uow = uow;
        }

        public async Task IssueAsync(
            IReadOnlyCollection<IssueItemDto> items,
            string operatorName,
            string? comment = null,
            CancellationToken ct = default)
        {

            if (items.Count == 0)
                throw new BusinessException("Список выдачи пуст");

            try
            {
                var operation = History.Create(OperationType.Issue, operatorName, comment);

                var stockIds = items.Select(x => x.StockId).ToList();

                var stocks = await _uow.Stocks.GetByIdsAsync(stockIds, ct);

                var stocksDictionary = stocks.ToDictionary(x => x.Id);

                foreach (var item in items)
                {
                    if (!stocksDictionary.TryGetValue(item.StockId, out var stock))
                    {
                        _logger.LogWarning("Stock {StockId} not found in preloaded data", item.StockId);
                        throw new BusinessException("Товар в одной из указанных ячеек не найден");
                    }

                    var before = stock.Quantity;


                    stock.Issue(item.Quantity);

                    if (stock.Quantity == 0)
                    {
                        _uow.Stocks.Delete(stock, ct);
                    }
                    else
                    {
                        _uow.Stocks.Update(stock, ct);
                    }

                    operation.AddItem(
                        stock.ComponentId,
                        stock.RackId,
                        stock.CellId,
                        before,
                        stock.Quantity);
                }

                operation.Validate();
                _uow.History.Add(operation, ct);

                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Issue operation completed successfully. ItemCount:{ItemCount}",
                    items.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Issue operation FAILED. ItemCount:{ItemCount}, Operator:{Operator}",
                    items.Count, operatorName);

                throw;
            }
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

            try
            {
                var component = await _uow.Components.GetByIdAsync(item.ComponentId);

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
            var components = await _componentRepo.GetViewFilterAsync(searchText, maxCount, token);

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

                result.Add(MappingExtensions.ComponentViewtoItemView(component));
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
