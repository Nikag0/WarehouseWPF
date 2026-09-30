using System.ComponentModel;
using WMS.Domain.ExceptionControl;
using WMS.Domain.Interfaces;

namespace WMS.Domain;

public class Component : ISoftDeletable
{
    public Guid Id { get; private set; }
    public string Article { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Manufacturer { get; private set; } = null!;
    public DateOnly? ExpirationDate { get; private set; }
    public int MinQuantity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    // Для EF Core
    private Component() { }

    private Component(
        Guid id,
        string article,
        string name,
        string manufacturer,
        DateOnly? expirationDate,
        int minQuantity,
        DateTime createdAt,
        DateTime updatedAt)
    {
        Id = id;

        CreatedAt = createdAt;
        UpdatedAt = updatedAt;

        SetArticle(article);
        SetName(name);
        SetManufacturer(manufacturer);
        ExpirationDate = expirationDate;
        SetMinQuantity(minQuantity);
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

        return new Component(
            Guid.NewGuid(),
            article,
            name,
            manufacturer,
            expirationDate,
            minQuantity,
            now,
            now);
    }

    public void Update(
       string article,
       string name,
       string manufacturer,
       DateOnly? expirationDate,
       int minQuantity)
    {
        UpdatedAt = DateTime.UtcNow;

        SetArticle(article);
        SetName(name);
        SetManufacturer(manufacturer);
        ExpirationDate =  expirationDate;
        SetMinQuantity(minQuantity);

    }

    // -------- Business rules --------

    private void SetArticle(string article)
    {
        if (string.IsNullOrWhiteSpace(article))
            throw new BusinessException("Артикул не может быть пустым");

        Article = article.Trim();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Название не может быть пустым");

        Name = name.Trim();
    }

    private void SetManufacturer(string manufacturer)
    {
        if (string.IsNullOrWhiteSpace(manufacturer))
            throw new BusinessException("Производитель не может быть пустым");

        Manufacturer = manufacturer.Trim();
    }

    // Не протестировано, т. к. не используется.
    private void SetExpirationDate(DateOnly expirationDate)
    {
        if (expirationDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new BusinessException("Срок годности не может быть в прошлом");
        }

        ExpirationDate = expirationDate;
    }

    private void SetMinQuantity(int minQuantity)
    {
        if (minQuantity <= 0)
            throw new BusinessException("Минимальный остаток не может быть отрицательным или равным нулю");

        MinQuantity = minQuantity;
    }

    public void Delete()
    {
        if (IsDeleted) return;

        IsDeleted = true;
    }
}