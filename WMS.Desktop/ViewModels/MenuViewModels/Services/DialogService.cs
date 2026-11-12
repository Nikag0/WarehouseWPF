using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using WMS.Application.Abstractions;

namespace WMS.Desktop.Services
{
    public class DialogService : IDialogService
    {
        public void ShowInfo(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarning(string message, string title = "Предупреждение")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // Можно вызывать из фонового потока (например, из Task.Run в поиске): окно всё равно покажется в потоке интерфейса
        public void ShowError(string message, string title = "Ошибка")
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher is null)
                return; // Приложение уже закрывается

            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() => ShowError(message, title));
                return;
            }

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowConfirmation(string message, string title = "Подтверждение")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        public string? ShowSaveCsvDialog(string title, string defaultFileName)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = title,
                Filter = "CSV файлы (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = defaultFileName
            };

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }
    }
}
