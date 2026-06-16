using CommunityToolkit.Mvvm.ComponentModel;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WMS.Application.Services;
using WMS.Desktop.Views;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;

namespace WMS.Desktop.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty] private object _currentView;

        public NotificationViewModel Notifications { get; }
        public ComponentsViewModel Components { get; }
        public IssueViewModel Issue { get; }
        public ReceiptViewModel Receipt { get; }
        public HistoryViewModel History { get; }

        public ICommand ShowNotificationCommand { get; }
        public ICommand ShowComponentsCommand { get; }
        public ICommand ShowIssueCommand { get; }
        public ICommand ShowReceiptCommand { get; }
        public ICommand ShowHistoryCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        public MainViewModel(
                    NotificationViewModel notifications,
                    ComponentsViewModel components,
                    IssueViewModel issue,
                    ReceiptViewModel receipt,
                    HistoryViewModel history,
                    Func<SettingsView> settingsWindowFactory)
        {
            Notifications = notifications;
            Components = components;
            Issue = issue;
            Receipt = receipt;
            History = history;

            CurrentView = Components;

            ShowNotificationCommand =
                new RelayCommand(_ => CurrentView = Notifications);

            ShowComponentsCommand =
                new RelayCommand(_ => CurrentView = Components);

            ShowIssueCommand =
                new RelayCommand(_ => CurrentView = Issue);

            ShowReceiptCommand =
                new RelayCommand(_ => CurrentView = Receipt);

            ShowHistoryCommand =
                new RelayCommand(_ => CurrentView = History);

            ShowSettingsCommand = new RelayCommand(_ =>
            {
                var window = settingsWindowFactory();

                window.ShowDialog();
            });
        }
    }
}
