using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WMS.Application.Services;
using WMS.Domain;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels
{
    public class IssueViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<StockItemDto> Stocks { get; } = new();
        public ObservableCollection<StockItemDto> FilteredStockItemDto { get; } = new();
        public ObservableCollection<StockItemDto> IssueStockItemDto { get; } = new();

        public int IssueQuantity { get; set; }
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

        public Guid SelectedComponentId { get; set; }
        public Guid SelectedCellId { get; set; }

        public ICommand IssueCommand { get; }
        public ICommand AddStockToIssueCommand { get; }

        private readonly StockService _stockService;
        private readonly IssueService _issueService;
        private bool _isLoading;
        private string _searchText;

        public IssueViewModel(
            StockService stockService, 
            IssueService issueService)
        {
            _issueService = issueService;
            _stockService = stockService;
            IssueCommand = new RelayCommand(IssueAsync);
            AddStockToIssueCommand = new RelayCommand(IssueAsync);

        }

        public async Task IssueAsync()
        {
            var issueDto = new StockOperationDto(SelectedComponentId, SelectedCellId, IssueQuantity);
            await _issueService.IssueAsync(issueDto);
            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            if (_isLoading) return;

            try
            {
                Stocks.Clear();
                var items = await _stockService.GetAllAsync();
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
            FilteredStockItemDto.Clear();

            var query = Stocks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.ToLower();

                query = query.Where(c =>
                        c.Article != null && c.Article.ToLower().Contains(text) ||
                        c.ComponentName != null && c.ComponentName.ToLower().Contains(text) ||
                        c.CellCode != null && c.CellCode.ToLower().Contains(text));
            }

            foreach (var item in query)
                FilteredStockItemDto.Add(item);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
