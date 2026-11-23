using WMS.Application.DTO;

namespace WMS.Desktop.ViewModels.MenuViewModels;

internal static class StockLocationOrder
{
    // Ключ сортировки: ряд стеллажа (буква), номер стеллажа, полка (буква), номер ячейки.
    internal static (int RackRow, int RackNumber, int Shelf, int CellNumber) GetPlaceKey(StockViewDTO stock)
    {
        var (rackNumber, rackRow) = ParsePlaceCode(stock.RackCode);
        var (cellNumber, shelf) = ParsePlaceCode(stock.CellCode);

        return (rackRow, rackNumber, shelf, cellNumber);
    }

    // RackCode и CellCode имеют вид «номер-ряд» числами (см. MappingExtensions.StockToView): «2-1» — это А-2.
    // Код, который не удалось разобрать, уходит в конец списка.
    private static (int Number, int Row) ParsePlaceCode(string code)
    {
        var parts = code?.Split('-');

        return parts is { Length: 2 } && int.TryParse(parts[0], out var number) && int.TryParse(parts[1], out var row)
            ? (number, row)
            : (int.MaxValue, int.MaxValue);
    }
}
