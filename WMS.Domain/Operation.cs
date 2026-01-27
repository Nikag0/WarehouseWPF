namespace WMS.Domain;
public class Operation
{
    private readonly List<OperationItem> _items = new();

    public Guid Id { get; private set; }
    public OperationType Type { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? Comment { get; private set; }

    public IReadOnlyCollection<OperationItem> Items => _items.AsReadOnly();

    // Для EF Core
    private Operation() { }

    private Operation(OperationType type, string? comment)
    {
        Id = Guid.NewGuid();
        Type = type;
        OccurredAt = DateTime.UtcNow;
        Comment = comment;
    }

    // Factory method
    public static Operation Create(OperationType type, string? comment = null)
    {
        return new Operation(type, comment);
    }

    // -------- Aggregate behavior --------

    public void AddItem(
        Guid componentId,
        Guid cellId,
        int quantityBefore,
        int quantityAfter)
    {
        if (_items.Any(i =>
                i.ComponentId == componentId &&
                i.CellId == cellId))
        {
            throw new ("Операция уже содержит позицию для этого товара и ячейки");
        }

        _items.Add(new OperationItem(
            componentId,
            cellId,
            quantityBefore,
            quantityAfter));
    }

    public void Validate()
    {
        if (_items.Count == 0)
            throw new Exception("Операция не может быть пустой");
    }
}
