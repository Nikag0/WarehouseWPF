using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using WMS.Application.Abstractions;
using WMS.Application.DTO;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using System.Linq;


namespace WMS.Application.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepo;
        private readonly IComponentRepository _componentRepo;
        private readonly IHistoryRepository _historyRepo;
        private readonly ILogger<StockService> _logger;
        private readonly IUnitOfWorkFactory _uowFactory;

        public StockService(
            IComponentRepository componentRepo,
            IStockRepository stockRepo,
            IHistoryRepository historyRepo,
            ILogger<StockService> logger,
            IUnitOfWorkFactory uowFactory)
        {
            _componentRepo = componentRepo;
            _stockRepo = stockRepo;
            _historyRepo = historyRepo;
            _logger = logger;
            _uowFactory = uowFactory;
        }

        public async Task IssueAsync(
            IReadOnlyCollection<IssueStockDto> issueStocks,
            string operatorName,
            string? comment = null,
            CancellationToken ct = default)
        {

            if (issueStocks.Count == 0)
                throw new BusinessException("Список выдачи пуст");

            try
            {
                await using var uow = _uowFactory.Create();

                var stockIds = issueStocks.Select(x => x.StockId).ToList();

                var stocks = await _stockRepo.GetByIdsAsync(stockIds, ct);

                var stocksDictionary = stocks.ToDictionary(x => x.Id);

                foreach (var issueStock in issueStocks)
                {
                    if (!stocksDictionary.TryGetValue(issueStock.StockId, out var stock))
                    {
                        _logger.LogWarning("Stock {StockId} not found in preloaded data", issueStock.StockId);
                        throw new BusinessException("Товар в одной из указанных ячеек не найден");
                    }

                    var previousQuantity = stock.Quantity;


                    stock.Issue(issueStock.QuantityChanged);

                    if (stock.Quantity == 0)
                    {
                        _stockRepo.Delete(stock, ct);
                    }
                    else
                    {
                        _stockRepo.Update(stock, ct);
                    }

                    var operationHistory = OperationHistory.Create(
                        OperationType.Issue, 
                        operatorName, 
                        comment,
                        issueStock.ComponentId,
                        issueStock.ComponentName,
                        issueStock.RackId,
                        issueStock.RackName,
                        issueStock.CellId,
                        issueStock.CellName,
                        previousQuantity,
                        issueStock.QuantityChanged,
                        stock.Quantity);

                    _historyRepo.Add(operationHistory, ct);
                }

                await uow.CommitAsync();

                _logger.LogInformation(
                    "Issue operation completed successfully. ItemCount:{ItemCount}",
                    issueStocks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Issue operation FAILED. ItemCount:{ItemCount}, Operator:{Operator}",
                    issueStocks.Count, operatorName);

                throw;
            }
        }

        public async Task ReceiveAsync(
            ReceiptItemDto receiptStock,
            string operatorName,
            string? comment = null,
            CancellationToken ct = default)
        {
            if (receiptStock is null)
            {
                _logger.LogWarning("ReceiveAsync called with null item by {Operator}", operatorName);
                throw new ArgumentNullException(nameof(receiptStock));
            }

            try
            {
                await using var uow = _uowFactory.Create();

                var component = await _componentRepo.GetByIdAsync(receiptStock.ComponentId);

                if (component is null)
                {
                    _logger.LogWarning("Component {ComponentId} not found", receiptStock.ComponentId);

                    throw new BusinessException($"Компонент {receiptStock.ComponentId} не найден");
                }

                var stock = await _stockRepo.GetByLocationAsync(receiptStock.ComponentId, receiptStock.RackId, receiptStock.CellId, ct);

                int previousQuantity;
                int newQuantity;

                if (stock is null)
                {
                    stock = Stock.Create(receiptStock.ComponentId, receiptStock.RackId, receiptStock.CellId, 0);
                    previousQuantity = 0;
                    stock.Receive(receiptStock.Quantity);
                    newQuantity = stock.Quantity;
                    _stockRepo.Add(stock, ct);
                }
                else
                {
                    previousQuantity = stock.Quantity;
                    stock.Receive(receiptStock.Quantity);
                    newQuantity = stock.Quantity;
                }

                var operationHistory = OperationHistory.Create(
                        OperationType.Receipt,
                        operatorName,
                        comment,
                        stock.ComponentId,
                        stock.Component.Name,
                        receiptStock.RackId,
                        receiptStock.RackName,
                        receiptStock.CellId,
                        receiptStock.CellName,
                        previousQuantity,
                        receiptStock.Quantity,
                        newQuantity);

                _historyRepo.Add(operationHistory, ct);

                await uow.CommitAsync();

                _logger.LogInformation(
                    "Receipt completed. Component:{ComponentId}, Change:{Before}->{After}",
                    receiptStock.ComponentId, previousQuantity, newQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Receipt FAILED for Component:{ComponentId}, Operator:{Operator}",
                    receiptStock.ComponentId, operatorName);

                throw;
            }
        }

        public async Task<IReadOnlyList<StockViewDTO>> GetFilteredStockOrComponentsAsync(string searchText, int maxCount, CancellationToken token)
        {
            await using var uow = _uowFactory.Create();
            
            var stocks = await _stockRepo.SearchAsync(searchText, maxCount);
            var components = await _componentRepo.GetViewFilterAsync(searchText, maxCount, token);

            var result = new List<StockViewDTO>();

            foreach (var stock in stocks)
            {
                result.Add(MappingExtensions.StockToView(stock));
            }
            var existingComponentIds = result.Select(x => x.ComponentId).ToHashSet();

            foreach (var component in components)
            {
                if (existingComponentIds.Contains(component.Id))
                    continue; // Пропускаем, так как Stock для этого компонента уже добавлен

                result.Add(MappingExtensions.ComponentViewtoStockView(component));
            }

            return result.Take(maxCount).ToList();
        }

        public async Task<IReadOnlyList<StockViewDTO>> GetFilteredStockAsync(string searchText, int maxCount)
        {
            await using var uow = _uowFactory.Create();

            var stocks = await _stockRepo.SearchAsync(searchText, maxCount);

            return stocks.Select(MappingExtensions.StockToView).ToList();
        }

        public async Task<IReadOnlyList<StockViewDTO>> GetStocksInRackAsync(Guid rackId)
        {
            await using var uow = _uowFactory.Create();

            var result = await _stockRepo.GetByRackAsync(rackId);

            return result.Select(MappingExtensions.StockToView).ToList();
        }

        public async Task<StockViewDTO?> GetStockByIdAsync(Guid stokId)
        {
            await using var uow = _uowFactory.Create();

            var result = await _stockRepo.GetByIdAsync(stokId);

            // Остаток удаляется из БД, когда товар выдан полностью
            return result is null ? null : MappingExtensions.StockToView(result);
        }

        public async Task ExportCsvAsync(string filePath, CancellationToken ct = default)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = ";", 
                HasHeaderRecord = true
            };

            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            using var csv = new CsvWriter(writer, config);

            await using var uow = _uowFactory.Create();

            IAsyncEnumerable<Stock> stocksStream = _stockRepo.GetAllForExportAsync(ct);

            IAsyncEnumerable<StockCsvDto> dtoStream = stocksStream.Select(stock => stock.StockToCsv());

            await csv.WriteRecordsAsync(dtoStream, ct);
        }
    }
}
