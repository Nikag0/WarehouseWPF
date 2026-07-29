using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using WMS.Application.DTO;
using WMS.Domain;

namespace WMS.Desktop.Models
{
    public class CellViewModel : ObservableObject
    {
        public Guid Id { get; }
        public string Code { get; }

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

        private bool _itemInCell;
        public bool ItemInCell
        {
            get => _itemInCell;
            set
            {
                _itemInCell = value;
                OnPropertyChanged(nameof(ItemInCell));
            }
        }

        public CellViewModel(Guid id, string code, double x, double y, double width, double height)
        {
            Id = id;

            Code = code;

            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

    }
}
