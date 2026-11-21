namespace WMS.Application.Services
{
    // Сигналы лампы ряда. Какие пины GPIO включает каждый сигнал, задаётся в appsettings.json (Lighting:LampSignals)
    public enum LampSignal
    {
        // Лампа погашена
        Off,

        // Идёт инициализация ряда
        Yellow,

        // Ряд готов к работе / в ряду есть ячейка для выдачи или приёмки
        Green,

        // Ряд не прошёл инициализацию
        Red
    }
}
