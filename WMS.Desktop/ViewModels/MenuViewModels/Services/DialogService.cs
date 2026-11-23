using Microsoft.Win32;
using WMS.Application.Abstractions;
using WMS.Desktop.Views;

namespace WMS.Desktop.Services
{
    // Сообщения пользователю показываются окном MessageDialog с крупным шрифтом вместо стандартного MessageBox
    public class DialogService : IDialogService
    {
        public void ShowInfo(string message, string title = "Информация")
            => Show(message, title, MessageDialogKind.Info);

        public void ShowWarning(string message, string title = "Предупреждение")
            => Show(message, title, MessageDialogKind.Warning);

        public void ShowError(string message, string title = "Ошибка")
            => Show(message, title, MessageDialogKind.Error);

        public bool ShowConfirmation(string message, string title, string confirmText, string cancelText = "Отмена")
            => Show(message, title, MessageDialogKind.Question, confirmText, cancelText);

        // Можно вызывать из фонового потока (например, из Task.Run в поиске): окно всё равно покажется в потоке интерфейса
        private static bool Show(string message, string title, MessageDialogKind kind, string? confirmText = null, string cancelText = "Отмена")
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher is null)
                return false; // Приложение уже закрывается

            if (!dispatcher.CheckAccess())
                return dispatcher.Invoke(() => Show(message, title, kind, confirmText, cancelText));

            return MessageDialog.Show(message, title, kind, confirmText, cancelText);
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
