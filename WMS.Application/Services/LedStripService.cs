using WMS.Application.Abstractions;
using WMS.Application.DTO;

namespace WMS.Application.Services
{
    // Управление подсветкой склада: зоны ячеек на светодиодных лентах и сигнальные лампы рядов.
    public class LedStripService
    {
        private readonly ISectorRepository _repository;
        private readonly ITcpPacketSender _packetSender;
        private readonly LightingOptions _options;

        public LedStripService(ISectorRepository repository, ITcpPacketSender packetSender, LightingOptions options)
        {
            _repository = repository;
            _packetSender = packetSender;
            _options = options;
        }

        // ============ Инициализация ============

        // Отправляет контроллерам зоны подсветки ряд за рядом и сопровождает это лампами:
        // 1) все лампы гаснут;
        // 2) лампа ряда загорается жёлтым, контроллерам ряда отправляются зоны;
        // 3) в конце исправные ряды загораются зелёным и через ResultHoldTime гаснут,
        //    ряды с ошибкой остаются красными до следующей инициализации или работы с их ячейками.
        // Если контроллер не ответил, ему больше ничего не отправляется: так ошибка стоит одного таймаута, а не таймаута на каждую зону.
        public async Task<LightingInitReport> InitializeAsync(CancellationToken ct = default)
        {
            // Подсветка отключена в настройках — сообщать не о чем
            if (!_options.Enabled)
                return new LightingInitReport(isConfigured: true, Array.Empty<string>());

            var sectors = await _repository.GetAllSectorsForInitializationAsync(ct);

            // Без зон в базе к контроллерам не уйдёт ни одной команды — это не успех, а отсутствие настроек
            if (sectors.Count == 0)
                return LightingInitReport.NotConfiguredReport;

            var lamps = await _repository.GetSignalLampsAsync(ct);
            var session = new InitSession();

            // Зоны группируются по лампе ряда. Контроллеры без лампы (или с несуществующей) идут отдельной группой.
            var lampIds = lamps.Select(l => l.Id).ToHashSet();
            var sectorsByLamp = sectors.ToLookup(s =>
                s.SignalLampId is Guid id && lampIds.Contains(id) ? id : (Guid?)null);

            foreach (var lamp in lamps)
                await SetLampAsync(session, lamp, LampSignal.Off, ct);

            var failedLampIds = new HashSet<Guid>();

            foreach (var lamp in lamps)
            {
                var isLampOk = await SetLampAsync(session, lamp, LampSignal.Yellow, ct);
                await Task.Delay(_options.RowStepDelay, ct);

                var isRowOk = await SendSectorsAsync(session, sectorsByLamp[lamp.Id], $"ряд «{lamp.Name}»", ct);

                if (!isLampOk || !isRowOk)
                    failedLampIds.Add(lamp.Id);
            }

            await SendSectorsAsync(session, sectorsByLamp[null], "контроллер без лампы ряда", ct);

            // Лампы рядов в это время горят жёлтым, итоговый цвет учтёт и ошибки проверки
            if (_options.CheckSectorsOnInit)
                await CheckSectorsAsync(session, failedLampIds, ct);

            foreach (var lamp in lamps)
            {
                if (!await SetLampAsync(session, lamp, failedLampIds.Contains(lamp.Id) ? LampSignal.Red : LampSignal.Green, ct))
                    failedLampIds.Add(lamp.Id);
            }

            await Task.Delay(_options.ResultHoldTime, ct);

            foreach (var lamp in lamps.Where(l => !failedLampIds.Contains(l.Id)))
            {
                if (!await SetLampAsync(session, lamp, LampSignal.Off, ct))
                    failedLampIds.Add(lamp.Id);
            }

            var affectedRows = lamps.Where(lamp => failedLampIds.Contains(lamp.Id))
                .Select(lamp => lamp.Name)
                .Distinct()
                .ToList();

            return new LightingInitReport(isConfigured: true, session.Errors, affectedRows);
        }

        // Отправляет зоны контроллер за контроллером. Возвращает false, если хотя бы один контроллер группы не ответил.
        private async Task<bool> SendSectorsAsync(
            InitSession session,
            IEnumerable<SectorInitDTO> sectors,
            string place,
            CancellationToken ct)
        {
            var isOk = true;

            foreach (var controllerSectors in sectors.GroupBy(s => s.ControllerId))
            {
                var ordered = controllerSectors
                    .OrderBy(s => s.StripNumber)
                    .ThenBy(s => s.Index);

                foreach (var sector in ordered)
                {
                    var packet = BuildCreateSectorPacket(
                        sector.Controller.DeviceAddress,
                        sector.StripNumber,
                        sector.Index,
                        sector.StartDiode,
                        sector.EndDiode);

                    var isSent = await SendTrackedAsync(
                        session,
                        sector.Controller,
                        packet,
                        $"инициализация",
                        ct);

                    if (!isSent)
                    {
                        // Остальные зоны этого контроллера пропускаем: он уже не ответил
                        isOk = false;
                        break;
                    }
                }
            }

            return isOk;
        }

        // Цвет зоны при проверке: зелёный средней яркости
        private const byte CheckR = 0, CheckG = 200, CheckB = 0, CheckBrightness = 200;

        // Поочерёдно зажигает каждую зону и через SectorCheckDelay гасит её. Порядок — как на складе:
        // стеллажи по ряду (А, Б, …), затем по номеру; внутри стеллажа — этаж за этажом (Row), на этаже слева направо (Column).
        // Если зона не загорелась или не погасла, лампа её ряда в конце инициализации будет красной.
        private async Task CheckSectorsAsync(InitSession session, HashSet<Guid> failedLampIds, CancellationToken ct)
        {
            var sectors = await _repository.GetAllSectorsForCheckAsync(ct);

            var ordered = sectors
                .OrderBy(s => s.RackRow)
                .ThenBy(s => s.RackColumn)
                .ThenBy(s => s.CellRow)
                .ThenBy(s => s.CellColumn);

            foreach (var sector in ordered)
            {
                var place = $"проверка: стеллаж {MappingExtensions.CodeToDisplay(sector.RackColumn, sector.RackRow)}, " +
                            $"ячейка {MappingExtensions.CodeToDisplay(sector.CellColumn, sector.CellRow)}";

                var onPacket = BuildColorPacket(sector.Controller.DeviceAddress, sector.StripNumber, sector.Index,
                    CheckR, CheckG, CheckB, CheckBrightness);
                var isOk = await SendTrackedAsync(session, sector.Controller, onPacket, place, ct);

                if (isOk)
                {
                    await Task.Delay(_options.SectorCheckDelay, ct);

                    var offPacket = BuildColorPacket(sector.Controller.DeviceAddress, sector.StripNumber, sector.Index,
                        0, 0, 0, CheckBrightness);
                    isOk = await SendTrackedAsync(session, sector.Controller, offPacket, place, ct);
                }

                if (!isOk && sector.SignalLampId is Guid lampId)
                    failedLampIds.Add(lampId);
            }
        }

        private Task<bool> SetLampAsync(InitSession session, SignalLampDto lamp, LampSignal signal, CancellationToken ct)
        {
            var packet = BuildLampPacket(lamp.Controller.DeviceAddress, _options.GetLampMask(signal));
            return SendTrackedAsync(session, lamp.Controller, packet, $"лампа ряда «{lamp.Name}»", ct);
        }

        // Отправка с учётом уже не ответивших контроллеров: ошибка по каждому контроллеру попадает в отчёт один раз
        private async Task<bool> SendTrackedAsync(
            InitSession session,
            ControllerEndpoint controller,
            byte[] packet,
            string action,
            CancellationToken ct)
        {
            if (session.UnreachableControllers.Contains(controller))
                return false;

            if (await _packetSender.SendAsync(controller.Ip, controller.Port, packet, ct))
                return true;

            session.UnreachableControllers.Add(controller);
            session.Errors.Add(
                $"Контроллер {controller} не ответил или отклонил команду ({action}).");

            return false;
        }

        private sealed class InitSession
        {
            public HashSet<ControllerEndpoint> UnreachableControllers { get; } = new();
            public List<string> Errors { get; } = new();
        }

        // ============ Подсветка ячейки ============

        // Включает зону ячейки её цветом и зелёный на лампе ряда.
        // Возвращает false, если контроллер не ответил. Для ячейки без подсветки возвращает true — это не ошибка.
        public Task<bool> HighlightCellAsync(Guid cellId, CancellationToken ct = default)
            => SwitchCellAsync(cellId, isOn: true, ct);

        // Гасит зону ячейки и лампу ряда
        public Task<bool> ClearCellAsync(Guid cellId, CancellationToken ct = default)
            => SwitchCellAsync(cellId, isOn: false, ct);

        private async Task<bool> SwitchCellAsync(Guid cellId, bool isOn, CancellationToken ct)
        {
            // Подсветка отключена в настройках — выдача и приёмка идут без неё и без предупреждений
            if (!_options.Enabled)
                return true;

            var cell = await _repository.GetByCellIdAsync(cellId, ct);
            if (cell is null)
                return true;

            var sectorPacket = isOn
                ? BuildColorPacket(cell.Controller.DeviceAddress, cell.StripNumber, cell.Index, cell.R, cell.G, cell.B, cell.Brightness)
                : BuildColorPacket(cell.Controller.DeviceAddress, cell.StripNumber, cell.Index, 0, 0, 0, cell.Brightness);

            var isSectorOk = await _packetSender.SendAsync(cell.Controller.Ip, cell.Controller.Port, sectorPacket, ct);

            if (cell.Lamp is null)
                return isSectorOk;

            // Лампа на том же контроллере, который только что не ответил, — второй таймаут ждать незачем
            if (!isSectorOk && cell.Lamp == cell.Controller)
                return false;

            var lampPacket = BuildLampPacket(cell.Lamp.DeviceAddress, _options.GetLampMask(isOn ? LampSignal.Green : LampSignal.Off));
            var isLampOk = await _packetSender.SendAsync(cell.Lamp.Ip, cell.Lamp.Port, lampPacket, ct);

            return isSectorOk && isLampOk;
        }

        public async Task<bool> SetCellColorAsync(Guid cellId, byte r, byte g, byte b, CancellationToken ct = default)
        {
            return await _repository.SetColorAsync(cellId, r, g, b, ct);
        }

        // ============ Пакеты ============
        // Второй байт каждого пакета — случайный номер посылки (0–255).

        private static byte[] BuildCreateSectorPacket(
            byte deviceAddress,
            byte stripNumber,
            byte sectorIndex,
            int startDiode,
            int endDiode)
        {
            return new byte[]
            {
                deviceAddress,
                (byte)Random.Shared.Next(0, 256),
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

            return new byte[]
            {
                deviceAddress,
                (byte)Random.Shared.Next(0, 256),
                0xEF,
                stripNumber,
                sectorIndex,
                pwmR, pwmG, pwmB,
                0x00,
                0x00
            };
        }

        // gpioMask: бит 0 — pin0, бит 1 — pin1, бит 2 — pin2, бит 3 — pin3
        private static byte[] BuildLampPacket(byte deviceAddress, byte gpioMask)
        {
            return new byte[]
            {
                deviceAddress,
                (byte)Random.Shared.Next(0, 256),
                0xFC,
                gpioMask,
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
