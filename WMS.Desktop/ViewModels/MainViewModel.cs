using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WMS.Application.Services;

namespace WMS.Desktop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ComponentsViewModel Components { get; }
        public StockViewModel Stocks { get; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(ComponentsViewModel components, StockViewModel stocks)
        {
            Components = components;
            Stocks = stocks;
        }
    }
}
