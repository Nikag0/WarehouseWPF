using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Services;
using WMS.Desktop.Services;
using WMS.Domain;

namespace WMS.Desktop.ViewModels.MenuViewModels
{
    public partial class NotificationViewModel : ObservableObject
    {
        public ObservableCollection<NotificationItem> NotificationItems { get; set; } = new();
        private NotificationService _notificationService;
        private readonly DialogService _dialogService;
        [ObservableProperty] private bool _isActiveTab;

        public NotificationViewModel(
            NotificationService notificationService,
            DialogService dialogService)
        {
            _notificationService = notificationService;
            _dialogService = dialogService;

            WeakReferenceMessenger.Default.Register<MessageService>(this, async (r, m) =>
            {
                if (IsActiveTab)
                {
                    await LoadNotification();
                }
            });
        }

        public async Task LoadNotification()
        {
            try
            {
                NotificationItems.Clear();
                var notifications = await _notificationService.GetStockWithMinQuantity();

                foreach (var notification in notifications)
                {
                    NotificationItems.Add(notification);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Критический сбой при загрузке данных: {ex.Message}");
            }
        }
    }
}
