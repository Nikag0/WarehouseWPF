using System.ComponentModel;
using WMS.Domain.ExceptionControl;

namespace WMS.Domain;
public class History
{
    private readonly List<HistoryItem> _items = new();

    public Guid Id { get; private set; }
    public OperationType Type { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Operator { get; private set; }
    public string? Comment { get; private set; }

    public IReadOnlyCollection<HistoryItem> Items => _items.AsReadOnly();

    // Для EF Core
    private History() { }

    private History(OperationType type, string operatorName, string? comment)
    {
        Id = Guid.NewGuid();
        Type = type;
        OccurredAt = DateTime.UtcNow;
        Operator = operatorName;
        Comment = comment;
    }

    // Factory method
    public static History Create(OperationType type, string operatorName, string? comment = null)
    {
        if (operatorName == string.Empty || operatorName == null)
            throw new BusinessException("Имя оператора не указано");

        if (comment == null) comment = string.Empty;

        return new History(type, operatorName, comment);
    }

    // -------- Aggregate behavior --------

    public void AddItem(
        Guid componentId,
        Guid rackId,
        Guid cellId,
        int quantityBefore,
        int quantityAfter)
    {
        if (_items.Any(i =>
                i.ComponentId == componentId &&
                i.CellId == cellId))
        {
            throw new BusinessException("Операция уже содержит позицию для этого товара и ячейки");
        }

        _items.Add(new HistoryItem(
            componentId,
            rackId,
            cellId,
            quantityBefore,
            quantityAfter));
    }

    public void Validate()
    {
        if (_items.Count == 0)
            throw new BusinessException("Операция не может быть пустой");
    }
}
