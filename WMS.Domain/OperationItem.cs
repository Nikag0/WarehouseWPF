using WMS.Domain.ExceptionControl;

namespace WMS.Domain;

public class OperationItem
{
    public Guid Id { get; private set; }

    public Guid ComponentId { get; private set; }
    public Guid CellId { get; private set; }

    public int QuantityBefore { get; private set; }
    public int QuantityAfter { get; private set; }

    // Для EF Core
    private OperationItem() { }

    internal OperationItem(
        Guid componentId,
        Guid cellId,
        int quantityBefore,
        int quantityAfter)
    {
        if (componentId == Guid.Empty)
            throw new OverallDomainException("ComponentId не задан");

        if (cellId == Guid.Empty)
            throw new OverallDomainException("CellId не задан");

        if (quantityBefore < 0 || quantityAfter < 0)
            throw new OverallDomainException("Количество не может быть отрицательным");

        Id = Guid.NewGuid();
        ComponentId = componentId;
        CellId = cellId;
        QuantityBefore = quantityBefore;
        QuantityAfter = quantityAfter;
    }
}
