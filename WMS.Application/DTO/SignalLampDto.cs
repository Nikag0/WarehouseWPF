namespace WMS.Application.DTO
{
    // Сигнальная лампа ряда и контроллер, к GPIO которого она подключена
    public record SignalLampDto(Guid Id, string Name, ControllerEndpoint Controller);
}
