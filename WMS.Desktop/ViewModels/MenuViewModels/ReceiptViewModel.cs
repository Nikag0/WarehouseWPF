using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WMS.Application.DTO;
using WMS.Application.Services;
using WMS.Application.WarehouseVisualization;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using Xceed.Wpf.AvalonDock.Layout;

namespace WMS.Desktop.ViewModels.MenuViewModels
{
    public partial class ReceiptViewModel : ObservableObject
    {
        // Список при поиске отсатков или компонентов.
        public ObservableCollection<ViewItemDTO> FilteredStocksOrComponents { get; } = new();
        // Список остатков в выбранном стеллаже
        public ObservableCollection<ViewItemDTO> FilteredItemsInRacks { get; } = new();
        // Выпадающий список при вводе стеллажа.
        public ObservableCollection<RackViewModel> FilteredRacks { get; } = new();
        // Выпадающий список при вводе ячейки.
        public ObservableCollection<CellViewModel> FilteredCells { get; } = new(); 
        public ObservableCollection<Operator> Operators { get; } = new();
        // Отвечает за ui отображение сетки стеллажей.
        public ObservableCollection<RackViewModel> RacksGrid { get; } = new();
        // Отвечает за ui отображение сетки ячеек.
        public ObservableCollection<CellViewModel> CellsGrid { get; } = new();
        // Отвечает за ручной поиск стеллажа.
        public string? SearchRacks
        {
            get => _searchRacks;
            set
            {
                if (SetProperty(ref _searchRacks, value))
                {
                    FilterRacks(value);

                    var autoSelected = RacksGrid.FirstOrDefault(x => x.Code == value);
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
                        previousRack.IsSelected = false;

                    if (value != null)
                        value.IsSelected = true;

                    // Синхронизируем текст
                    if (value != null || string.IsNullOrEmpty(_searchRacks))
                    {
                        _searchRacks = value?.Code ?? string.Empty;
                        OnPropertyChanged(nameof(SearchRacks));
                        FilterRacks(_searchRacks);
                    }
                    // Загрузка ячеек в CellsGrid.
                    LoadCells();
                    // Обновляем фильтр.
                    FilterCells(_searchCell);
                    // Загружаем остатки и обновляем подсветку
                    _ = RefreshStockInRackAsync();
                }
            }
        }
        // Отвечает за ручной поиск ячейки.
        public string SearchCell
        {
            get => _searchCell;
            set
            {
                if (SetProperty(ref _searchCell, value))
                {
                    FilterCells(value);

                    var autoSelected = CellsGrid.FirstOrDefault(x => x.Code == value);
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
        public CellViewModel SelectedCell
        {
            get => _selectedCell;
            set
            {
                var previousCell = _selectedCell;

                if (SetProperty(ref _selectedCell, value))
                {
                    if (previousCell != null)
                        previousCell.IsSelected = false;

                    if (value != null)
                        value.IsSelected = true;

                    if (value != null || string.IsNullOrEmpty(_searchCell))
                    {
                        _searchCell = value?.Code ?? string.Empty;
                        OnPropertyChanged(nameof(SearchCell));
                        FilterCells(_searchCell);
                    }
                }
            }
        }

        [ObservableProperty] private StockViewModel _receiptItem = new();
        [ObservableProperty] private bool _isReceiptPopupOpen;
        [ObservableProperty] private bool _isRackPopupOpen;
        [ObservableProperty] private bool _isCellPopupOpen;
        [ObservableProperty] private Operator _operatorName;
        [ObservableProperty] private string _commentText;
        [ObservableProperty] private string _searchStockOrComponent;

        private string _searchRacks;
        private string _searchCell;
        private RackViewModel _selectedRack;
        private CellViewModel _selectedCell;
        private CancellationTokenSource? _cts;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly ReceiptService _receiptService;
        private readonly DialogService _dialogService;
        private readonly OperatorService _operatorrService;
        private readonly WarehouseService _warehouseService;


        public ReceiptViewModel(
            ReceiptService receiptService,
            DialogService dialogService,
            OperatorService operatorrService,
            WarehouseService warehouseService)
        {
            _receiptService = receiptService;
            _dialogService = dialogService;
            _operatorrService = operatorrService;
            _warehouseService = warehouseService;
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
                $"Cтеллаж: {SelectedRack.Code} Ячейка: {SelectedCell.Code}"))
                return;

            await _lock.WaitAsync();

            try
            {
                var receiptDto = new ReceiptItemDto(
                    ReceiptItem.ComponentId,
                    SelectedRack.Id,
                    SelectedCell.Id,
                    ReceiptItem.OperationQuantity);

                await _receiptService.ReceiveAsync(receiptDto, OperatorName.FullName, CommentText);

                await LoadWarehouseAsync();

                SelectedRack.IsSelected = false;
                SelectedRack = null;
                SearchRacks = string.Empty;

                SelectedCell.IsSelected = false;
                SelectedCell = null;
                SearchCell = string.Empty;

                ReceiptItem.Article = string.Empty;
                ReceiptItem.ComponentName = string.Empty;
                ReceiptItem.Manufacturer = string.Empty;
                ReceiptItem.OperationQuantity = 0;
                CommentText = string.Empty;

                OnSearchStockOrComponentChanged(SearchStockOrComponent);
                ReplaceCollection(FilteredRacks, RacksGrid);

                _dialogService.ShowInfo("Приём товаров успешно выполнен.");
            }
            catch (BusinessException ex)
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
                await LoadWarehouseAsync();
                await LoadOperatorsAsync();
                ReplaceCollection(FilteredRacks, RacksGrid);
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
        }

        [RelayCommand]
        public void SelectRack(RackViewModel rack)
        {
            SelectedRack = rack;
            IsRackPopupOpen = false;
        }

        [RelayCommand]
        public void SelectCell(CellViewModel cell)
        {
            SelectedCell = cell;
            IsCellPopupOpen = false;
        }

        [RelayCommand] // можно подумать над оптимизацией
        public void AddItemToReceipt(object obj)
        {
            try
            {
                if (obj is not ViewItemDTO stock)
                    return;


                ReceiptItem.Id = stock.StockId;
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

                LoadCells();

                if (stock.CellId != Guid.Empty)
                {
                    SelectedCell = CellsGrid.FirstOrDefault(c => c.Id == stock.CellId);
                    SearchCell = SelectedCell?.Code ?? string.Empty;
                }

            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при выборе элемента к выдаче: {ex.Message}");
            }
        }

        private async Task LoadWarehouseAsync()
        {
            var racks = await _warehouseService.GetWarehouseAsync();

            RacksGrid.Clear();

            foreach (var rackDTO in racks)
            {
                RacksGrid.Add(new RackViewModel(rackDTO));
            }
        }

        private void LoadCells()
        {
            CellsGrid.Clear();

            if (SelectedRack is null)
                return;

            foreach (var cell in SelectedRack.Cells)
            {
                CellsGrid.Add(cell);
            }
        }

        private async Task LoadOperatorsAsync()
        {
            try
            {
                Operators.Clear();
                var items = await _operatorrService.GetAllAsync();
                foreach (var item in items)
                    Operators.Add(item);
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
        }

        private async Task RefreshStockInRackAsync()
        {
            this._cts?.Cancel();

            if (SelectedRack == null)
            {
                FilteredItemsInRacks.Clear();
                return;
            }

            this._cts = new CancellationTokenSource();
            var token = this._cts.Token;

            try
            {
                var stocks = await _receiptService.GetStocksInRackAsync(SelectedRack.Id);

                token.ThrowIfCancellationRequested();

                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    FilteredItemsInRacks.Clear();

                    foreach (var stock in stocks)
                    {
                        FilteredItemsInRacks.Add(stock);
                    }

                    UpdateCellHighlights(stocks);
                });
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отменённый запрос
            }
        }

        private void FilterRacks(string? searchText)
        {
            var query = RacksGrid.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.ToLower();
                query = query.Where(c => c.Code != null && c.Code.ToLower().Contains(text));
            }

            var sortedQuery = query
                             .OrderByDescending(r => r.Code);

            ReplaceCollection(FilteredRacks, sortedQuery);
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
                query = query.Where(c => c.Code != null && c.Code.ToLower().Contains(text));
            }

            var sortedQuery = query.OrderByDescending(r => r.Code);

            ReplaceCollection(FilteredCells, sortedQuery);
        }

        private void UpdateCellHighlights(IEnumerable<ViewItemDTO> stocks)
        {
            var occupiedCellIds = stocks
                .Select(x => x.CellId)
                .ToHashSet();

            foreach (var cell in CellsGrid)
            {
                var shouldHighlight =
                    occupiedCellIds.Contains(cell.Id);

                if (cell.ItemInCell != shouldHighlight)
                {
                    cell.ItemInCell = shouldHighlight;
                }
            }
        }

        partial void OnSearchStockOrComponentChanged(string value)
        {
            _cts?.Cancel();

            if (string.IsNullOrWhiteSpace(value))
            {
                FilteredStocksOrComponents.Clear();
                return;
            }

            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500, _cts.Token);

                    var suggestions = await _receiptService.GetFilteredStockOrComponentsAsync(value, maxCount: 15, _cts.Token);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (!_cts.Token.IsCancellationRequested)
                        {
                            FilteredStocksOrComponents.Clear();
                            foreach (var item in suggestions)
                            {
                                FilteredStocksOrComponents.Add(item);
                            }
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    // Задача отменена новым вводом текста — ничего не делаем
                }
            });
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
