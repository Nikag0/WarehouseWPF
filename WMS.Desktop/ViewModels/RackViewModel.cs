using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Desktop.ViewModels
{
    public class RackViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; set;}
        public int Column
        {
            get => _column;
            set
            {
                _column = value;
                OnPropertyChanged(nameof(Column));
            }
        }
        public int Row
        {
            get => _row switch
            {
                1 => 1,
                2 => 3,
                3 => 4,
                4 => 6,
                5 => 8
            };
            set
            {
                _row = value;
                OnPropertyChanged(nameof(Row));
            }
        }
        public bool IsSelectedRack =>
                   _selectedItemsToIssue != null && _selectedItemsToIssue.Any(x => x.RackId == Id)
                   ||
                   _selectedItemToReceipt != null && _selectedItemToReceipt.RackId == Id;

        private int _column;
        private int _row;
        private readonly ObservableCollection<ViewItemDTO> _selectedItemsToIssue;
        private readonly StockViewModel _selectedItemToReceipt;

        public RackViewModel(Rack rack, ObservableCollection<ViewItemDTO> selectedItemsToIssue)
        {
            Id = rack.Id;
            Column = rack.Column;
            Row = rack.Row;

            _selectedItemsToIssue = selectedItemsToIssue;
            _selectedItemsToIssue.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(IsSelectedRack));
            };
        }

        public RackViewModel(Rack rack, StockViewModel selectedItemToReceipt)
        {
            Id = rack.Id;
            Column = rack.Column;
            Row = rack.Row;

            _selectedItemToReceipt = selectedItemToReceipt;
            _selectedItemToReceipt.PropertyChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(IsSelectedRack));
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
