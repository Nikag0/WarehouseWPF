public record ReceiptStockDto(
        Guid ComponentId,
        Guid RackId,
        Guid CellId,

        string Article,
        string ComponentName,
        string Manufacturer,
        string CellCode,
        int Quantity);