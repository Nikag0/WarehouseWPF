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
        private readonly IOperationRepository _operationRepo;

        public IssueService(
            IStockRepository stockRepo,
            IOperationRepository operationRepo)
        {
            _stockRepo = stockRepo;
            _operationRepo = operationRepo;
        }

        public async Task IssueAsync(
            IReadOnlyCollection<StockOperationDto> items,
            string? comment = null)
        {
            if (items.Count == 0)
                throw new Exception("Список выдачи пуст");

            var operation = Operation.Create(OperationType.Issue, comment);

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

                operation.AddItem(
                    item.ComponentId,
                    item.CellId,
                    before,
                    stock.Quantity);
            }

            operation.Validate();
            await _operationRepo.AddAsync(operation);
        }
    }
}
