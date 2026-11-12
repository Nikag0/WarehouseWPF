using WMS.Domain.ExceptionControl;

namespace WMS.Domain;

public class Component
{
    // Ограничение длины артикула, названия и производителя
    public const int TextMaxLength = 100;

    public Guid Id { get; private set; }
    public string Article { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Manufacturer { get; private set; } = null!;
    public DateOnly? ExpirationDate { get; private set; }
    public int MinQuantity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Для EF Core
    private Component() { }

    private Component(Guid id, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    // Factory method
    public static Component Create(
        string article,
        string name,
        string manufacturer,
        DateOnly? expirationDate,
        int minQuantity)
    {
        var now = DateTime.UtcNow;

        var component = new Component(Guid.NewGuid(), now, now);
        component.SetFields(article, name, manufacturer, expirationDate, minQuantity);

        return component;
    }

    public void Update(
       string article,
       string name,
       string manufacturer,
       DateOnly? expirationDate,
       int minQuantity)
    {
        SetFields(article, name, manufacturer, expirationDate, minQuantity);

        UpdatedAt = DateTime.UtcNow;
    }

    // -------- Business rules --------

    private void SetFields(
        string article,
        string name,
        string manufacturer,
        DateOnly? expirationDate,
        int minQuantity)
    {
        // Сначала проверяем все поля, чтобы при ошибке объект остался без изменений
        var normalizedArticle = Normalize(article, "Артикул не может быть пустым", "Артикул");
        var normalizedName = Normalize(name, "Название не может быть пустым", "Название");
        var normalizedManufacturer = Normalize(manufacturer, "Производитель не может быть пустым", "Производитель");

        if (minQuantity <= 0)
            throw new BusinessException("Минимальный остаток не может быть отрицательным или равным нулю");

        Article = normalizedArticle;
        Name = normalizedName;
        Manufacturer = normalizedManufacturer;
        ExpirationDate = expirationDate;
        MinQuantity = minQuantity;
    }

    private static string Normalize(string value, string emptyMessage, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessException(emptyMessage);

        value = value.Trim();

        if (value.Length > TextMaxLength)
            throw new BusinessException($"{fieldName} не может быть длиннее {TextMaxLength} символов");

        return value;
    }
}
