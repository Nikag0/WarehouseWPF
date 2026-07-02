using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class NotificationService
    {
        // Список всех уведомлений в памяти
        public ObservableCollection<NotificationItem> Items { get; set; } = new();

        // Метод, который вы вызовете при старте приложения
        public void CheckWarehouseStatus()
        {
            Items.Clear();

            // 1. Запрос в БД: получить товары, где Остаток < Минимального
            // foreach (var item in db.Products.Where(p => p.Count < p.MinCount))
            // {
            //     Items.Add(new NotificationItem { Title = "Критический остаток", Type = NotificationType.LowStock ... });
            // }

            // 2. Запрос в БД: получить планируемые задачи на сегодня

            // 3. Запрос в БД: проверить сроки годности
        }

        // Метод для добавления уведомления на лету (например, во время работы)
        public void AddNotification(string title, string desc, NotificationType type)
        {
            Items.Add(new NotificationItem { Title = title, Description = desc, Type = type });
        }
    }

}
