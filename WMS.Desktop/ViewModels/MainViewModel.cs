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
using WMS.Desktop.ViewModels.MenuViewModels;
using WMS.Desktop.ViewModels;
using WMS.Desktop.Views.MenuView;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;
using WMS.Desktop.Views;

namespace WMS.Desktop.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty] private object _currentView;

        public NotificationViewModel Notification { get; }
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
        public ICommand ShowWarehouseCommand { get; }

        public MainViewModel(
                    NotificationViewModel notification,
                    ComponentsViewModel components,
                    IssueViewModel issue,
                    ReceiptViewModel receipt,
                    HistoryViewModel history,
                    Func<SettingsView> settingsWindowFactory,
                    Func<WarehouseView> warehouseFactory)
        {
            Notification = notification;
            Components = components;
            Issue = issue;
            Receipt = receipt;
            History = history;

            _currentView = Components;
            Components.IsActiveTab = true;

            ShowNotificationCommand = new RelayCommand(_ => ChangeView(Notification));
            ShowComponentsCommand = new RelayCommand(_ => ChangeView(Components));
            ShowIssueCommand = new RelayCommand(_ => ChangeView(Issue));
            ShowReceiptCommand = new RelayCommand(_ => ChangeView(Receipt));
            ShowHistoryCommand = new RelayCommand(_ => ChangeView(History));

            ShowSettingsCommand = new RelayCommand(_ =>
            {
                var window = settingsWindowFactory();
                window.ShowDialog();
            });

            ShowWarehouseCommand = new RelayCommand(_ =>
            {
                var window = warehouseFactory();
                window.ShowDialog();
            });
        }

        private void ChangeView(object newView)
        {
            // 1. Сначала жестко гасим флаги активности у ВСЕХ вкладок
            Notification.IsActiveTab = false;
            Components.IsActiveTab = false;
            Issue.IsActiveTab = false;
            Receipt.IsActiveTab = false;
            History.IsActiveTab = false;

            // 2. Включаем флаг только у той ViewModel, которую открываем
            if (newView is NotificationViewModel n) n.IsActiveTab = true;
            else if (newView is ComponentsViewModel c) c.IsActiveTab = true;
            else if (newView is IssueViewModel i) i.IsActiveTab = true;
            else if (newView is ReceiptViewModel r) r.IsActiveTab = true;
            else if (newView is HistoryViewModel h) h.IsActiveTab = true;

            // 3. Обновляем текущий View на экране
            CurrentView = newView;
        }
    }
}
