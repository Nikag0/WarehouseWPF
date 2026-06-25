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
using WMS.Application.WarehouseVisualization;
using WMS.Domain;

namespace WMS.Desktop.ViewModels
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

        public RackViewModel(RackDTO dto)
        {
            Id = dto.Id;
            Code = LocationFormatter.CodeToDisplay(dto.Column, dto.Row);
            Type = dto.Type;

            Cells = new ObservableCollection<CellViewModel>(
                dto.Cells.Select(c => new CellViewModel(c)));

            X = dto.X;
            Y = dto.Y;
            Width = dto.Width;
            Height = dto.Height;
        }
    }
}
