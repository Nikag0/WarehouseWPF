public record ReceiptItemDto
(
    Guid ComponentId,
    Guid RackId,
    string RackName,
    Guid CellId,
    string CellName,
    decimal Quantity
);