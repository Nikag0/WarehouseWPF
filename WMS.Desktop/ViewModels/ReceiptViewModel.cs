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
        private readonly List<ComponentDTO> _allComponents = new();
        public ObservableCollection<ComponentDTO> FilteredComponents { get; } = new();
        public string SearchComponents
        {
            get => _searchComponents;
            set
            {
                _searchComponents = value;
                OnPropertyChanged();
                FilterComponents();
            }
        }
        private string _searchComponents;

        // Отображение остатков. Средняя левая часть экрана ReceiptView.
        public ObservableCollection<StockViewDto> Stocks { get; } = new();

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

        // SelectedRack - свойство решает, ячейки какого стеллажа будут визуализироваться.
        public RackViewModel SelectedRack
        {
            get => _selectedRack;
            set
            {
                _selectedRack = value;
                OnPropertyChanged(nameof(VisibleCells));
            }
        }
        private RackViewModel _selectedRack;
        public IEnumerable<RackViewModel> NormalVisibleRacks =>
                racksVisualise.Where(r => r.Row != 5);
        public IEnumerable<RackViewModel> SpecialVisibleRacks =>
                racksVisualise.Where(r => r.Row == 5);
        public IEnumerable<CellViewModel> VisibleCells =>
                cellsVisualise.Where(c => c.RackId == SelectedRack.Id);

        private readonly SemaphoreSlim _lock = new(1, 1);

        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private readonly CellService _cellService;
        private readonly RackService _rackService;
        private readonly DialogService _dialogService;
        private readonly OperatorService _operatorrService;

        public ICommand AddItemToReceiptCommand { get; }
        public ICommand SetRackFromListCommand { get; }
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
            AddItemToReceiptCommand = new RelayCommand(AddItemToReceipt);
            AddItemToReceiptCommand = new RelayCommand(AddItemToReceipt);
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

            if (OperatorName == null)
            {
                _dialogService.ShowWarning("Оператор не указан.");
                return;
            }

            await _lock.WaitAsync();

            try
            {
                var receiptDto = new OperationDTO(
                    ReceiptItem.ComponentId,
                    ReceiptItem.RackId,
                    ReceiptItem.CellId,
                    ReceiptItem.OperationQuantity);

                ReceiptItem.RackId = Guid.Empty;
                SearchRacks = string.Empty;
                ReceiptItem.CellId = Guid.Empty;
                SearchFreeCell = string.Empty;
                ReceiptItem.OperationQuantity = 0;

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

                _racks.Clear();
                _racks.AddRange(await _rackService.GetAllAsync());

                _cells.Clear();
                _cells.AddRange(await _cellService.GetAllAsync());

                ReplaceCollection(Stocks, await _stockService.GetAllAsync());

                FilterComponents();
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

        private void FilterComponents()
        {
            var query = _allComponents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchComponents))
            {
                var text = SearchComponents.ToLower();

                query = query.Where(c =>
                        (c.Article?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (c.Name?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (c.Manufacturer?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            var sortedQuery = query.OrderByDescending(r => r.Name);

            ReplaceCollection(FilteredComponents, query);
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
                             .OrderByDescending(r => r.Row)
                             .ThenByDescending(r => r.RackNum);

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
                             .OrderByDescending(r => r.Line)
                             .ThenByDescending(r => r.Column);

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
                if (obj is ComponentDTO component)
                {
                    ReceiptItem.ComponentId = component.Id;
                    ReceiptItem.Article = component.Article;
                    ReceiptItem.ComponentName = component.Name;
                    ReceiptItem.Manufacturer = component.Manufacturer;
                    ReceiptItem.RackId = Guid.Empty;
                    ReceiptItem.CellId = Guid.Empty;
                }

                if (obj is StockViewDto stock)
                {
                    ReceiptItem.ComponentId = stock.ComponentId;
                    ReceiptItem.Article = stock.Article;
                    ReceiptItem.ComponentName = stock.ComponentName;
                    ReceiptItem.Manufacturer = stock.Manufacturer;
                    ReceiptItem.RackId = stock.RackId;
                    SearchRacks = stock.RackCode;
                    ReceiptItem.CellId = stock.CellId;
                    SearchFreeCell = stock.CellCode;
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
