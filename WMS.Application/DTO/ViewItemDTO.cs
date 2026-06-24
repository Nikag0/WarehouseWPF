public record ViewItemDTO(
        Guid ComponentId,
        string Article,
        string ComponentName,
        string Manufacturer,

        Guid StockId,

        Guid RackId,
        string RackCode,
        string RackCodeDisplay,

        Guid CellId,
        string CellCode,
        string CellCodeDisplay,

        int Quantity,
        int OperationQuantity);