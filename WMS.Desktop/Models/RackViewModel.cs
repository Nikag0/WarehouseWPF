using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WMS.Application.DTO;
using WMS.Domain;

namespace WMS.Desktop.Models
{
    public class RackViewModel : ObservableObject
    {
        public Guid Id { get; }
        public string Code { get; }
        public RackType Type { get; }
        public ObservableCollection<CellViewModel> Cells { get; }

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

        public RackViewModel(Guid id, string code, RackType rackType, List<CellViewModel> cells, double x, double y, double width, double height)
        {
            Id = id;
            Code = code;
            Type = rackType;

            Cells = new ObservableCollection<CellViewModel>(cells);

            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
