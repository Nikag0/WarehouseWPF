public record StockOperationDto(
    Guid ComponentId,
    Guid CellId,
    int Quantity
);