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
        public int Row
        {
            get
            {
                return _row switch
                {
                    1 => 1,
                    2 => 3,
                    3 => 5,
                    4 => 6,
                    5 => 8
                };
            }
            set
            {
                _row = value;
                OnPropertyChanged(nameof(Row));
            }
        }
        public int RackNum
        {
            get => _rackNum;
            set
            {
                _rackNum = value;
                OnPropertyChanged(nameof(RackNum));
            }
        }
        public bool IsSelectedRack =>
                   _selectedItemsToIssue != null && _selectedItemsToIssue.Any(x => x.RackId == Id)
                   ||
                   _selectedItemToReceipt != null && _selectedItemToReceipt.RackId == Id;

        private int _row;
        private int _rackNum;
        private readonly ObservableCollection<ViewItemDTO> _selectedItemsToIssue;
        private readonly StockViewModel _selectedItemToReceipt;

        public RackViewModel(Rack rack, ObservableCollection<ViewItemDTO> selectedItemsToIssue)
        {
            Id = rack.Id;
            Row = rack.Column;
            RackNum = rack.Row;

            _selectedItemsToIssue = selectedItemsToIssue;
            _selectedItemsToIssue.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(IsSelectedRack));
            };
        }

        public RackViewModel(Rack rack, StockViewModel selectedItemToReceipt)
        {
            Id = rack.Id;
            Row = rack.Column;
            RackNum = rack.Row;

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
