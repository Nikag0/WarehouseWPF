using System.Collections.ObjectModel;
using WMS.Domain;

public record HistoryDto(
    DateTime OccurredAt, 
    string Operator, 
    string Type, 
    string ComponentName, 
    int QuantityBefore, 
    int QuantityAfter);