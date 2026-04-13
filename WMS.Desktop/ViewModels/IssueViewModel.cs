using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels
{
    public class IssueViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<StockViewDto> IssueItems { get; } = new();
        public ObservableCollection<StockViewDto> FilteredStocks { get; } = new();
        public ObservableCollection<Operator> Operators { get; } = new();
        private ObservableCollection<RackViewModel> allRacksToVisualise { get; set; } = new();
        private ObservableCollection<CellViewModel> allCellsToVisualise { get; } = new();
    
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
        public string CommentText
        {
            get => _commentText;
            set
            {
                _commentText = value;
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
        public bool IsIssue
        {
            get => _isIssue;
            set
            {
                _isIssue = value;
                OnPropertyChanged();
            }
        }
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
            allRacksToVisualise.Where(r => r.Row != 5);
        public IEnumerable<RackViewModel> SpecialVisibleRacks =>
            allRacksToVisualise.Where(r => r.Row == 5);
        public IEnumerable<CellViewModel> VisibleCells =>
            allCellsToVisualise.Where(c => c.RackId == SelectedRack.Id);

        private readonly List<StockViewDto> _stocks = new();
        private string _searchText;
        private string _commentText;
        private Operator _operatorName;
        private bool _isLoading;
        private bool _isIssue;

        public ICommand IssueCommand { get; }
        public ICommand AddIssueItemCommand { get; }
        public ICommand RemoveIssueItemCommand { get; }
        public ICommand ClearIssueItemsCommand { get; }

        private readonly IssueService _issueService;
        private readonly DialogService _dialogService;
        private readonly OperatorService _operatorService;
        private readonly CellService _cellService;
        private readonly RackService _rackService;

        public IssueViewModel(
            StockService stockService,
            IssueService issueService,
            DialogService dialogService,
            OperatorService operatorService,
            CellService cellService,
            RackService rackService)
        {
            _issueService = issueService;
            _dialogService = dialogService;
            _operatorService = operatorService;
            _cellService = cellService;
            _rackService = rackService;

            IssueCommand = new RelayCommand(IssueAsync);
            AddIssueItemCommand = new RelayCommand(AddIssueItem);
            RemoveIssueItemCommand = new RelayCommand(RemoveIssueItem);
            ClearIssueItemsCommand = new RelayCommand(ClearIssueItems);

            allRacksToVisualise.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(NormalVisibleRacks));
                OnPropertyChanged(nameof(SpecialVisibleRacks));
            };
        }

        public async Task IssueAsync()
        {
            if (!IssueItems.Any()) return;

            if (OperatorName == null)
            {
                _dialogService.ShowWarning("Оператор не указан");
                return;
            }

            try
            {
                var issueOperation = IssueItems
                    .Select(item => new OperationDTO(
                        item.ComponentId,
                        item.RackId,
                        item.CellId,
                        item.OperationQuantity))
                    .ToList();

                await _issueService.IssueAsync(issueOperation, OperatorName.FullName, CommentText);

                await RefreshAsync();
                CommentText = string.Empty;
                IsIssue = true;
                _dialogService.ShowInfo("Выдача успешно выполнена.");
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
                _stocks.Clear();
                var items = await _issueService.GetAllAsync();
                foreach (var item in items)
                    _stocks.Add(item);

                ApplyFilter();
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
                var items = await _operatorService.GetAllAsync();
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

        public async Task LoadWarehouseVisualise()
        {
            try
            {
                allRacksToVisualise.Clear();
                var racks = await _rackService.GetAllAsync();
                foreach (var item in racks)
                    allRacksToVisualise.Add(new RackViewModel(item, IssueItems));

                allCellsToVisualise.Clear();
                var cells = await _cellService.GetAllAsync();
                foreach (var item in cells)
                    allCellsToVisualise.Add(new CellViewModel(item, IssueItems));
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

        private void ApplyFilter()
        {
            FilteredStocks.Clear();

            var query = _stocks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.ToLower();

                query = query.Where(c =>
                    c.Quantity > 0 && (
                    (c.Article?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.ComponentName?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.CellCode?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
                ));
            }

            foreach (var item in query)
                FilteredStocks.Add(item);
        }

        private void AddIssueItem(object obj)
        {
            if (obj is not StockViewDto item)
                return;

            if (IssueItems.Any(x => x.ComponentId == item.ComponentId && x.CellId == item.CellId))
                return;

            IssueItems.Add(item);
        }

        private void RemoveIssueItem (object obj)
        {
            if (obj is not StockViewDto item)
                return;

            IssueItems.Remove(item);
        }

        private void ClearIssueItems()
        {
            IssueItems.Clear();
            IsIssue = false; 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
