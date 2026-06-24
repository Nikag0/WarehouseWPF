using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class InventoryService
    {
        private readonly IStockRepository _stockRepo;
        private readonly IOperationRepository _operationRepo;

        public InventoryService(
            IStockRepository stockRepo,
            IOperationRepository operationRepo)
        {
            _stockRepo = stockRepo;
            _operationRepo = operationRepo;
        }

        //public async Task InventoryAsync(
        //    IReadOnlyCollection<ServiceItemDTO> items,
        //    string? comment = null)
        //{
        //    if (items.Count == 0)
        //        throw new Exception("Список инвентаризации пуст");

        //    var operation = Operation.Create(OperationType.Inventory, comment);

        //    foreach (var item in items)
        //    {
        //        var stock = await _stockRepo
        //            .GetAsync(item.RackId, item.ComponentId, item.CellId);

        //        int before;

        //        if (stock is null)
        //        {
        //            stock = Stock.Create(
        //                item.ComponentId,
        //                item.CellId,
        //                item.RackId,
        //                item.Quantity);

        //            before = 0;
        //            await _stockRepo.AddAsync(stock);
        //        }
        //        else
        //        {
        //            before = stock.Quantity;
        //            stock.Inventory(item.Quantity);
        //            await _stockRepo.UpdateAsync(stock);
        //        }

        //        operation.AddItem(
        //            item.ComponentId,
        //            item.RackId,
        //            item.CellId,
        //            before,
        //            stock.Quantity);
        //    }

        //    operation.Validate();
        //    await _operationRepo.AddAsync(operation);
        //}
    }
}
