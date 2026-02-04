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
        public IssueViewModel Issue { get; }
        public ReceiptViewModel Receipt { get; }

        public MainViewModel(ComponentsViewModel components, IssueViewModel issues, ReceiptViewModel receipt)
        {
            Components = components;
            Issue = issues;
            Receipt = receipt;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
