using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class ReceiptService
    {
        private readonly IComponentRepository _componentRepo;
        private readonly IStockRepository _stockRepo;
        private readonly IHistoryRepository _historyRepo;

        public ReceiptService(
            IComponentRepository componentRepo,
            IStockRepository stockRepo, 
            IHistoryRepository operationRepo)
        {
            _componentRepo = componentRepo;
            _stockRepo = stockRepo;
            _historyRepo = operationRepo;
        }

        public async Task ReceiveAsync(ReceiptItemDto item, string operatorName, string? comment = null)
        {
            if (item == null)
                return;

            var operation = Operation.Create(OperationType.Receipt, operatorName, comment);

            Component? component = await _componentRepo.GetByIdAsync(item.ComponentId);

            if (component is null)
                    throw new Exception($"Компонент {item.ComponentId} не найден");

            Stock? stock = await _stockRepo.GetByLocationAsync(item.ComponentId, item.RackId, item.CellId);

            int before;

            if (stock is null)
            {
                stock = Stock.Create(item.ComponentId, item.RackId ,item.CellId, 0);
                before = 0;
                stock.Receive(item.Quantity);
                await _stockRepo.AddAsync(stock);
            }
            else
            {
                before = stock.Quantity;
                stock.Receive(item.Quantity);
                await _stockRepo.UpdateAsync(stock);
            }

            operation.AddItem(
                    item.ComponentId,
                    item.RackId,
                    item.CellId,
                    before,
                    stock.Quantity);

            operation.Validate();

            await _historyRepo.AddAsync(operation);
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

        public async Task<IReadOnlyList<ViewItemDTO>> GetStocksInRackAsync(Guid rackId)
        {
            var result = await _stockRepo.GetByRackAsync(rackId);

            return result.Select(MappingExtensions.ToViewItemDto).ToList();
        }
    }
}