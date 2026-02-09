using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Infrastructure.Migrations;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels
{
    public class ReceiptViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<StockDto> Stocks { get; } = new ();
        public ObservableCollection<ReceiptStockDto> FilteredComponents { get; } = new ();
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

        private Collection<ReceiptStockDto> Components = new();
        private ReceiptStockDto _itemToReceipt;
        private bool _isLoading;
        private string _searchText;

        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private readonly CellService _cellService;

        public ICommand RefreshCommand { get; } 
        public ICommand ReceiveCommand { get; }
        public ICommand AddSelectedComponentCommand { get; }

        public ReceiptViewModel(
            StockService stockService, 
            ReceiptService receiptService,
            CellService cellService)
        {
            _stockService = stockService;
            _receiptService = receiptService;
            _cellService = cellService;

            RefreshCommand = new RelayCommand(RefreshAsync);
            ReceiveCommand = new RelayCommand(ReceiveAsync);
            AddSelectedComponentCommand = new RelayCommand(AddSelectedStock);
        }

        public async Task ReceiveAsync()
        {
            var receiptDto = new OperationDTO(
                ItemToReceipt.ComponentId,
                ItemToReceipt.CellId,
                ItemToReceipt.Quantity);
            await _receiptService.ReceiveAsync(receiptDto);
            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            if (_isLoading) return;
            
            try
            {
                Components.Clear();
                var itemCpmponent = await _receiptService.GetAllComponentsAsync();
                foreach (var item in itemCpmponent)
                    Components.Add(item);

                Stocks.Clear();
                var itemStock = await _stockService.GetAllAsync();
                foreach (var item in itemStock)
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
            FilteredComponents.Clear();

            var query = Components.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.ToLower();

                query = query.Where(c =>
                        c.Article != null && c.Article.ToLower().Contains(text) ||
                        c.ComponentName!= null && c.ComponentName.ToLower().Contains(text) ||
                        c.Manufacturer != null && c.Manufacturer.ToLower().Contains(text));
            }

            foreach (var item in query)
                FilteredComponents.Add(item);
        }

        private void AddSelectedStock(object obj)
        {
            if (obj is not ReceiptStockDto item)
                return;

            ItemToReceipt = item;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string ?propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
