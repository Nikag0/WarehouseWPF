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
using WMS.Application.WarehouseVisualization;
using WMS.Domain;

namespace WMS.Desktop.ViewModels
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

        public CellViewModel(CellDto dto)
        {
            Id = dto.Id;

            Code = LocationFormatter.CodeToDisplay(dto.Column, dto.Row);

            X = dto.X;
            Y = dto.Y;
            Width = dto.Width;
            Height = dto.Height;
        }

    }
}
