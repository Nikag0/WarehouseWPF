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
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using Xceed.Wpf.AvalonDock.Layout;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels
{
    public partial class ReceiptViewModel : INotifyPropertyChanged
    {
        // Поиск по компонентам. Верхняя левая часть экрана ReceiptView.
        private readonly List<ViewItemDTO> _allComponents = new();
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
        public bool IsReceiptPopupOpen
        {
            get => _isReceiptPopupOpen;
            set
            {
                _isReceiptPopupOpen = value;
                OnPropertyChanged();
            }
        }
        private bool _isReceiptPopupOpen;

        // Отображение остатков. Средняя левая часть экрана ReceiptView.
        private readonly List<ViewItemDTO> _allStocks = new();
        public ObservableCollection<ViewItemDTO> Stocks { get; } = new();

        // Объект приёмки.
        public StockViewModel ReceiptItem
        {
            get => _receiptItem;
            set
            {

                _receiptItem = value;
               
            }
        }
        private StockViewModel _receiptItem = new();

        // Объект выбранного стеллажа.
        public RackViewModel SelectedRack
        {
            get => _selectedRack;
            set
            {
                _selectedRack = value;
                OnPropertyChanged();

                // Подсветка
                foreach (var r in Racks)
                    r.IsSelected = r == value;

                _ = LoadCellsForSelectedRack();
            }
        }
        private RackViewModel _selectedRack;
        public string? SearchRacks
        {
            get => _searchRacks;
            set
            {
                _searchRacks = value;
                OnPropertyChanged();
                FilterRacks();
                AutomaticRackSelection();
            }
        }
        private string _searchRacks;
        public ObservableCollection<RackViewModel> Racks { get; } = new();
        public ObservableCollection<RackViewModel> FilteredRacks { get; } = new();
        public bool IsRackPopupOpen
        {
            get => _isRackPopupOpen;
            set
            {
                _isRackPopupOpen = value;
                OnPropertyChanged();
            }
        }
        private bool _isRackPopupOpen;


        // Объект ячеек выбранного стеллажа.
        private Dictionary<RackType, List<CellLayout>> _cellLayouts;

        public CellViewModel SelectedCell
        {
            get => _selectedCell;
            set
            {
                _selectedCell = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Cells));

                foreach (var cell in Cells)
                {
                    cell.IsSelected = _selectedCell != null && cell.Id == _selectedCell.Id;

                }
            }
        }
        private CellViewModel _selectedCell;
        public string SearchCell
        {
            get => _searchCell;
            set
            {
                _searchCell = value;
                OnPropertyChanged();
                FilterCells();
                AutomaticCellSelection();
            }
        }
        private string _searchCell;

        private List<Cell> _cells = new();
        public ObservableCollection<CellViewModel> Cells { get; } = new();
        public ObservableCollection<CellViewModel> FilteredCells { get; } = new();
        public bool IsCellPopupOpen
        {
            get => _isCellPopupOpen;
            set
            {
                _isCellPopupOpen = value;
                OnPropertyChanged();
            }
        }
        private bool _isCellPopupOpen;

        public ObservableCollection<Operator> Operators { get; } = new();
        public Operator OperatorName
        {
            get => _operatorName;
            set
            {
                _operatorName = value;
                OnPropertyChanged();
            }
        }
        private Operator _operatorName;
        public string CommentText
        {
            get => _commentText;
            set
            {
                _commentText = value;
                OnPropertyChanged();
            }
        }
        private string _commentText;

        // Визуализация
        private readonly SemaphoreSlim _lock = new(1, 1);

        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private readonly CellService _cellService;
        private readonly RackService _rackService;
        private readonly DialogService _dialogService;
        private readonly OperatorService _operatorrService;

        public ICommand SetCellFromListCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ReceiveCommand { get; }

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

            RefreshCommand = new RelayCommand(LoadWindow);
            ReceiveCommand = new RelayCommand(ReceiveAsync);
            SetCellFromListCommand = new RelayCommand(SetCellFromList);
        }

        public async Task LoadWindow()
        {
            try
            {
                _allComponents.Clear();
                _allComponents.AddRange(await _receiptService.GetAllComponentsAsync());

                _allStocks.Clear();
                _allStocks.AddRange(await _stockService.GetAllAsync());
                ReplaceCollection(Stocks, _allStocks);

                await LoadRacks();
                ReplaceCollection(FilteredRacks, Racks);

                _cells.Clear();
                _cells.AddRange(await _cellService.GetAllAsync());

                LoadCellLayouts();

                await LoadOperators();

                FilterItemsToReceipt();
                FilterCells();
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

        public async Task LoadOperators()
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

        private async Task LoadRacks()
        {
            Racks.Clear();

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RackDescription.json");

            var layouts = JsonSerializer.Deserialize<List<RackLayout>>(
                File.ReadAllText(path));

            var layoutDict = layouts.ToDictionary(l => l.Code);

            var racksDb = await _rackService.GetAllAsync();

            foreach (var rack in racksDb)
            {
                if (!layoutDict.TryGetValue(rack.RackCode, out var layout))
                    continue;

                Racks.Add(new RackViewModel(rack, layout));
            }
        }

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

                SelectedRack = null;
                SearchRacks = string.Empty;
                SelectedCell = null;
                SearchCell = string.Empty;
                ReceiptItem.OperationQuantity = 0;
                CommentText = string.Empty;

                await _receiptService.ReceiveAsync(receiptDto, OperatorName.FullName, CommentText);
                await LoadWindow();
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

        private void FilterItemsToReceipt()
        {
            if (string.IsNullOrWhiteSpace(SearchtemsToReceipt))
            {
                ReplaceCollection(FilteredItemsToReceipt,
                    _allStocks.OrderByDescending(x => x.ComponentName));
                return;
            }

            var text = SearchtemsToReceipt;

            var filteredStocks = _allStocks.Where(FilterPredicate);
            var filteredComponents = _allComponents.Where(FilterPredicate);

            var stockIds = _allStocks
                .Select(x => x.ComponentId)
                .ToHashSet();

            var result = filteredStocks
                        .Concat(filteredComponents.Where(c =>
                        !filteredStocks.Any(s => s.ComponentId == c.ComponentId)));

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

        [RelayCommand]
        private async void SelectRack(RackViewModel rack)
        {
            if (ReceiptItem.Article == null) return;

            SelectedRack = rack;
            SearchRacks = rack.CodeDisplay;
        }

        private void FilterRacks()
        {
            var query = Racks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchRacks))
            {
                var text = SearchRacks.ToLower();

                query = query.Where(c =>
                        c.CodeDisplay != null
                        && c.CodeDisplay.ToLower().Contains(text));
            }

            var sortedQuery = query
                             .OrderByDescending(r => r.Column)
                             .ThenByDescending(r => r.Row);

            ReplaceCollection(FilteredRacks, sortedQuery);
        }

        private void AutomaticRackSelection()
        {
            var rack = Racks.FirstOrDefault(x => x.CodeDisplay == SearchRacks);

            SelectedRack = rack;
        }

        [RelayCommand]
        private void SetRackFromList(object obj)
        {
            if (obj is not RackViewModel rack)
                return;

            SelectedRack = rack;
            SearchRacks = rack.Code;
            IsRackPopupOpen = false;
        }


        private void LoadCellLayouts()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CellDescription.json");

            var raw = JsonSerializer.Deserialize<List<CellLayoutRoot>>(File.ReadAllText(path));

            _cellLayouts = raw.ToDictionary(
                x => Enum.Parse<RackType>(x.Type),
                x => x.Cells);
        }

        private async Task LoadCellsForSelectedRack()
        {
            if (SelectedRack == null)
                return;

            SelectedCell = null;
            SearchCell = "";
            Cells.Clear();

            var rackType = SelectedRack.Type;

            if (!_cellLayouts.TryGetValue(rackType, out var layout))
                return;

            var rackCells = _cells.Where(c => c.RackId == SelectedRack.Id);

            foreach (var cell in rackCells)
            {
                var cellLayout = layout.FirstOrDefault(l => l.Code == cell.CellCode);
                if (cellLayout == null)
                    continue;

                Cells.Add(new CellViewModel(cell, cellLayout));
            }
        }

        [RelayCommand]
        private void SelectCell(CellViewModel cell)
        {
            SelectedCell = cell;

            if (cell != null)
            {
                SearchCell = cell.CodeDisplay;
            }
        }
        private void SetCellFromList(object obj)
        {
            if (obj is not CellViewModel cell)
                return;

            SelectedCell = cell;
            SearchCell = cell.CodeDisplay;
            IsCellPopupOpen = false;
        }

        private void FilterCells()
        {
            if (SelectedRack == null) return;

            var query = Cells.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchCell))
            {
                var text = SearchCell.ToLower();

                query = query.Where(c =>
                        c.CodeDisplay != null
                        && c.CodeDisplay.ToLower().Contains(text));
            }

            var sortedQuery = query
                             .OrderByDescending(r => r.Column)
                             .ThenByDescending(r => r.Row);

            ReplaceCollection(FilteredCells, sortedQuery);
        }

        private void AutomaticCellSelection()
        {
            var cell = Cells.FirstOrDefault(x => x.CodeDisplay == SearchCell);

            SelectedCell = cell;
        }

        [RelayCommand]
        private async Task AddItemToReceipt(object obj)
        {
            try
            {
                if (obj is ViewItemDTO stock)
                {
                    ReceiptItem.ComponentId = stock.ComponentId;
                    ReceiptItem.Article = stock.Article;
                    ReceiptItem.ComponentName = stock.ComponentName;
                    ReceiptItem.Manufacturer = stock.Manufacturer;
                    if (stock.RackId != Guid.Empty)
                    {
                        SelectedRack = Racks.FirstOrDefault(r => r.Id == stock.RackId);
                        SearchRacks = SelectedRack.CodeDisplay;
                    }
                    if (stock.CellId != Guid.Empty)
                    {
                        SelectedCell = Cells.FirstOrDefault(c => c.Id == stock.CellId);
                        SearchCell = SelectedCell.CodeDisplay;
                    }
                }
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


        private void ReplaceCollection<T>(
            ObservableCollection<T> target,
            IEnumerable<T> source)
        {
            target.Clear();
            foreach (var item in source)
                target.Add(item);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
