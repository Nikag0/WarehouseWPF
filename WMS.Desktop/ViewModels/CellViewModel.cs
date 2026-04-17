using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using WMS.Domain;

namespace WMS.Desktop.ViewModels
{
    public class CellViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; }
        public Guid RackId { get; }
        public int Line
        {
            get
            {
                return _line;
            }
            set
            {
                _line = value;
                OnPropertyChanged(nameof(Line));
            }
        }
        public int Column
        {
            get => _column;
            set
            {
                _column = value;
                OnPropertyChanged(nameof(Column));
            }
        }
        public int ColumnSpan => Line == 1 ? 4 : 1;

        public bool IsSelectedCell =>
                    _selectedItemsToIssue != null && _selectedItemsToIssue.Any(x => x.CellId == Id)
                    ||
                    _selectedItemToReceipt != null && _selectedItemToReceipt.CellId == Id;

        private int _line;
        private int _column;
        private readonly ObservableCollection<ViewItemDTO> _selectedItemsToIssue;
        private readonly StockViewModel _selectedItemToReceipt;

        public CellViewModel(Cell cell, ObservableCollection<ViewItemDTO> selectedItemsToIssue)
        {
            Id = cell.Id;
            RackId = cell.RackId;
            Line = cell.Column;
            Column = cell.Row;

            _selectedItemsToIssue = selectedItemsToIssue;
            _selectedItemsToIssue.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(IsSelectedCell));
            };
        }

        public CellViewModel(Cell cell, StockViewModel selectedItemToReceipt)
        {
            Id = cell.Id;
            RackId = cell.RackId;
            Line = cell.Column;
            Column = cell.Row;

            _selectedItemToReceipt = selectedItemToReceipt;
            _selectedItemToReceipt.PropertyChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(IsSelectedCell));
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
