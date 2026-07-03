using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Services;
using WMS.Domain;

namespace WMS.Desktop.ViewModels.MenuViewModels
{
    public class NotificationViewModel
    {
        private NotificationService _notificationService;
        public ObservableCollection<NotificationItem> NotificationItems { get; set; } = new();

        public NotificationViewModel( NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task LoadNotification ()
        {
            NotificationItems.Clear();
            var notifications = await _notificationService.GetStockWithMinQuantity();

            foreach (var notification in notifications)
            {
                NotificationItems.Add(notification);
            }
        }
    }
}
