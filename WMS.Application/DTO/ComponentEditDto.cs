public record ComponentEditDto(
    Guid Id,
    string Article,
    string Name,
    string Manufacturer,
    DateOnly? ExpirationDate,
    decimal MinQuantity,
    bool HasStock = false);
