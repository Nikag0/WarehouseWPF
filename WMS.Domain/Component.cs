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
    public bool IsDeleted { get; private set; }

    public void Delete()
    {
        if (IsDeleted) return; 

        // Здесь можно прописать дополнительные бизнес-правила перед удалением, например:
        // if (CurrentQuantity > 0) throw new OverallDomainException("Нельзя удалить компонент, пока он есть на складе");

        IsDeleted = true;
    }

    // Для EF Core
    private Component() { }

    private Component(
        Guid id,
        string article,
        string name,
        string manufacturer,
        DateOnly? expirationDate,
        int minQuantity)
    {
        Id = id;
        SetArticle(article);
        SetName(name);
        SetManufacturer(manufacturer);
        SetExpirationDate(expirationDate);
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
        return new Component(
            Guid.NewGuid(),
            article,
            name,
            manufacturer,
            expirationDate,
            minQuantity);
    }

    // -------- Business rules --------

    public void SetArticle(string article)
    {
        if (string.IsNullOrWhiteSpace(article))
            throw new BusinessException("Артикул не может быть пустым");

        Article = article.Trim();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Название не может быть пустым");

        Name = name.Trim();
    }

    public void SetManufacturer(string manufacturer)
    {
        if (string.IsNullOrWhiteSpace(manufacturer))
            throw new BusinessException("Производитель не может быть пустым");

        Manufacturer = manufacturer.Trim();
    }

    public void SetExpirationDate(DateOnly? expirationDate)
    {
        if (expirationDate.HasValue &&
            expirationDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new BusinessException("Срок годности не может быть в прошлом");
        }

        ExpirationDate = expirationDate;
    }

    public void SetMinQuantity(int minQuantity)
    {
        if (minQuantity < 0)
            throw new BusinessException("Минимальный остаток не может быть отрицательным");

        MinQuantity = minQuantity;
    }
}