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
        public ObservableCollection<IssueStockDto> IssueItems { get; } = new();
        public ObservableCollection<IssueStockDto> FilteredStocks { get; } = new();
        public ObservableCollection<Operator> Operators { get; } = new();
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


        private IssueStockDto _selectedIssueItem;
        private readonly List<IssueStockDto> Stocks = new();
        private string _searchText;
        private string _commentText;
        private Operator _operatorName;
        private bool _isLoading;
        private bool _isIssue;

        public ICommand IssueCommand { get; }
        public ICommand AddIssueItemCommand { get; }
        public ICommand RemoveIssueItemCommand { get; }
        public ICommand ClearIssueItemsCommand { get; }

        private readonly StockService _stockService;
        private readonly IssueService _issueService;
        private readonly DialogService _dialogService;
        private readonly OperatorService _userService;


        public IssueViewModel(
            StockService stockService, 
            IssueService issueService,
            DialogService dialogService,
            OperatorService userService)
        {
            _issueService = issueService;
            _stockService = stockService;
            _dialogService = dialogService;
            _userService = userService;

            IssueCommand = new RelayCommand(IssueAsync);
            AddIssueItemCommand = new RelayCommand(AddIssueItem);
            RemoveIssueItemCommand = new RelayCommand(RemoveIssueItem);
            ClearIssueItemsCommand = new RelayCommand(ClearIssueItems);

            // Привязка для срабатывания конвертора RackHighlightConverter.
            IssueItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IssueItems));
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
                        item.CellId,
                        item.IssueQuantity))
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
                Stocks.Clear();
                var items = await _issueService.GetAllAsync();
                foreach (var item in items)
                    Stocks.Add(item);

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

        public async Task LoadUsers()
        {
            try
            {
                Operators.Clear();
                var items = await _userService.GetAllAsync();
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

        private void ApplyFilter()
        {
            FilteredStocks.Clear();

            var query = Stocks.AsEnumerable();

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
            if (obj is not IssueStockDto item)
                return;

            if (IssueItems.Any(x => x.ComponentId == item.ComponentId && x.CellId == item.CellId))
                return;

            IssueItems.Add(item);
        }

        private void RemoveIssueItem (object obj)
        {
            if (obj is not IssueStockDto item)
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
