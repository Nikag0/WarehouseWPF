using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class ReceiptService
    {
        private readonly IComponentRepository _componentRepo;
        private readonly IStockRepository _stockRepo;
        private readonly ICellRepository _cellRepo;
        private readonly IRackRepository _rackRepo;
        private readonly IOperationRepository _operationRepo;

        public ReceiptService(
            IComponentRepository componentRepo,
            IStockRepository stockRepo, 
            ICellRepository cellRepo,
            IRackRepository rackRepo,
            IOperationRepository operationRepo)
        {
            _componentRepo = componentRepo;
            _stockRepo = stockRepo;
            _cellRepo = cellRepo;
            _rackRepo = rackRepo;
            _operationRepo = operationRepo;
        }

        public async Task<List<ViewItemDTO>> GetAllComponentsAsync()
        {
            var components = await _componentRepo.GetAllAsync();

            return components
                .Select(c => new ViewItemDTO(
                    c.Id,
                    c.Article,
                    c.Name,
                    c.Manufacturer,
                    Guid.Empty,
                    "-",
                    "-",
                    Guid.Empty,
                    "-",
                    "-",
                    0,
                    0
            )).ToList();
        }

        public async Task ReceiveAsync(ServiceItemDTO item, string operatorName, string? comment = null)
        {
            if (item == null)
                return;

            var operation = Operation.Create(OperationType.Receipt, operatorName, comment);

            Component? component = await _componentRepo.GetByIdAsync(item.ComponentId);

            if (component is null)
                    throw new Exception($"Компонент {item.ComponentId} не найден");

            Stock? stock = await _stockRepo.GetAsync(item.RackId, item.ComponentId, item.CellId);

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
    }
}