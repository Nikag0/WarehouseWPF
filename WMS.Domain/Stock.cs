using WMS.Domain.ExceptionControl;

namespace WMS.Domain;
public class Stock
{
    public Guid Id { get; private set; }
    public Guid ComponentId { get; private set; }
    public Guid RackId { get; private set; }
    public Guid CellId { get; private set; }
    public decimal Quantity { get; private set; }

    // Навигационные свойства.
    public Component Component { get; private set; } = null!;
    public Rack Rack { get; private set; } = null!;
    public Cell Cell { get; private set; } = null!;

    // Для EF Core
    private Stock() { }

    private Stock(Guid id, Guid componentId, Guid rackId, Guid cellId, decimal quantity)
    {
        Id = id;
        ComponentId = componentId;
        RackId = rackId;
        CellId = cellId;
        SetInitialQuantity(quantity);
    }

    // Factory method
    public static Stock Create(
        Guid componentId,
        Guid rackId,
        Guid cellId,
        decimal initialQuantity)
    {
        if (componentId == Guid.Empty)
            throw new BusinessException("Компонент не указан");

        if (rackId == Guid.Empty)
            throw new BusinessException("Стеллаж не указан");

        if (cellId == Guid.Empty)
            throw new BusinessException("Ячейка не указана");
        

        return new Stock(
            Guid.NewGuid(),
            componentId,
            rackId,
            cellId,
            initialQuantity);
    }

    // -------- Business logic --------

    public void Receive(decimal quantity)
    {
        ValidatePositiveQuantity(quantity);
        Quantity += quantity;
    }

    public void Issue(decimal quantity)
    {
        ValidatePositiveQuantity(quantity);

        if (quantity > Quantity)
            throw new BusinessException("Недостаточно товара в ячейке");

        Quantity -= quantity;
    }

    // -------- Helpers --------

    private void SetInitialQuantity(decimal quantity)
    {
        if (quantity < 0)
            throw new BusinessException("Начальное количество не может быть отрицательным");

        QuantityRules.EnsureValid(quantity);

        Quantity = quantity;
    }

    private static void ValidatePositiveQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("Количество должно быть больше нуля");

        QuantityRules.EnsureValid(quantity);
    }
}