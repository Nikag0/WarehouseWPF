public record IssueStockDto(
     Guid ComponentId,
     Guid CellId,
     Guid RackId,

     string Article,
     string ComponentName,
     string Manufacturer,
     string CellCode,
     int Quantity,
     int IssueQuantity);