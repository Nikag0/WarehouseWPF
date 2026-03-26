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
        public ObservableCollection<StockDto> Stocks { get; } = new();
        public ObservableCollection<ReceiptStockDto> FilteredComponents { get; } = new();
        public ObservableCollection<Cell> FilteredFreeCells { get; } = new();
        public ObservableCollection<Operator> Operators { get; } = new();
        public ReceiptStockDto ItemToReceipt
        {
            get => _itemToReceipt;
            set
            {
                if (_itemToReceipt != value)
                {
                    _itemToReceipt = value;
                    OnPropertyChanged();
                }
            }
        }
        public Guid CellId
        {
            get => _cellId;
            set
            {

                _cellId = value;
                OnPropertyChanged();
            }
        }
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
        public string SearchFreeCell
        {
            get => _searchFreeCell;
            set
            {
                _searchFreeCell = value;
                OnPropertyChanged();
                ApplyCellFilter();
            }
        }
        public string CommentText
        {
            get => _commentText;
            set
            {
                _commentText = value;
                OnPropertyChanged();
            }
        }
        public Rack SelectedRack
        {
            get => _selectedRacks;
            set
            {
                _selectedRacks = value;
                OnPropertyChanged();
            }
        }
        public RackViewModel SelectedRackView
        {
            get => _selectedRackView;
            set
            {
                _selectedRackView = value;
                OnPropertyChanged();
            }
        }
        public Operator OperatorName
        {
            get => _operatorName;
            set
            {
                _operatorName = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<CellViewModel> Cells { get; } = new();
        public ObservableCollection<Rack> Racks { get; } = new();
        public ObservableCollection<RackViewModel> RacksView { get; set; } = new();

        public IEnumerable<CellViewModel> VisibleCells =>
                Cells.Where(c => c.RackId == SelectedRack.Id);
        public IEnumerable<RackViewModel> NormalRacks =>
                RacksView.Where(r => r.Row != 5);
        public IEnumerable<RackViewModel> SpecialRacks =>
                RacksView.Where(r => r.Row == 5);

        private readonly List<ReceiptStockDto> _allComponents = new();
        private readonly List<Cell> _freeCells = new();
        private ReceiptStockDto _itemToReceipt;
        private Guid _cellId;
        private bool _isLoading;
        private string _searchText;
        private string _searchFreeCell;
        private string _commentText;
        private Rack _selectedRacks;
        private RackViewModel _selectedRackView;
        private Operator _operatorName;

        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private readonly CellService _cellService;
        private readonly RackService _rackService;
        private readonly DialogService _dialogService;
        private readonly OperatorService _operatorrService;

        public ICommand RefreshCommand { get; }
        public ICommand ReceiveCommand { get; }
        public ICommand AddSelectedComponentCommand { get; }
        public ICommand AddSelectedCellCommand { get; }
        public ICommand AddToReceiptCommand { get; }

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

            RefreshCommand = new RelayCommand(RefreshAsync);
            ReceiveCommand = new RelayCommand(ReceiveAsync);
            AddSelectedComponentCommand = new RelayCommand(AddSelectedStock);
            AddSelectedCellCommand = new RelayCommand(AddSelectedCell);
            AddToReceiptCommand = new RelayCommand(AddToReceipt);

            RacksView.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(NormalRacks));
                OnPropertyChanged(nameof(SpecialRacks));
            };
        }

        public async Task ReceiveAsync()
        {
            if (ItemToReceipt == null || CellId == Guid.Empty) return;

            if (OperatorName == null)
            {
                _dialogService.ShowWarning("Оператор не указан");
                return;
            }

            try
            {
                var receiptDto = new OperationDTO(
                    ItemToReceipt.ComponentId,
                    SelectedRack.Id,
                    CellId,
                    ItemToReceipt.Quantity);
                await _receiptService.ReceiveAsync(receiptDto, OperatorName.FullName, CommentText);
                await RefreshAsync();
                SearchFreeCell = string.Empty;
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
        }

        public async Task RefreshAsync()
        {
            if (_isLoading) return;

            _isLoading = true;

            try
            {
                _allComponents.Clear();
                _allComponents.AddRange(await _receiptService.GetAllComponentsAsync());

                _freeCells.Clear();
                _freeCells.AddRange(await _cellService.GetFreeCellsAsync(ItemToReceipt?.ComponentId));

                ReplaceCollection(Stocks, await _stockService.GetAllAsync());

                ApplyFilter();
                ApplyCellFilter();
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
                _isLoading = false;
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
            finally
            {
                _isLoading = false;
            }
        }

        public async Task LoadRacks()
        {
            try
            {
                RacksView.Clear();
                var items = await _rackService.GetAllAsync();
                foreach (var item in items)
                {
                    RacksView.Add(new RackViewModel(item, Stocks));
                    Racks.Add(item);
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
            finally
            {
                _isLoading = false;
            }
        }

        public async Task LoadCells()
        {
            try
            {
                Cells.Clear();
                var items = await _cellService.GetAllAsync();
                foreach (var item in items)
                    Cells.Add(new CellViewModel(item, Stocks));
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
                _isLoading = false;
            }
        }

        public void AddToReceipt(/*ReceiptStockDto selectedStock*/)
        {
            //if (selectedStock == null) return;

            //ItemToReceipt.ComponentId = selectedStock.ComponentId; 
            //ItemToReceipt.RackId = selectedStock.RackId; 
            //ItemToReceipt.CellId = selectedStock.CellId; 
            //ItemToReceipt.Article = selectedStock.Article; 
            //ItemToReceipt.ComponentName = selectedStock.ComponentName; 
            //ItemToReceipt.Manufacturer = selectedStock.Manufacturer; 
            //ItemToReceipt.CellCode = selectedStock.CellCode;
            //ItemToReceipt.Quantity = selectedStock.Quantit; 
        }

        private void ApplyFilter()
        {
            var query = _allComponents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.ToLower();

                query = query.Where(c =>
                        (c.Article?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (c.ComponentName?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (c.Manufacturer?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            ReplaceCollection(FilteredComponents, query);
        }

        private void ApplyCellFilter()
        {
            if (SelectedRack == null) return;

            var query = SelectedRack.Cells.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchFreeCell))
            {
                var text = SearchFreeCell.ToLower();

                query = query.Where(c =>
                        c.Code != null

                        && c.Code.ToLower().Contains(text));
            }

            ReplaceCollection(FilteredFreeCells, query);
        }

        private async Task AddSelectedStock(object obj)
        {
            if (obj is not ReceiptStockDto item)
                return;

            ItemToReceipt = item;
            SearchText = null;
            await RefreshAsync();
        }

        private void AddSelectedCell(object obj)
        {
            if (obj is not Cell item)
                return;

            CellId = item.Id;
            SearchFreeCell = item.Code;
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
