public record StockDto(

        Guid ComponentId,
        Guid CellId,
        Guid RackId,
        string Article,
        string ComponentName,
        string Manufacturer,
        string CellCode,
        int Quantity);