using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WMS.Application.Abstractions;
using WMS.Application.DTO;
using WMS.Application.Services;
using WMS.Desktop.Models;
using WMS.Desktop.Services;

namespace WMS.Desktop.ViewModels.MenuViewModels;

public partial class StockOverviewViewModel(
    IStockService stockService,
    WarehouseService warehouseService,
    WarehouseVisualizationService visualizationService,
    DialogService dialogs) : ObservableObject
{
    private IReadOnlyList<StockAvailabilityDto> _availability = [];

    [ObservableProperty] private IReadOnlyList<StockOverviewRow> _stocks = [];
    [ObservableProperty] private ObservableCollection<RackViewModel> _racksGrid = [];
    [ObservableProperty] private ObservableCollection<CellViewModel> _cellsGrid = [];
    [ObservableProperty] private RackViewModel? _selectedRack;
    [ObservableProperty] private CellViewModel? _selectedCell;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _onlyLowStock;
    [ObservableProperty] private bool _onlyOutOfStock;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRackFilterOn), nameof(IsCellFilterOn))]
    private SearchScope _searchScope;

    public bool IsRackFilterOn
    {
        get => SearchScope != SearchScope.Warehouse;
        set => SearchScope = value
            ? (IsCellFilterOn ? SearchScope.Cell : SearchScope.Rack)
            : SearchScope.Warehouse;
    }

    public bool IsCellFilterOn
    {
        get => SearchScope == SearchScope.Cell;
        set => SearchScope = value ? SearchScope.Cell : SearchScope.Rack;
    }

    public async Task LoadAsync()
    {
        try
        {
            var racks = await visualizationService.GetWarehouseAsync(await warehouseService.GetRacksAsync());
            var rackId = SelectedRack?.Id;
            var cellId = SelectedCell?.Id;
            RacksGrid = new(racks);
            SelectedRack = RacksGrid.FirstOrDefault(rack => rack.Id == rackId);
            SelectedCell = CellsGrid.FirstOrDefault(cell => cell.Id == cellId);
        }
        catch (Exception ex)
        {
            dialogs.ShowError($"Не удалось загрузить схему склада: {ex.Message}");
        }

        try
        {
            _availability = await stockService.GetAvailabilityAsync(CancellationToken.None);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            dialogs.ShowError($"Не удалось загрузить остатки: {ex.Message}");
        }
    }

    [RelayCommand]
    private void SelectRack(RackViewModel rack) => SelectedRack = rack;

    [RelayCommand]
    private void SelectCell(CellViewModel cell) => SelectedCell = cell;

    partial void OnSelectedRackChanged(RackViewModel? oldValue, RackViewModel? newValue)
    {
        if (oldValue is not null)
            oldValue.IsSelected = false;
        if (newValue is not null)
            newValue.IsSelected = true;

        SelectedCell = null;
        CellsGrid = newValue?.Cells ?? [];
        if (IsRackFilterOn)
            ApplyFilters();
    }

    partial void OnSelectedCellChanged(CellViewModel? oldValue, CellViewModel? newValue)
    {
        if (oldValue is not null)
            oldValue.IsSelected = false;
        if (newValue is not null)
            newValue.IsSelected = true;
        if (IsCellFilterOn)
            ApplyFilters();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();

    partial void OnSearchScopeChanged(SearchScope value) => ApplyFilters();

    partial void OnOnlyLowStockChanged(bool value) => ApplyFilters();

    partial void OnOnlyOutOfStockChanged(bool value) => ApplyFilters();

    [RelayCommand]
    private void ShowStockDetails(StockAvailabilityDto availability)
    {
        var component = availability.Component;
        var locations = availability.Locations.Count == 0
            ? "Мест с остатком нет."
            : string.Join("\n", availability.Locations.Select(location =>
                $"Стеллаж {location.RackCodeDisplay}, ячейка {location.CellCodeDisplay} — {location.Quantity:0.###}"));

        dialogs.ShowInfo(
            $"{component.Name}\nАртикул: {component.Article}\nПроизводитель: {component.Manufacturer}\n\n" +
            $"На складе: {availability.Total:0.###}\nМинимум: {component.MinQuantity:0.###}\n\n{locations}",
            availability.Total == 0 ? "Нет на складе" : "Критический остаток");
    }

    private void ApplyFilters()
    {
        var text = SearchText.Trim();
        var rackId = IsRackFilterOn ? SelectedRack?.Id : null;
        var cellId = IsCellFilterOn && rackId.HasValue ? SelectedCell?.Id : null;
        var rows = new List<StockOverviewRow>();

        foreach (var item in _availability)
        {
            var component = item.Component;
            if (!component.Article.Contains(text, StringComparison.OrdinalIgnoreCase) &&
                !component.Name.Contains(text, StringComparison.OrdinalIgnoreCase) &&
                !component.Manufacturer.Contains(text, StringComparison.OrdinalIgnoreCase))
                continue;

            if (item.Locations.Count == 0)
            {
                if (OnlyOutOfStock && rackId is null)
                    rows.Add(new StockOverviewRow(item, null));
                continue;
            }

            if (OnlyLowStock ? !item.IsLow : OnlyOutOfStock)
                continue;

            foreach (var location in item.Locations)
            {
                if ((rackId is null || location.RackId == rackId) &&
                    (cellId is null || location.CellId == cellId))
                    rows.Add(new StockOverviewRow(item, location));
            }
        }

        Stocks = rows;
    }
}

public sealed record StockOverviewRow(StockAvailabilityDto Availability, StockViewDTO? Location)
{
    public decimal Quantity => Location?.Quantity ?? 0;
    public string Status => Location is null
        ? "Нет на складе"
        : $"⚠ Мало · {Availability.Total:0.###} из {Availability.Component.MinQuantity:0.###}";
}
