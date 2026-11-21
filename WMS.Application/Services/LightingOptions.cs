namespace WMS.Application.Services
{
    // Настройки подсветки. Единственный источник значений — секция Lighting в appsettings.json
    // (читается в App.LoadLightingOptions). Значений по умолчанию нет: если настройки не хватает, программа не запустится
    // и подскажет, что дописать.
    public sealed class LightingOptions
    {
        // false — программа работает без подсветки: контроллерам ничего не отправляется, ошибок подсветки не показывается
        public required bool Enabled { get; init; }

        // Сколько ждать ответа контроллера
        public required TimeSpan TcpTimeout { get; init; }

        // Пауза после того, как лампа ряда загорелась жёлтым, — чтобы переход от ряда к ряду был виден глазом
        public required TimeSpan RowStepDelay { get; init; }

        // Сколько держать итоговый цвет (зелёный или красный) в конце инициализации, прежде чем погасить зелёные лампы
        public required TimeSpan ResultHoldTime { get; init; }

        // Проверять ли в конце инициализации каждую зону ячейки: зона загорается, через SectorCheckDelay гаснет.
        // Порядок — по расположению на складе (см. LedStripService.CheckSectorsAsync).
        public required bool CheckSectorsOnInit { get; init; }

        // Сколько горит каждая зона при проверке
        public required TimeSpan SectorCheckDelay { get; init; }

        // Состояние пинов GPIO для каждого сигнала лампы
        public required IReadOnlyDictionary<LampSignal, byte> LampMasks { get; init; }

        public byte GetLampMask(LampSignal signal)
        {
            if (!LampMasks.TryGetValue(signal, out var mask))
                throw new InvalidOperationException($"Для сигнала лампы {signal} не задано состояние пинов (Lighting:LampSignals:{signal}).");

            return mask;
        }

        // Строка из 4 символов «0»/«1» — состояние pin0, pin1, pin2, pin3 слева направо.
        // Например, «0101»: pin0 = 0, pin1 = 1, pin2 = 0, pin3 = 1.
        public static byte ParseLampMask(string pins, LampSignal signal)
        {
            if (pins is null || pins.Length != 4 || pins.Any(c => c is not ('0' or '1')))
            {
                throw new InvalidOperationException(
                    $"Настройка Lighting:LampSignals:{signal} = «{pins}» задана неверно. " +
                    "Нужно 4 символа «0» или «1» — состояние pin0…pin3, например «0101».");
            }

            byte mask = 0;
            for (var pin = 0; pin < 4; pin++)
            {
                if (pins[pin] == '1')
                    mask |= (byte)(1 << pin);
            }

            return mask;
        }
    }
}
