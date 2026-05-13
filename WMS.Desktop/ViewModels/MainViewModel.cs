using MvvmHelpers;
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
    public class MainViewModel : BaseViewModel
    {
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ComponentsViewModel Components { get; }
        public IssueViewModel Issue { get; }
        public ReceiptViewModel Receipt { get; }

        public ICommand ShowComponentsCommand { get; }
        public ICommand ShowIssueCommand { get; }
        public ICommand ShowReceiptCommand { get; }

        public MainViewModel(
            ComponentsViewModel components,
            IssueViewModel issue,
            ReceiptViewModel receipt)
        {
            Components = components;
            Issue = issue;
            Receipt = receipt;

            CurrentView = Components;

            ShowComponentsCommand =
                new RelayCommand(_ => CurrentView = Components);

            ShowIssueCommand =
                new RelayCommand(_ => CurrentView = Issue);

            ShowReceiptCommand =
                new RelayCommand(_ => CurrentView = Receipt);
        }
    }
}
