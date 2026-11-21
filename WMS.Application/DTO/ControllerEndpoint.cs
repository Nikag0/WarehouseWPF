namespace WMS.Application.DTO
{
    // Адрес контроллера подсветки в сети и на шине: куда и кому отправлять пакет
    public record ControllerEndpoint(string Ip, int Port, byte DeviceAddress)
    {
        public override string ToString() => $"{Ip}:{Port} (адрес {DeviceAddress})";
    }
}
