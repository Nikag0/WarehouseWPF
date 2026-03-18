using System;
using System.Collections.Generic;
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

        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                _isHighlighted = value;
                OnPropertyChanged();
            }
        }

        public RackViewModel(Rack rack)
        {
            Id = rack.Id;
            Row = rack.Row;
            RackNum = rack.RackNum;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
