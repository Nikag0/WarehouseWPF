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
    public class IssueService
    {
        private readonly IStockRepository _stockRepo;
        private readonly IHistoryRepository _operationRepo;

        public IssueService(
            IStockRepository stockRepo,
            IComponentRepository componentRepo,
            IHistoryRepository operationRepo)
        {
            _stockRepo = stockRepo;
            _operationRepo = operationRepo;
        }

        public async Task IssueAsync(
            IReadOnlyCollection<IssueItemDto> items,
            string operatorName,
            string? comment = null)
        {
            if (items.Count == 0)
                throw new BusinessException("Список выдачи пуст");

            var operation = Operation.Create(OperationType.Issue, operatorName, comment);

            foreach (var item in items)
            {
                var stock = await _stockRepo.GetByIdAsync(item.StockId);

                if (stock is null)
                    throw new BusinessException("Товар в указанной ячейке не найден");

                var before = stock.Quantity;

                stock.Issue(item.Quantity);

                await _stockRepo.UpdateAsync(stock);

                if (stock.Quantity == 0)
                    await _stockRepo.DeletAsync(stock);

                operation.AddItem(
                    stock.ComponentId,
                    stock.RackId,
                    stock.CellId,
                    before,
                    stock.Quantity);
            }

            operation.Validate();
            await _operationRepo.AddAsync(operation);
        }

        public async Task<ViewItemDTO> GetStockByIdAsync(Guid stokId)
        {
            var result = await _stockRepo.GetByIdAsync(stokId);

            return MappingExtensions.ToViewItemDto(result);
        }

        public async Task<IReadOnlyList<ViewItemDTO>> GetFilteredStockAsync(string searchText, int maxCount)
        {
            var stocks = await _stockRepo.SearchAsync(searchText, maxCount);

            return stocks.Select(MappingExtensions.ToViewItemDto).ToList();
        }

    }
}
