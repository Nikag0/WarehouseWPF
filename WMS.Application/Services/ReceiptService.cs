using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class ReceiptService
    {
        private readonly IComponentRepository _componentRepo;
        private readonly IStockRepository _stockRepo;
        private readonly IOperationRepository _operationRepo;

        public ReceiptService(
            IComponentRepository componentRepo,
            IStockRepository stockRepo, 
            IRackRepository rackRepo,
            IOperationRepository operationRepo)
        {
            _componentRepo = componentRepo;
            _stockRepo = stockRepo;
            _operationRepo = operationRepo;
        }

        public async Task<List<ViewItemDTO>> GetAllComponentsAsync()
        {
            var components = await _componentRepo.GetAllAsync();

            return components
                .Select(MappingExtensions.ToViewItemDto).ToList(); ;
        }

        public async Task ReceiveAsync(ServiceItemDTO item, string operatorName, string? comment = null)
        {
            if (item == null)
                return;

            var operation = Operation.Create(OperationType.Receipt, operatorName, comment);

            Component? component = await _componentRepo.GetByIdAsync(item.ComponentId);

            if (component is null)
                    throw new Exception($"Компонент {item.ComponentId} не найден");

            Stock? stock = await _stockRepo.GetStockAsync(item.RackId);

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

            await _operationRepo.AddAsync(operation);
        }

        public async Task<IEnumerable<ViewItemDTO>> GetFilteredStockOrComponentsAsync(string searchText, int maxCount)
        {
            var stocks = await _stockRepo.GetFilteredStockAsync(searchText, maxCount);
            var components = await _componentRepo.GetFilteredComponentAsync(searchText, maxCount);

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

            return result.Take(maxCount);
        }
    }
}