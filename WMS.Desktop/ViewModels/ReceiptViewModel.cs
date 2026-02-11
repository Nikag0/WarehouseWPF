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
        public ObservableCollection<Cell> FilteredFreeCells { get; } = new ();
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
                ApplyFilter();
            }
        }

        private Collection<ReceiptStockDto> Components = new();
        private Collection<Cell> FreeCells = new();
        private Guid _cellId;
        private ReceiptStockDto _itemToReceipt;
        private bool _isLoading;
        private string _searchText;
        private string _searchFreeCell;

        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private readonly CellService _cellService;

        public ICommand RefreshCommand { get; } 
        public ICommand ReceiveCommand { get; }
        public ICommand AddSelectedComponentCommand { get; }
        public ICommand AddSelectedCellCommand { get; }

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
            AddSelectedCellCommand = new RelayCommand(AddSelectedCell);
        }

        public async Task ReceiveAsync()
        {
            var receiptDto = new OperationDTO(
                ItemToReceipt.ComponentId,
                CellId,
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

                FreeCells.Clear();
                var query = await _cellService.GetFreeCellsAsync(ItemToReceipt?.ComponentId);
                foreach (var item in query)
                    FreeCells.Add(item);

                ApplyFilter();
                ApplyCellFilter();
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

        private void ApplyCellFilter()
        {
            FilteredFreeCells.Clear();

            var query = FreeCells.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchFreeCell.ToLower();

                query = query.Where(c =>
                        c.Code != null && c.Code.ToLower().Contains(text));
            }

            foreach (var item in query)
                FilteredFreeCells.Add(item);
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string ?propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
