using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WMS.Application.Services;

namespace WMS.Desktop.ViewModels
{
    public class IssueViewModel : INotifyCollectionChanged
    {
        public int IssueQuantity { get; set; }
        public ObservableCollection<StockItemDto> Stocks { get; } = new();
        public Guid SelectedComponentId { get; set; }
        public Guid SelectedCellId { get; set; }

        public ICommand IssueCommand { get; }

        private readonly StockService _stockService;
        private readonly IssueService _issueService;
        private bool _isLoading;

        public IssueViewModel(
            StockService stockService, 
            IssueService issueService)
        {
            _issueService = issueService;
            _stockService = stockService;
            IssueCommand = new RelayCommand(IssueAsync);

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
            }
            finally
            {
                _isLoading = false;
            }
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
    }
}
