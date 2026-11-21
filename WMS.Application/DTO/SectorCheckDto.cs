namespace WMS.Application.DTO
{
    // Зона ячейки для поочерёдной проверки подсветки.
    // Положение стеллажа и ячейки нужно, чтобы проверять зоны в порядке их расположения на складе.
    public record SectorCheckDto(
        ControllerEndpoint Controller,
        byte StripNumber,
        byte Index,
        Guid? SignalLampId,
        int RackColumn,
        int RackRow,
        int CellColumn,
        int CellRow
    );
}
