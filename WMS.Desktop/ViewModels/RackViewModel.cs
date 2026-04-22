using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Desktop.ViewModels
{
    public class RackViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; }

        public string Code { get; }
        public int Column { get; }
        public int Row { get; }
        public string CodeDisplay => LocationFormatter.CodeToDisplay(Column, Row);
        public RackType Type { get; }

        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                _isHighlighted = value;
                OnPropertyChanged(nameof(IsHighlighted));
            }
        }

        public RackViewModel(Rack rack, RackLayout layout)
        {
            Id = rack.Id;
            Code = rack.RackCode;
            Column = rack.Column;
            Row = rack.Row;
            Type = layout.Type;

            X = layout.X;
            Y = layout.Y;
            Width = layout.Width;
            Height = layout.Height;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
