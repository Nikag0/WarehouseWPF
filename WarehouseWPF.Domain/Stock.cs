namespace WarehouseWPF.Domain;

public class Stock
{
    public Guid Id { get; private set; }

    public Guid ComponentId { get; private set; }
    public Guid CellId { get; private set; }

    public int Quantity { get; private set; }

    // Для EF Core
    private Stock() { }

    private Stock(Guid id, Guid componentId, Guid cellId, int quantity)
    {
        Id = id;
        ComponentId = componentId;
        CellId = cellId;
        SetInitialQuantity(quantity);
    }

    // Factory method
    public static Stock Create(
        Guid componentId,
        Guid cellId,
        int initialQuantity)
    {
        if (componentId == Guid.Empty)
            throw new Exception("ComponentId не задан");

        if (cellId == Guid.Empty)
            throw new Exception("CellId не задан");

        return new Stock(
            Guid.NewGuid(),
            componentId,
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
            throw new Exception("Недостаточно товара в ячейке");

        Quantity -= quantity;
    }

    public void Inventory(int actualQuantity)
    {
        if (actualQuantity < 0)
            throw new Exception("Фактическое количество не может быть отрицательным");

        Quantity = actualQuantity;
    }

    // -------- Helpers --------

    private void SetInitialQuantity(int quantity)
    {
        if (quantity < 0)
            throw new Exception("Начальное количество не может быть отрицательным");

        Quantity = quantity;
    }

    private static void ValidatePositiveQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Количество должно быть больше нуля");
    }
}