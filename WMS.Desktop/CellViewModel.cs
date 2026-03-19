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
        private int _row;
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

        private int _rackNum;
        public int RackNum
        {
            get => _rackNum;
            set
            {
                _rackNum = value;
                OnPropertyChanged(nameof(RackNum));
            }
        }

        private int _line;
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

        private int _column;
        public int Column
        {
            get => _column;
            set
            {
                _column = value;
                OnPropertyChanged(nameof(Column));
            }
        }

        public Cell Cell { get; }

        private readonly ObservableCollection<IssueStockDto> _issueItems;
        public bool IsHighlighted =>
            _issueItems.Any(i => i.CellId == Cell.Id);

        public CellViewModel(Cell cell, ObservableCollection<IssueStockDto> issueItems)
        {
            Cell = cell;
            _issueItems = issueItems;

            _issueItems.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(IsHighlighted));
            };

            Row = cell.Rack.Row;
            RackNum = cell.Rack.RackNum;
            Line = cell.Line;
            Column = cell.Column;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
