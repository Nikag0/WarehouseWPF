using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class IssueService
    {
        private readonly IStockRepository _stockRepo;
        private readonly IComponentRepository _componentRepo;
        private readonly ICellRepository _cellRepo;
        private readonly IRackRepository _rackRepo;
        private readonly IOperationRepository _operationRepo;

        public IssueService(
            IStockRepository stockRepo,
            IComponentRepository componentRepo,
            IRackRepository rackRepo,
            ICellRepository cellRepo,
            IOperationRepository operationRepo)
        {
            _stockRepo = stockRepo;
            _componentRepo = componentRepo;
            _rackRepo = rackRepo;
            _cellRepo = cellRepo;
            _operationRepo = operationRepo;
        }

        public async Task<List<ViewItemDTO>> GetAllAsync()
        {
            var stocks = await _stockRepo.GetAllAsync(); 
            var components = await _componentRepo.GetAllAsync();
            var racks = await _rackRepo.GetAllAsync();
            var cells = await _cellRepo.GetAllAsync();

            var result =
                from s in stocks
                join c in components on s.ComponentId equals c.Id
                join rack in racks on s.RackId equals rack.Id
                join cell in cells on s.CellId equals cell.Id
                select new ViewItemDTO(
                    s.ComponentId,
                    c.Article,
                    c.Name,
                    c.Manufacturer,
                    s.RackId,
                    rack.RackCode,
                    LocationFormatter.CodeToDisplay(rack.Column, rack.Row),
                    s.CellId,
                    cell.CellCode,
                    LocationFormatter.CodeToDisplay(cell.Column, cell.Row),
                    s.Quantity,
                    0);

            return result.ToList();
        }

        public async Task IssueAsync(
            IReadOnlyCollection<ServiceItemDTO> items,
            string operatorName,
            string? comment = null)
        {
            if (items.Count == 0)
                throw new Exception("Список выдачи пуст");

            var operation = Operation.Create(OperationType.Issue, operatorName, comment);

            foreach (var item in items)
            {
                var stock = await _stockRepo
                    .GetAsync(item.RackId, item.ComponentId, item.CellId);

                if (stock is null)
                    throw new Exception(
                        "Товар в указанной ячейке не найден");

                var before = stock.Quantity;

                stock.Issue(item.Quantity);

                await _stockRepo.UpdateAsync(stock);

                if (stock.Quantity == 0)
                    await _stockRepo.RemoveAsync(stock);

                operation.AddItem(
                    item.ComponentId,
                    item.RackId,
                    item.CellId,
                    before,
                    stock.Quantity);
            }

            operation.Validate();
            await _operationRepo.AddAsync(operation);
        }
    }
}
