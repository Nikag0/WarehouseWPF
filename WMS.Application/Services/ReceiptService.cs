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
            IOperationRepository operationRepo)
        {
            _componentRepo = componentRepo;
            _stockRepo = stockRepo;
            _operationRepo = operationRepo;
        }

        public async Task ReceiveAsync(ReceiptItemDto item)
        {
            if (item == null)
                return;

            Component? component = await _componentRepo.GetByIdAsync(item.ComponentId);

            if (component is null)
                    throw new Exception($"Компонент {item.ComponentId} не найден");

            // 2. Получаем или создаём остаток
            Stock? stock = await _stockRepo.GetAsync(item.ComponentId, item.CellId);

            if (stock is null)
            {
                stock = Stock.Create(item.ComponentId, item.CellId, 0);

                stock.Receive(item.Quantity);
                await _stockRepo.AddAsync(stock);
            }
            else
            {
                stock.Receive(item.Quantity);
                await _stockRepo.UpdateAsync(stock);
            }
        }


        // Расширенный метод с фиксированием истории операций и добавление списка приёмки.
        //public async Task ReceiveAsync(
        //    IReadOnlyCollection<ReceiptItemDto> items,
        //    string? comment = null)
        //{
        //    if (items.Count == 0)
        //        throw new Exception("Список приёмки пуст");

        //    var operation = Operation.Create(OperationType.Receipt, comment);

        //    foreach (var item in items)
        //    {
        //        // 1. Проверяем, что компонент существует
        //        Component? component = await _componentRepo
        //            .GetByIdAsync(item.ComponentId);

        //        if (component is null)
        //            throw new Exception(
        //                $"Компонент {item.ComponentId} не найден");

        //        // 2. Получаем или создаём остаток
        //        var stock = await _stockRepo
        //            .GetAsync(item.ComponentId, item.CellId);

        //        int before;

        //        if (stock is null)
        //        {
        //            stock = Stock.Create(
        //                item.ComponentId,
        //                item.CellId,
        //                0);

        //            before = 0;
        //            stock.Receive(item.Quantity);
        //            await _stockRepo.AddAsync(stock);
        //        }
        //        else
        //        {
        //            before = stock.Quantity;
        //            stock.Receive(item.Quantity);
        //            await _stockRepo.UpdateAsync(stock);
        //        }

        //        // 3. Фиксируем операцию
        //        operation.AddItem(
        //            item.ComponentId,
        //            item.CellId,
        //            before,
        //            stock.Quantity);
        //    }

        //    // 4. Валидация агрегата
        //    operation.Validate();

        //    // 5. Сохраняем историю
        //    await _operationRepo.AddAsync(operation);
        //}
    }
}
