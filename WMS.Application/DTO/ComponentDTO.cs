public record ComponentDTO(
    Guid Id,
    string Article,
    string Name,
    string Manufacturer,
    DateOnly? ExpirationDate,
    int MinQuantity);