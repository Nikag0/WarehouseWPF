using WMS.Domain.ExceptionControl;

namespace WMS.Domain
{
    public class Operator
    {
        // Ограничение длины фамилии, имени и отчества
        public const int NameMaxLength = 100;

        public Guid Id { get; private set; }
        public string Surname { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string Patronymic { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Operator() { } // для EF

        private Operator(Guid id, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public static Operator Create(
            string surname,
            string name,
            string patronymic)
        {
            var now = DateTime.UtcNow;

            var @operator = new Operator(Guid.NewGuid(), now, now);
            @operator.SetFullName(surname, name, patronymic);

            return @operator;
        }

        public void Update(string surname, string name, string patronymic)
        {
            SetFullName(surname, name, patronymic);

            UpdatedAt = DateTime.UtcNow;
        }

        // -------- Business rules --------

        private void SetFullName(string surname, string name, string patronymic)
        {
            // Проверка полей.
            var normalizedSurname = Normalize(surname, "Фамилия не задана", "Фамилия");
            var normalizedName = Normalize(name, "Имя не задано", "Имя");
            var normalizedPatronymic = Normalize(patronymic, "Отчество не задано", "Отчество");

            Surname = normalizedSurname;
            Name = normalizedName;
            Patronymic = normalizedPatronymic;
        }

        private static string Normalize(string value, string emptyMessage, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessException(emptyMessage);

            value = value.Trim();

            if (value.Length > NameMaxLength)
                throw new BusinessException($"{fieldName} не может быть длиннее {NameMaxLength} символов");

            return value;
        }

        public string FullName => $"{Surname} {Name[0]}. {Patronymic[0]}.";
    }
}
