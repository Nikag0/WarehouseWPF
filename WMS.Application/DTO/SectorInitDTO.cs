namespace WMS.Application.DTO
{
    // Зона подсветки ячейки для отправки контроллеру при инициализации
    public record SectorInitDTO(
        Guid ControllerId,
        ControllerEndpoint Controller,
        byte StripNumber,
        byte Index,
        int StartDiode,
        int EndDiode,
        // Лампа ряда, к которой относится контроллер. null — у ряда нет лампы
        Guid? SignalLampId
    );
}
