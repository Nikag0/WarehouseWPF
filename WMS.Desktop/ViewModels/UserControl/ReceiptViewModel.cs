using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WMS.Application.WarehouseVisualization;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using Xceed.Wpf.AvalonDock.Layout;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WMS.Desktop.ViewModels
{
    public partial class ReceiptViewModel : ObservableObject
    {
        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private readonly CellService _cellService;
        private readonly RackService _rackService;
        private readonly DialogService _dialogService;
        private readonly OperatorService _operatorrService;

        [ObservableProperty] private StockViewModel _receiptItem = new();
        [ObservableProperty] private bool _isReceiptPopupOpen;
        [ObservableProperty] private bool _isRackPopupOpen;
        [ObservableProperty] private bool _isCellPopupOpen;
        [ObservableProperty] private Operator _operatorName;
        [ObservableProperty] private string _commentText;

        public ObservableCollection<Operator> Operators { get; } = new();
        public ObservableCollection<RackViewModel> RacksGrid { get; } = new(); // отвечает за ui отображение сетки стеллажей
        public ObservableCollection<CellViewModel> CellsGrid { get; } = new(); // отвечает за ui отображение сетки ячеек

        private Dictionary<Guid, List<CellViewModel>> _groupedCellsCache = new(); // словарь всех ячеек, привязанных к RackId
        public ObservableCollection<RackViewModel> FilteredRacks { get; } = new(); // выпадающий список при вводе стеллажа
        public ObservableCollection<CellViewModel> FilteredCells { get; } = new(); // выпадающий список при вводе ячейки

        // отвечает за ручной поиск стеллажа
        public string? SearchRacks
        {
            get => _searchRacks;
            set
            {
                if (SetProperty(ref _searchRacks, value))
                {
                    FilterRacks(value);

                    var autoSelected = RacksGrid.FirstOrDefault(x => x.CodeDisplay == value);
                    if (autoSelected != null)
                    {
                        SelectedRack = autoSelected;
                    }
                    else
                    {
                        SelectedRack = null;
                    }
                }
            }
        }
        private string _searchRacks;

        public RackViewModel SelectedRack
        {
            get => _selectedRack;
            set
            {
                var previousRack = _selectedRack;

                if (SetProperty(ref _selectedRack, value))
                {
                    // Подсветка самого стеллажа
                    if (previousRack != null)
                        previousRack.IsHighlighted = false;

                    if (value != null)
                        value.IsHighlighted = true;

                    // Синхронизируем текст
                    if (value != null || string.IsNullOrEmpty(_searchRacks))
                    {
                        _searchRacks = value?.CodeDisplay ?? string.Empty;
                        OnPropertyChanged(nameof(SearchRacks));
                        FilterRacks(_searchRacks);
                    }

                    SelectCellsForRack();   // Загрузили ячейки в CellsGrid
                    FilterItemInRacks();    // Отфильтровали остатки товаров в FilteredItemsInRacks
                    UpdateCellState();      // Подсветили ячейки с товаром
                }
            }
        }
        private RackViewModel _selectedRack;

        // отвечает за ручной поиск ячейки
        public string SearchCell
        {
            get => _searchCell;
            set
            {
                if (SetProperty(ref _searchCell, value))
                {
                    FilterCells(value);

                    var autoSelected = CellsGrid.FirstOrDefault(x => x.CodeDisplay == value);
                    if (autoSelected != null)
                    {
                        SelectedCell = autoSelected;
                    }
                    else
                    {
                        SelectedCell = null;
                    }
                }
            }
        }
        private string _searchCell;

        public CellViewModel SelectedCell
        {
            get => _selectedCell;
            set
            {
                var previousCell = _selectedCell;

                if (SetProperty(ref _selectedCell, value))
                {
                    if (previousCell != null)
                        previousCell.IsHighlighted = false;

                    if (value != null)
                        value.IsHighlighted = true;

                    if (value != null || string.IsNullOrEmpty(_searchCell))
                    {
                        _searchCell = value?.CodeDisplay ?? string.Empty;
                        OnPropertyChanged(nameof(SearchCell));
                        FilterCells(_searchCell);
                    }
                }
            }
        }
        private CellViewModel _selectedCell;

        // Ниже не переработанные свойства
        private readonly List<ViewItemDTO> _components = new(); 
        private readonly List<ViewItemDTO> _stocks = new();

        [ObservableProperty] private ObservableCollection<ViewItemDTO> _filteredStocks = new();
        public ObservableCollection<ViewItemDTO> FilteredItemsToReceipt { get; } = new();
        public string SearchtemsToReceipt
        {
            get => _searchtemsToIssue;
            set
            {
                _searchtemsToIssue = value;
                OnPropertyChanged();
                FilterItemsToReceipt();
            }
        }
        private string _searchtemsToIssue;

        public ObservableCollection<ViewItemDTO> Stocks { get; } = new();

        public ObservableCollection<ViewItemDTO> FilteredItemsInRacks { get; } = new();

        private readonly SemaphoreSlim _lock = new(1, 1);

        public ReceiptViewModel(
            StockService stockService,
            ReceiptService receiptService,
            CellService cellService,
            DialogService dialogService,
            OperatorService operatorrService,
            RackService rackService)
        {
            _stockService = stockService;
            _receiptService = receiptService;
            _cellService = cellService;
            _rackService = rackService;
            _dialogService = dialogService;
            _operatorrService = operatorrService;
        }

        [RelayCommand]
        public async Task ReceiveAsync()
        {

            if (ReceiptItem == null)
            {
                _dialogService.ShowWarning("Компонент не выбран.");
                return;
            }

            if (SelectedRack == null || SelectedCell == null)
            {
                _dialogService.ShowWarning("Стеллаж или ячейка не выбраны.");
                return;
            }

            if (ReceiptItem.OperationQuantity <= 0)
            {
                _dialogService.ShowWarning("Количество товаров для приёмки должно быть больше 0.");
                return;
            }

            if (OperatorName == null)
            {
                _dialogService.ShowWarning("Оператор не указан.");
                return;
            }

            if (!_dialogService.ShowConfirmation("Вы уверены, что хотите выполнить приёмку товара? \n" +
                $"• {ReceiptItem.Article} | {ReceiptItem.ComponentName} \n" +
                $"Производитель: {ReceiptItem.Manufacturer}\n" +
                $"Количество: {ReceiptItem.OperationQuantity}\n" +
                $"Cтеллаж: {SearchRacks} Ячейка: {SearchCell}"))
                return;

            await _lock.WaitAsync();

            try
            {
                var receiptDto = new ServiceItemDTO(
                    ReceiptItem.ComponentId,
                    SelectedRack.Id,
                    SelectedCell.Id,
                    ReceiptItem.OperationQuantity);


                await _receiptService.ReceiveAsync(receiptDto, OperatorName.FullName, CommentText);
                await LoadDataAsync();

                SelectedRack.IsHighlighted = false;
                SelectedRack = null;
                SearchRacks = string.Empty;
                SelectedCell.IsHighlighted = false;
                SelectedCell = null;
                SearchCell = string.Empty;
                ReceiptItem.OperationQuantity = 0;
                CommentText = string.Empty;
            }
            catch (WrongValueExeption ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (OverallDomainException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task LoadDataAsync()
        {
            try
            {
                _components.Clear();
                _components.AddRange(await _receiptService.GetAllComponentsAsync());

                await LoadStocksAsync();
                ReplaceCollection(Stocks, _stocks);

                await CreateRacksGridAsync();
                ReplaceCollection(FilteredRacks, RacksGrid);

                await LoadCellsLookupAsync();

                await LoadOperatorsAsync();

                FilterItemsToReceipt();
                UpdateHighlights();
            }
            catch (OverallDomainException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
        }

        private async Task LoadStocksAsync()
        {
            _stocks.Clear();
            _stocks.AddRange(await _stockService.GetAllAsync());
        }

        public async Task LoadOperatorsAsync()
        {
            try
            {
                Operators.Clear();
                var items = await _operatorrService.GetAllAsync();
                foreach (var item in items)
                    Operators.Add(item);
            }
            catch (OverallDomainException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
        }

        private async Task CreateRacksGridAsync()
        {
            try
            {
                var racksData = await _rackService.GetRacksWithLayoutsAsync();

                RacksGrid.Clear();

                foreach (var item in racksData)
                {
                    RacksGrid.Add(new RackViewModel(item.Rack, item.Layout));
                }
            }
            catch (FileNotFoundException ex)
            {
                _dialogService.ShowError($"{ex.Message}");
            }
        }

        private async Task LoadCellsLookupAsync()
        {
            try
            {
                var allCellsWithLayouts = await _cellService.GetCellsWithLayoutsAsync();

                _groupedCellsCache = allCellsWithLayouts.GroupBy(pair => pair.cell.RackId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(pair => new CellViewModel(pair.cell, pair.layout)).ToList()
                    );
            }
            catch (FileNotFoundException ex)
            {
                _dialogService.ShowError($"{ex.Message}");
            }
        }

        private void SelectCellsForRack()
        {
            if (SelectedRack == null)
                return;

            CellsGrid.Clear();

            if (_groupedCellsCache.TryGetValue(SelectedRack.Id, out var cachedCells))
            {
                foreach (var cellVm in cachedCells)
                {
                    CellsGrid.Add(cellVm);
                }
            }
        }

        private void FilterRacks(string? searchText)
        {
            var query = RacksGrid.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.ToLower();
                query = query.Where(c => c.CodeDisplay != null && c.CodeDisplay.ToLower().Contains(text));
            }

            var sortedQuery = query
                             .OrderByDescending(r => r.Column)
                             .ThenByDescending(r => r.Row);

            ReplaceCollection(FilteredRacks, sortedQuery);
        }

        [RelayCommand]
        private void SelectRack(RackViewModel? rack)
        {
            SelectedRack = rack;
            IsRackPopupOpen = false;
        }

        private void FilterItemInRacks()
        {
            var query = _stocks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchRacks))
            {
                var text = SearchRacks.ToLower();

                query = query.Where(c =>
                        c.RackCodeDisplay != null
                        && c.RackCodeDisplay.ToLower().Contains(text));

                var sortedQuery = query
                                 .OrderByDescending(r => r.ComponentName);

                ReplaceCollection(FilteredItemsInRacks, sortedQuery);
            }
            else
                FilteredItemsInRacks.Clear();
        }

        private void UpdateCellState()
        {
            var occupiedCellIds = FilteredItemsInRacks
                .Select(x => x.CellId)
                .Distinct()
                .ToHashSet();

            foreach (var cell in CellsGrid)
            {
                cell.HasItemsInCell = occupiedCellIds.Contains(cell.Id);
            }
        }

        private void FilterCells(string? searchText)
        {
            if (SelectedRack == null)
            {
                FilteredCells.Clear();
                return;
            }

            var query = CellsGrid.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.ToLower();
                query = query.Where(c => c.CodeDisplay != null && c.CodeDisplay.ToLower().Contains(text));
            }

            var sortedQuery = query
                             .OrderByDescending(r => r.Column)
                             .ThenByDescending(r => r.Row);

            ReplaceCollection(FilteredCells, sortedQuery);
        }

        [RelayCommand]
        private void SelectCell(CellViewModel cell)
        {
            SelectedCell = cell;
            IsCellPopupOpen = false;
        }

        [RelayCommand] // можно подумать над оптимизацией
        private void AddItemToReceipt(object obj)
        {
            try
            {
                if (obj is not ViewItemDTO stock)
                    return;

                ReceiptItem.ComponentId = stock.ComponentId;
                ReceiptItem.Article = stock.Article;
                ReceiptItem.ComponentName = stock.ComponentName;
                ReceiptItem.Manufacturer = stock.Manufacturer;

                if (stock.RackId != Guid.Empty)
                {
                    SelectedRack = RacksGrid.FirstOrDefault(r => r.Id == stock.RackId);
                }
                else
                {
                    SelectedRack = null;
                    SearchRacks = string.Empty;
                    SelectedCell = null;
                    SearchCell = string.Empty;
                    return;
                }

                SelectCellsForRack();

                if (stock.CellId != Guid.Empty)
                {
                    SelectedCell = CellsGrid.FirstOrDefault(c => c.Id == stock.CellId);
                    SearchCell = SelectedCell?.CodeDisplay ?? string.Empty;
                }

            }
            catch (OverallDomainException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при выборе элемента к выдаче: {ex.Message}");
            }
        }

        // ниже не переработанные методы
        private void FilterItemsToReceipt()
        {
            if (string.IsNullOrWhiteSpace(SearchtemsToReceipt))
            {
                ReplaceCollection(FilteredItemsToReceipt,
                    _stocks.OrderByDescending(x => x.ComponentName));
                return;
            }

            var filteredStocks = _stocks.Where(FilterPredicate);
            var filteredStockIds = filteredStocks.Select(x => x.ComponentId).ToHashSet();
            var filteredComponents = _components.Where(FilterPredicate);

            var stockIds = _stocks
                .Select(x => x.ComponentId)
                .ToHashSet();

            var result = filteredStocks
                .Concat(filteredComponents.Where(c => !filteredStockIds.Contains(c.ComponentId)));

            ReplaceCollection(FilteredItemsToReceipt,
                result.OrderByDescending(x => x.ComponentName));
        }

        private bool FilterPredicate(ViewItemDTO c)
        {
            return
                (c.Article?.Contains(SearchtemsToReceipt, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.ComponentName?.Contains(SearchtemsToReceipt, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Manufacturer?.Contains(SearchtemsToReceipt, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private void UpdateHighlights()
        {
            foreach (var rack in RacksGrid)
                rack.IsHighlighted = rack.Id == ReceiptItem.RackId;

            foreach (var cell in CellsGrid)
                cell.IsHighlighted = cell.Id == ReceiptItem.CellId;
        }

        private void ReplaceCollection<T>(
            ObservableCollection<T> target,
            IEnumerable<T> source)
        {
            target.Clear();
            foreach (var item in source)
                target.Add(item);
        }

    }
}
