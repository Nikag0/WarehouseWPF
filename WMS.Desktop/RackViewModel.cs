using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Desktop
{
    public class RackViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; }
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
        public bool IsHighlighted =>
           _issueItems.Any(i => i.RackId == Id);

        private int _row;
        private int _rackNum;
        private readonly ObservableCollection<IssueStockDto> _issueItems;

        public RackViewModel(Rack rack, ObservableCollection<IssueStockDto> issueItems)
        {
            Id = rack.Id;
            Row = rack.Row;
            RackNum = rack.RackNum;

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
