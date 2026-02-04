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
        public ObservableCollection<StockItemDto> Stocks { get; } = new ();
        public ObservableCollection<Component> Components { get; } = new ();
        public ObservableCollection<Cell> Cells { get; } = new ();
        public ObservableCollection<string> Rows { get; } = new();
        public ObservableCollection<int> Racks { get; } = new();
        public ObservableCollection<int> Positions { get; } = new();

        public Guid SelectedComponentId { get; set; }
        public Guid SelectedStockId { get; set; }
        public Guid SelectedCellId { get; set; }

        private string? _selectedRow;
        public string? SelectedRow
        {
            get => _selectedRow;
            set
            {
                _selectedRow = value;
                OnPropertyChanged();
                LoadRacks();
            }
        }

        private int? _selectedRack;
        public int? SelectedRack
        {
            get => _selectedRack;
            set
            {
                _selectedRack = value;
                OnPropertyChanged();
                LoadPositions();
            }
        }

        private int? _selectedPosition;
        public int? SelectedPosition
        {
            get => _selectedPosition;
            set
            {
                _selectedPosition = value;
                OnPropertyChanged();
                ResolveCellId();
            }
        }

        public int ReceiveQuantity { get; set; }

        private bool _isLoading;

        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private readonly ComponentService _componentService;
        private readonly CellService _cellService;

        public ICommand RefreshCommand { get; } 
        public ICommand ReceiveCommand { get; }
        
        public ReceiptViewModel(
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

        public async Task ReceiveAsync()
        {
            var receiptDto = new StockOperationDto(SelectedComponentId, SelectedCellId, ReceiveQuantity);
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

            LoadRows();
        }

        private void ResolveCellId()
        {
            if (SelectedRow == null || SelectedRack == null || SelectedPosition == null)
                return;

            var cell = Cells.FirstOrDefault(x =>
                x.Row == SelectedRow &&
                x.Rack == SelectedRack &&
                x.Position == SelectedPosition);

            SelectedCellId = cell?.Id ?? Guid.Empty;
        }

        private void LoadRows()
        {
            Rows.Clear();
            foreach (var r in Cells.Select(x => x.Row).Distinct())
                Rows.Add(r);
        }

        private void LoadRacks()
        {
            Racks.Clear();
            Positions.Clear();
            SelectedRack = null;
            SelectedPosition = null;

            if (SelectedRow == null) return;

            foreach (var r in Cells
                .Where(x => x.Row == SelectedRow)
                .Select(x => x.Rack)
                .Distinct())
                Racks.Add(r);
        }

        private void LoadPositions()
        {
            Positions.Clear();
            SelectedPosition = null;

            if (SelectedRow == null || SelectedRack == null) return;

            foreach (var p in Cells
                .Where(x => x.Row == SelectedRow && x.Rack == SelectedRack)
                .Select(x => x.Position)
                .Distinct())
                Positions.Add(p);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string ?propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
