using System.Linq;
using System.Windows;

namespace WMS.Desktop.Views
{
    public static class WindowOwner
    {
        // Окно, поверх которого нужно открыть новый диалог: то, с которым сейчас работает пользователь
        // (например, «Настройки»), иначе главное окно. Диалог с владельцем всегда остаётся над ним
        // и не прячется за ним после Alt+Tab — иначе программа выглядит зависшей.
        public static Window? Current =>
            System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            ?? System.Windows.Application.Current.MainWindow;
    }
}
