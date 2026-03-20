using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using WMS.Domain;

namespace WMS.Desktop
{
    public class CellViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; }
        public Guid RackId { get; }
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
        public bool IsHighlighted =>
            _issueItems.Any(i => i.CellId == Id);

        private int _row;
        private int _rackNum;
        private int _line;
        private int _column;
        private readonly ObservableCollection<IssueStockDto> _issueItems;

        public CellViewModel(Cell cell, ObservableCollection<IssueStockDto> issueItems)
        {
            Id = cell.Id;
            RackId = cell.Rack.Id;
            Row = cell.Rack.Row;
            RackNum = cell.Rack.RackNum;
            Line = cell.Line;
            Column = cell.Column;

            _issueItems = issueItems;
            _issueItems.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(IsHighlighted));
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
