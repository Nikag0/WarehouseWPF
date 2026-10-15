using System.Collections.ObjectModel;
using WMS.Domain;

public record OperationHystoryViewDto(
    DateTime OccurredAt, 
    string Type, 
    string ComponentName,
    string RackAndCellName, 
    int PreviousQuantity,
    string QuantityChanged,
    int NewQuantity,
    string Operator, 
    string Comment);