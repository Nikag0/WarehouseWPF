namespace WMS.Domain.LedStrip
{
    // Сигнальная лампа ряда стеллажей. Физически подключена к GPIO одного контроллера (Microcontroller),
    // а относиться к ней могут несколько контроллеров (Microcontroller.SignalLamp): например, два соседних ряда с общей лампой.
    // Данные заполняются вручную командой разработки, поэтому у сущности нет публичного конструктора.
    public class SignalLamp
    {
        public const int NameMaxLength = 50;

        public Guid Id { get; private set; }

        // Название для отчётов об ошибках, например «Ряд А» или «Ряды 2–3»
        public string Name { get; private set; } = null!;

        // Контроллер, к GPIO которого подключена лампа
        public Guid MicrocontrollerId { get; private set; }
        public Microcontroller Microcontroller { get; private set; } = null!;

        // Контроллеры, ячейки которых сигнализируют этой лампой
        public ICollection<Microcontroller> AssignedControllers { get; private set; } = new List<Microcontroller>();

        private SignalLamp() { }
    }
}
