namespace WMS.Application.DTO
{
    // Всё, что нужно, чтобы подсветить ячейку: её зону на ленте и лампу ряда
    public record SectorColorDTO(
        ControllerEndpoint Controller,
        byte StripNumber,
        byte Index,
        byte R, byte G, byte B,
        byte Brightness,
        // Контроллер, к которому подключена лампа ряда. null — у ряда нет лампы
        ControllerEndpoint? Lamp
    );
}
