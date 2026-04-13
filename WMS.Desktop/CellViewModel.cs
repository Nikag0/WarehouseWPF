using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using WMS.Desktop.ViewModels;
using WMS.Domain;

namespace WMS.Desktop
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
        public bool IsSelectToIssue =>
            _selectedItemsToIssue.Any(x=> x.CellId == Id);
        public bool IsSelectToReceipt =>
           _selectedItemToReceipt.CellId == Id;

        private int _line;
        private int _column;
        private readonly ObservableCollection<StockViewDto> _selectedItemsToIssue;
        private readonly StockViewModel _selectedItemToReceipt;

        public CellViewModel(Cell cell, ObservableCollection<StockViewDto> selectedItemsToIssue)
        {
            Id = cell.Id;
            RackId = cell.RackId;
            Line = cell.Line;
            Column = cell.Column;

            _selectedItemsToIssue = selectedItemsToIssue;
            _selectedItemsToIssue.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(IsSelectToIssue));
            };
        }

        public CellViewModel(Cell cell, StockViewModel selectedItemToReceipt)
        {
            Id = cell.Id;
            RackId = cell.RackId;
            Line = cell.Line;
            Column = cell.Column;

            _selectedItemToReceipt = selectedItemToReceipt;
            _selectedItemToReceipt.PropertyChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(IsSelectToReceipt));
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
