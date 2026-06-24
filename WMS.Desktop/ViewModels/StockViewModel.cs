using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Desktop.ViewModels
{
    public class StockViewModel : INotifyPropertyChanged
    {
        public Guid Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }
        private Guid _id;

        public Guid ComponentId
        {
            get => _componentId;
            set 
            {
                _componentId = value;
                OnPropertyChanged();
            }
        }
        private Guid _componentId;
        public string Article
        {
            get => _article;
            set 
            {
                _article = value;
                OnPropertyChanged();
            }
        }
        private string? _article;
        public string ComponentName
        {
            get => _componentName;
            set 
            {
                _componentName = value;
                OnPropertyChanged();
            }
        }
        private string _componentName;
        public string Manufacturer
        {
            get => _manufacturer;
            set 
            {
                _manufacturer = value;
                OnPropertyChanged();
            }
        }
        private string? _manufacturer;

        public Guid RackId
        {
            get => _rackId;
            set 
            {
                _rackId = value;
                OnPropertyChanged();
            }
        }
        private Guid _rackId;
        public string RackCode
        {
            get => _rackCode;
            set 
            {
                _rackCode = value;
                OnPropertyChanged();
            }
        }
        private string? _rackCode;

        public Guid CellId
        {
            get => _cellId;
            set 
            {
                _cellId = value;
                OnPropertyChanged();
            }
        }
        private Guid _cellId;
        public string CellCode
        {
            get => _cellCode;
            set 
            {
                _cellCode = value;
                OnPropertyChanged();
            }
        }
        private string? _cellCode;

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }
        private int _quantity;
        public int OperationQuantity
        {
            get => _operationQuantity;
            set
            {
                _operationQuantity = value;
                OnPropertyChanged();
            }
        }
        private int _operationQuantity;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
