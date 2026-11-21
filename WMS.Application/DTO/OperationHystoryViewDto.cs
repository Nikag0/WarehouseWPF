using System.Collections.ObjectModel;
using WMS.Domain;

public record OperationHystoryViewDto(
    DateTime OccurredAt, 
    string Type, 
    string ComponentName,
    string RackAndCellName,
    decimal PreviousQuantity,
    string QuantityChanged,
    decimal NewQuantity,
    string Operator, 
    string Comment);