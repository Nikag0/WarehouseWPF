public class OperationHistoryDto
{
    public Guid OperationId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Operator { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
}