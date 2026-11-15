using System.Net.NetworkInformation;
using WMS.Application.Abstractions;

namespace WMS.Application.Services
{
    public class LedStripService
    {
        private readonly ISectorRepository _repository;
        private readonly ITcpPacketSender _packetSender;

        public LedStripService(ISectorRepository repository, ITcpPacketSender packetSender)
        {
            _repository = repository;
            _packetSender = packetSender;
        }

        public async Task<ControllersInitResult> InitializeSectorsAsync(CancellationToken ct = default)
        {
            var sectors = await _repository.GetAllSectorsForInitializationAsync(ct);

            // Без зон в базе к контроллерам не уйдёт ни одной команды — это не успех, а отсутствие настроек
            if (sectors.Count == 0)
                return ControllersInitResult.NotConfigured;

            foreach (var sector in sectors)
            {
                var packet = BuildCreateSectorPacket(
                    sector.DeviceAddress,
                    sector.StripNumber,
                    sector.Index,
                    sector.StartDiode,
                    sector.EndDiode
                );

                var success = await _packetSender.SendAsync(
                    sector.Ip,
                    sector.Port,
                    packet,
                    ct
                );

                if (!success)
                {
                    // Можно логировать: какой сектор не создался
                    return ControllersInitResult.Failed;
                }
            }

            return ControllersInitResult.Success;
        }

        public async Task<bool> TurnOnSectorAsync(Guid cellId, CancellationToken ct = default)
        {
            var sector = await _repository.GetByCellIdAsync(cellId, ct);
            if (sector is null) return true; // Для ячейки не настроена подсветка — это не ошибка

            var packet = BuildColorPacket(
              sector.DeviceAddress,
              sector.StripNumber,
              sector.Index,
              sector.R, sector.G, sector.B,
              sector.Brightness
            );

            return await _packetSender.SendAsync(
                    sector.Ip,
                    sector.Port,
                    packet,
                    ct
                );
        }

        public async Task<bool> TurnOffSectorAsync(Guid cellId, CancellationToken ct = default)
        {
            var sector = await _repository.GetByCellIdAsync(cellId, ct);
            if (sector is null) return true; // Для ячейки не настроена подсветка — это не ошибка

            var packet = BuildColorPacket(
              sector.DeviceAddress,
              sector.StripNumber,
              sector.Index,
              0, 0, 0,
              sector.Brightness
            );

            return await _packetSender.SendAsync(
                    sector.Ip,
                    sector.Port,
                    packet,
                    ct
                );
        }

        public async Task<bool> SetCellColorAsync(Guid cellId, byte r, byte g, byte b, CancellationToken ct = default)
        {
            return await _repository.SetColorAsync(cellId, r, g, b, ct);
        }

        public async Task AskGPIOAsync(Guid cellId, CancellationToken ct = default)
        {
            var sector = await _repository.GetByCellIdAsync(cellId, ct);
            if (sector is null) return;

            var packet = BuildAskGPIO(sector.DeviceAddress);

            var success = await _packetSender.SendAsync(
                    sector.Ip,
                    sector.Port,
                    packet,
                    ct
            );
        }

        public async Task<bool> SetGPIOAsync(Guid cellId, int active, CancellationToken ct = default)
        {
            var sector = await _repository.GetByCellIdAsync(cellId, ct);
            if (sector is null) return true; // Для ячейки не настроена подсветка — это не ошибка

            var packet = BuildSetGPIO(sector.DeviceAddress, active, active, active, active);

            return await _packetSender.SendAsync(
                    sector.Ip,
                    sector.Port,
                    packet,
                    ct
                );
        }

        // ============ Пакеты ============

        private static byte[] BuildCreateSectorPacket(
            byte deviceAddress,
            byte stripNumber,
            byte sectorIndex,
            int startDiode,
            int endDiode)
        {

            var random = new Random();

            return new byte[]
            {
            deviceAddress,
            (byte)random.Next(0, 255),
            0xF0,
            stripNumber,
            sectorIndex,
            LowByte(startDiode),
            HighByte(startDiode),
            LowByte(endDiode),
            HighByte(endDiode),
            0x00
            };
        }

        private static byte[] BuildColorPacket(
            byte deviceAddress,
            byte stripNumber,
            byte sectorIndex,
            byte r, byte g, byte b,
            byte brightness)
        {
            var (pwmR, pwmG, pwmB) = CalculatePwm(r, g, b, brightness);
            var random = new Random();

            return new byte[]
            {
                deviceAddress,
                (byte)random.Next(0, 255),
                0xEF,
                stripNumber,
                sectorIndex,
                pwmR, pwmG, pwmB,
                0x00,
                0x00
            };
        }

        private static byte[] BuildAskGPIO(byte deviceAddress)
        {
            var random = new Random();

            return new byte[]
            {
                deviceAddress,
                (byte)random.Next(0, 255),
                0xFD,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00
            };
        }

        private static byte[] BuildSetGPIO(byte deviceAddress, int pin0, int pin1, int pin2, int pin3)
        {
            var random = new Random();
            byte gpioByte = (byte)((pin0 & 1) | ((pin1 & 1) << 1) | ((pin2 & 1) << 2) | ((pin3 & 1) << 3));

            return new byte[]
            {
                deviceAddress,
                (byte)random.Next(0, 255),
                0xFC,
                gpioByte,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00
            };
        }

        private static byte LowByte(int value) => (byte)(value & 0xFF);
        private static byte HighByte(int value) => (byte)((value >> 8) & 0xFF);

        // Гамма-коррекция: глаз воспринимает яркость нелинейно, поэтому значение ШИМ возводится в степень 2.8.
        // Таблица вычисляется, а не набирается руками: в ней всегда ровно 256 значений, на каждое возможное значение байта.
        private const double Gamma = 2.8;

        private static readonly byte[] Gamma8 = Enumerable.Range(0, 256)
            .Select(i => (byte)Math.Round(Math.Pow(i / 255.0, Gamma) * 255))
            .ToArray();

        public static (byte R, byte G, byte B) CalculatePwm(
            byte r, byte g, byte b, byte masterBrightness)
        {
            byte rLin = (byte)((r * masterBrightness) >> 8);
            byte gLin = (byte)((g * masterBrightness) >> 8);
            byte bLin = (byte)((b * masterBrightness) >> 8);

            return (
                Gamma8[rLin],
                Gamma8[gLin],
                Gamma8[bLin]
            );
        }
    }
}
