using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels
{
    public class ReceiptViewModel : INotifyPropertyChanged
    {
        // Поиск по компонентам. Верхняя левая часть экрана ReceiptView.
        private readonly List<ViewItemDTO> _allComponents = new();
        public ObservableCollection<ViewItemDTO> FilteredItemsToIssue { get; } = new();
        public string SearchtemsToIssue
        {
            get => _searchtemsToIssue;
            set
            {
                _searchtemsToIssue = value;
                OnPropertyChanged();
                FilterItemsToIssue();
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

        // Приёмка. Нижняя левая часть экрана ReceiptView.
        public StockViewModel ReceiptItem
        {
            get => _receiptItem;
            set
            {

                _receiptItem = value;
               
            }
        }
        private StockViewModel _receiptItem = new();

        private readonly List<Rack> _racks = new();
        public ObservableCollection<Rack> FilteredRacks { get; } = new();
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

        private readonly List<Cell> _cells = new();
        private readonly List<Cell> _freeCells = new();
        public ObservableCollection<Cell> FilteredFreeCells { get; } = new();
        public string SearchFreeCell
        {
            get => _searchFreeCell;
            set
            {
                _searchFreeCell = value;
                OnPropertyChanged();
                FilterFreeCells();
                AutomaticCellSelection();
            }
        }
        private string _searchFreeCell;
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
        private ObservableCollection<RackViewModel> racksVisualise { get; set; } = new();
        private ObservableCollection<CellViewModel> cellsVisualise { get; } = new();
        public CellsType CurrentCellType
        {
            get => _currentCellType;
            set
            {
                _currentCellType = value;
                OnPropertyChanged();
            }
        }
        private CellsType _currentCellType;
        public RackViewModel SelectedRack
        {
            get => _selectedRack;
            set
            {
                _selectedRack = value;
                OnPropertyChanged(nameof(VisibleCells));
            }
        } // SelectedRack - свойство решает, ячейки какого стеллажа будут визуализироваться.
        private RackViewModel _selectedRack;
        public IEnumerable<RackViewModel> NormalVisibleRacks =>
                racksVisualise.Where(r => r.Row != 4);
        public IEnumerable<RackViewModel> SpecialVisibleRacks =>
                racksVisualise.Where(r => r.Row == 4);
        public IEnumerable<CellViewModel> VisibleCells =>
                cellsVisualise.Where(c => c.RackId == SelectedRack.Id);

        private readonly SemaphoreSlim _lock = new(1, 1);

        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private readonly CellService _cellService;
        private readonly RackService _rackService;
        private readonly DialogService _dialogService;
        private readonly OperatorService _operatorrService;

        public ICommand SetRackFromListCommand { get; }
        public ICommand SetCellFromListCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ReceiveCommand { get; }
        public ICommand AddItemToReceiptCommand { get; }

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
            AddItemToReceiptCommand = new RelayCommand(AddItemToReceipt);
            ReceiveCommand = new RelayCommand(ReceiveAsync);
            SetRackFromListCommand = new RelayCommand(SetRackFromList);
            SetCellFromListCommand = new RelayCommand(SetCellFromList);

            racksVisualise.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(NormalVisibleRacks));
                OnPropertyChanged(nameof(SpecialVisibleRacks));
            };
        }

        public async Task ReceiveAsync()
        {

            if (ReceiptItem == null)
            {
                _dialogService.ShowWarning("Компонент не выбран.");
                return;
            }

            if (ReceiptItem.RackId == Guid.Empty|| ReceiptItem.CellId == Guid.Empty)
            {
                _dialogService.ShowWarning("Стеллаж или ячейка не выбраны.");
                return;
            }

            if (ReceiptItem.RackId == Guid.Empty|| ReceiptItem.OperationQuantity <= 0)
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
                $"Cтеллаж: {SearchRacks} Ячейка: {SearchFreeCell}"))
                return;

            await _lock.WaitAsync();

            try
            {
                var receiptDto = new ServiceItemDTO(
                    ReceiptItem.ComponentId,
                    ReceiptItem.RackId,
                    ReceiptItem.CellId,
                    ReceiptItem.OperationQuantity);

                ReceiptItem.RackId = Guid.Empty;
                SearchRacks = string.Empty;
                ReceiptItem.CellId = Guid.Empty;
                SearchFreeCell = string.Empty;
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

        public async Task LoadWindow()
        {
            try
            {
                _allComponents.Clear();
                _allComponents.AddRange(await _receiptService.GetAllComponentsAsync());

                _allStocks.Clear();
                _allStocks.AddRange(await _stockService.GetAllAsync());

                _racks.Clear();
                _racks.AddRange(await _rackService.GetAllAsync());

                _cells.Clear();
                _cells.AddRange(await _cellService.GetAllAsync());

                ReplaceCollection(Stocks, await _stockService.GetAllAsync());

                FilterItemsToIssue();
                FilterFreeCells();
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

        public async Task LoadWarehouseView()
        {
            try
            {
                racksVisualise.Clear();
                foreach (var rack in _racks)
                    racksVisualise.Add(new RackViewModel(rack, ReceiptItem));

                cellsVisualise.Clear();
                var cells = await _cellService.GetAllAsync();
                foreach (var cell in cells)
                    cellsVisualise.Add(new CellViewModel(cell, ReceiptItem));
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

        public void SelectRack(RackViewModel rackVm)
        {
            SelectedRack = rackVm;

            if (rackVm.Column == 3 && rackVm.Row != 4)
                CurrentCellType = CellsType.Cell1;

            else if ((rackVm.Column == 1 || rackVm.Column == 2 || rackVm.Column == 4) && rackVm.Column != 4 )
                CurrentCellType = CellsType.Cell2;

            else if (rackVm.Row == 4)
                CurrentCellType = CellsType.Cell3;
        }

        private void FilterItemsToIssue()
        {
            if (string.IsNullOrWhiteSpace(SearchtemsToIssue))
            {
                ReplaceCollection(FilteredItemsToIssue,
                    _allStocks.OrderByDescending(x => x.ComponentName));
                return;
            }

            var text = SearchtemsToIssue;

            var filteredStocks = _allStocks.Where(FilterPredicate);
            var filteredComponents = _allComponents.Where(FilterPredicate);

            var stockIds = _allStocks
                .Select(x => x.ComponentId)
                .ToHashSet();

            var result = filteredStocks
                        .Concat(filteredComponents.Where(c =>
                        !filteredStocks.Any(s => s.ComponentId == c.ComponentId)));

            ReplaceCollection(FilteredItemsToIssue,
                result.OrderByDescending(x => x.ComponentName));
        }

        private bool FilterPredicate(ViewItemDTO c)
        {
            return
                (c.Article?.Contains(SearchtemsToIssue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.ComponentName?.Contains(SearchtemsToIssue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Manufacturer?.Contains(SearchtemsToIssue, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private void FilterRacks()
        {
            var query = _racks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchRacks))
            {
                var text = SearchRacks.ToLower();

                query = query.Where(c =>
                        c.RackCode != null
                        && c.RackCode.ToLower().Contains(text));
            }

            var sortedQuery = query
                             .OrderByDescending(r => r.Column)
                             .ThenByDescending(r => r.Row);

            ReplaceCollection(FilteredRacks, sortedQuery);
        }

        private void AutomaticRackSelection()
        {
            var rack = FilteredRacks.FirstOrDefault(x => x.RackCode == SearchRacks);

            if (rack != null)
            {
                ReceiptItem.RackId = rack.Id;
            }
            else
            {
                ReceiptItem.RackId = Guid.Empty;
            }
        }

        private void FilterFreeCells()
        {
            if (ReceiptItem.RackId == Guid.Empty) return;

            var query = _cells.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchFreeCell))
            {
                var text = SearchFreeCell.ToLower();

                query = query.Where(c =>
                        c.CellCode != null
                        && c.CellCode.ToLower().Contains(text)
                        && c.RackId == ReceiptItem.RackId);
            }

            var sortedQuery = query
                             .OrderByDescending(r => r.Column)
                             .ThenByDescending(r => r.Row);

            ReplaceCollection(FilteredFreeCells, query);
        }

        private void AutomaticCellSelection()
        {
            var cell = FilteredFreeCells.FirstOrDefault(x => x.CellCode == SearchFreeCell);

            if (cell != null && ReceiptItem.RackId == cell.RackId)
            {
                ReceiptItem.CellId = cell.Id;
            }
            else
            {
                ReceiptItem.CellId = Guid.Empty;
            }
        }

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
                    ReceiptItem.RackId = stock.RackId;
                    SearchRacks = stock.RackCode == "-" ? string.Empty : stock.RackCodeDisplay;
                    ReceiptItem.CellId = stock.CellId;
                    SearchFreeCell = stock.CellCode == "-" ? string.Empty : stock.CellCodeDisplay;
                }

                _freeCells.Clear();
                _freeCells.AddRange(await _cellService.GetFreeCellsAsync(ReceiptItem.ComponentId));
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

        private void SetRackFromList(object obj)
        {
            if (obj is not Rack rack)
                return;

            ReceiptItem.RackId = rack.Id;
            SearchRacks = rack.RackCode;
        }

        private void SetCellFromList(object obj)
        {
            if (obj is not Cell cell)
                return;

            ReceiptItem.CellId = cell.Id;
            SearchFreeCell = cell.CellCode;
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
