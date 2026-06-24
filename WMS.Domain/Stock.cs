using WMS.Domain.ExceptionControl;

namespace WMS.Domain;
public class Stock
{
    public Guid Id { get; private set; }
    public Guid ComponentId { get; private set; }
    public Guid RackId { get; private set; }
    public Guid CellId { get; private set; }
    public int Quantity { get; private set; }

    // Навигационные свойства.
    public Component Component { get; private set; } = null!;
    public Rack Rack { get; private set; } = null!;
    public Cell Cell { get; private set; } = null!;

    // Для EF Core
    private Stock() { }

    private Stock(Guid id, Guid componentId, Guid rackId, Guid cellId, int quantity)
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
        int initialQuantity)
    {
        if (componentId == Guid.Empty)
            throw new BusinessException("ComponentId не задан");

        if (rackId == Guid.Empty)
            throw new BusinessException("RackId не задан");

        if (cellId == Guid.Empty)
            throw new BusinessException("CellId не задан");
        

        return new Stock(
            Guid.NewGuid(),
            componentId,
            rackId,
            cellId,
            initialQuantity);
    }

    // -------- Business logic --------

    public void Receive(int quantity)
    {
        ValidatePositiveQuantity(quantity);
        Quantity += quantity;
    }

    public void Issue(int quantity)
    {
        ValidatePositiveQuantity(quantity);

        if (quantity > Quantity)
            throw new BusinessException("Недостаточно товара в ячейке");

        Quantity -= quantity;
    }

    public void Inventory(int actualQuantity)
    {
        if (actualQuantity < 0)
            throw new BusinessException("Фактическое количество не может быть отрицательным");

        Quantity = actualQuantity;
    }

    // -------- Helpers --------

    private void SetInitialQuantity(int quantity)
    {
        if (quantity < 0)
            throw new BusinessException("Начальное количество не может быть отрицательным");

        Quantity = quantity;
    }

    private static void ValidatePositiveQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("Количество должно быть больше нуля");
    }
}