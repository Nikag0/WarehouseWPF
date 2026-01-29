using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels
{
    public class StockViewModel : INotifyPropertyChanged
    {
        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private readonly ComponentService _componentService;
        private readonly CellService _cellService;
        public ObservableCollection<StockItemDto> Stocks { get; } = new ();
        public ObservableCollection<Component> Components { get; } = new ();
        public ObservableCollection<Cell> Cells { get; } = new ();

        public Guid SelectedComponentId { get; set; }
        public Guid SelectedCellId { get; set; }
        public int Quantity { get; set; }

        public ICommand RefreshCommand { get; } 
        public ICommand ReceiveCommand { get; }

        public StockViewModel(
            StockService stockService, 
            ReceiptService receiptService,
            ComponentService componentService,
            CellService cellService)
        {
            _stockService = stockService;
            _receiptService = receiptService;
            _componentService = componentService;
            _cellService = cellService;
            RefreshCommand = new RelayCommand(RefreshAsync);
            ReceiveCommand = new RelayCommand(ReceiveAsync);
        }

        public async Task RefreshAsync()
        {
            Stocks.Clear();
            var items = await _stockService.GetAllAsync();
            foreach (var item in items)
                Stocks.Add(item);
        }

        public async Task ReceiveAsync()
        {
            var receiptDto = new ReceiptItemDto(SelectedComponentId, SelectedCellId, Quantity);
            await _receiptService.ReceiveAsync(receiptDto);
            await RefreshAsync();
        }

        public async Task LoadLookupsAsync()
        {
            Components.Clear();
            var comps = await _componentService.GetAllAsync();
            foreach (var copm in comps)
                Components.Add(copm);

            Cells.Clear();
            var cells = await _cellService.GetAllAsync();
            foreach (var cell in cells)
                Cells.Add(cell);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
