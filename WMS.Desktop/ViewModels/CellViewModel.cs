using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using WMS.Application.WarehouseVisualization;
using WMS.Domain;

namespace WMS.Desktop.ViewModels
{
    public class CellViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; }
        public Guid RackId { get; }
        public string Code { get; }
        public int Column { get; }
        public int Row { get; }
        public string CodeDisplay => LocationFormatter.CodeToDisplay(Column, Row);

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

        private bool _hasItemsInCell;
        public bool HasItemsInCell
        {
            get => _hasItemsInCell;
            set
            {
                _hasItemsInCell = value;
                OnPropertyChanged(nameof(HasItemsInCell));
            }
        }

        public CellViewModel(Cell cell, CellLayout layout)
        {
            Id = cell.Id;
            Code = cell.CellCode;
            Column = cell.Column;
            Row = cell.Row;
            RackId = cell.RackId;

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
