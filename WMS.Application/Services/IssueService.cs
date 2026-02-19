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
        private readonly IOperationRepository _operationRepo;

        public IssueService(
            IStockRepository stockRepo,
            IComponentRepository componentRepo,
            ICellRepository cellRepo,
            IOperationRepository operationRepo)
        {
            _stockRepo = stockRepo;
            _componentRepo = componentRepo;
            _cellRepo = cellRepo;
            _operationRepo = operationRepo;
        }

        public async Task<List<IssueStockDto>> GetAllAsync()
        {
            var stocks = await _stockRepo.GetAllAsync(); 
            var components = await _componentRepo.GetAllAsync();
            var cells = await _cellRepo.GetAllAsync();


            return stocks
                .Join(components, s => s.ComponentId, c => c.Id, (s, c) => new { s, c })
                .Join(cells, sc => sc.s.CellId, cell => cell.Id, (sc, cell) => new IssueStockDto(
                    sc.s.ComponentId,
                    sc.s.CellId,
                    sc.c.Article,
                    sc.c.Name,
                    sc.c.Manufacturer,
                    cell.Code,
                    sc.s.Quantity,
                    0))
                .ToList();
        }

        public async Task IssueAsync(
            IReadOnlyCollection<OperationDTO> items,
            string? comment = null)
        {
            if (items.Count == 0)
                throw new Exception("Список выдачи пуст");

            //var operation = Operation.Create(OperationType.Issue, comment);

            foreach (var item in items)
            {
                var stock = await _stockRepo
                    .GetAsync(item.ComponentId, item.CellId);

                if (stock is null)
                    throw new Exception(
                        "Товар в указанной ячейке не найден");

                var before = stock.Quantity;

                stock.Issue(item.Quantity);

                await _stockRepo.UpdateAsync(stock);

                if (stock.Quantity == 0)
                    await _stockRepo.RemoveAsync(stock);

                //operation.AddItem(
                //    item.ComponentId,
                //    item.CellId,
                //    before,
                //    stock.Quantity);
            }

            //operation.Validate();
            //await _operationRepo.AddAsync(operation);
        }
    }
}
