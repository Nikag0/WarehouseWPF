using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WMS.Application.Services;
using WMS.Domain;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels
{
    public class IssueViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<IssueStockDto> IssueItems { get; } = new();
        public ObservableCollection<IssueStockDto> FilteredStocks { get; } = new();
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

        private IssueStockDto _selectedIssueItem;
        private Collection<IssueStockDto> Stocks = new();
        private bool _isLoading;
        private string _searchText;

        public ICommand IssueCommand { get; }
        public ICommand AddIssueItemCommand { get; }
        public ICommand RemoveIssueItemCommand { get; }

        private readonly StockService _stockService;
        private readonly IssueService _issueService;

        public IssueViewModel(
            StockService stockService, 
            IssueService issueService)
        {
            _issueService = issueService;
            _stockService = stockService;
            IssueCommand = new RelayCommand(IssueAsync);
            AddIssueItemCommand = new RelayCommand(AddIssueItem);
            RemoveIssueItemCommand = new RelayCommand(RemoveIssueItem);

            // Привязка для срабатывания конвертора RackHighlightConverter
            IssueItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IssueItems));
            };
        }

        public async Task IssueAsync()
        {
            Collection<OperationDTO> issueOperation = new();

            try
            {
                foreach (var item in IssueItems)
                {
                    issueOperation.Add(new OperationDTO(
                        item.ComponentId,
                        item.CellId,
                        item.IssueQuantity));
                }

                await _issueService.IssueAsync(issueOperation);
                await RefreshAsync();
                IssueItems.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                        c.Quantity > 0 &&(
                        c.Article != null && c.Article.ToLower().Contains(text) ||
                        c.ComponentName != null && c.ComponentName.ToLower().Contains(text) ||
                        c.CellCode != null && c.CellCode.ToLower().Contains(text)));
            }

            foreach (var item in query)
                FilteredStocks.Add(item);
        }

        private void AddIssueItem(object obj)
        {
            if (obj is not IssueStockDto item)
                return;

            if (IssueItems.Contains(item))
                return;

            IssueItems.Add(item);
        }

        private void RemoveIssueItem (object obj)
        {
            if (obj is not IssueStockDto item)
                return;

            IssueItems.Remove(item);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
