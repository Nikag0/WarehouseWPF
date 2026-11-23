namespace WMS.Application.DTO;

public sealed record StockAvailabilityDto(ComponentEditDto Component, IReadOnlyList<StockViewDTO> Locations)
{
    public decimal Total => Locations.Sum(location => location.Quantity);
    public bool IsLow => Total <= Component.MinQuantity;
}
