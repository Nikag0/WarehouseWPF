using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;
using WMS.Domain.ExceptionControl;

namespace WMS.Application.Services
{
    public class IssueService
    {
        private readonly IStockRepository _stockRepo;
        private readonly IOperationRepository _operationRepo;

        public IssueService(
            IStockRepository stockRepo,
            IComponentRepository componentRepo,
            IRackRepository rackRepo,
            IOperationRepository operationRepo)
        {
            _stockRepo = stockRepo;
            _operationRepo = operationRepo;
        }

        public async Task IssueAsync(
            IReadOnlyCollection<ServiceItemDTO> items,
            string operatorName,
            string? comment = null)
        {
            if (items.Count == 0)
                throw new BusinessException("Список выдачи пуст");

            var operation = Operation.Create(OperationType.Issue, operatorName, comment);

            foreach (var item in items)
            {
                var stock = await _stockRepo.GetStockAsync(item.Stockid);

                if (stock is null)
                    throw new BusinessException("Товар в указанной ячейке не найден");

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
