public record StockViewDto(
        Guid ComponentId,
        string Article,
        string ComponentName,
        string Manufacturer,

        Guid RackId,
        string RackCode,

        Guid CellId,
        string CellCode,

        int Quantity,
        int OperationQuantity);