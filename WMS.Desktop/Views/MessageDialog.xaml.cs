using System.Media;
using System.Windows;
using System.Windows.Media;

namespace WMS.Desktop.Views
{
    public enum MessageDialogKind
    {
        Info,
        Warning,
        Error,
        Question
    }

    // Окно сообщения с крупным шрифтом. Для вопроса — кнопки «Да» / «Нет», для остальных — «ОК».
    // Enter нажимает «Да» / «ОК», Esc — «Нет» / закрывает окно.
    public partial class MessageDialog : Window
    {
        private MessageDialog(string message, string title, MessageDialogKind kind)
        {
            InitializeComponent();

            Title = title;
            MessageText.Text = message;

            var isQuestion = kind == MessageDialogKind.Question;
            YesButton.Visibility = isQuestion ? Visibility.Visible : Visibility.Collapsed;
            NoButton.Visibility = isQuestion ? Visibility.Visible : Visibility.Collapsed;
            OkButton.Visibility = isQuestion ? Visibility.Collapsed : Visibility.Visible;

            (IconText.Text, IconText.Foreground) = kind switch
            {
                MessageDialogKind.Info => ("", new SolidColorBrush(Color.FromRgb(0x7F, 0xC8, 0xF8))),
                MessageDialogKind.Warning => ("", new SolidColorBrush(Color.FromRgb(0xF5, 0xC5, 0x42))),
                MessageDialogKind.Error => ("", new SolidColorBrush(Color.FromRgb(0xF2, 0x6D, 0x6D))),
                _ => ("", new SolidColorBrush(Color.FromRgb(0x7F, 0xC8, 0xF8)))
            };
        }

        // Возвращает true, если нажата «Да» / «ОК»
        public static bool Show(string message, string title, MessageDialogKind kind)
        {
            var dialog = new MessageDialog(message, title, kind);

            // Пока главное окно не показано (ошибки при запуске), владельца нет — окно по центру экрана
            var owner = WindowOwner.Current;
            if (owner != null && owner.IsVisible)
                dialog.Owner = owner;
            else
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Звук как у стандартного MessageBox
            (kind switch
            {
                MessageDialogKind.Warning => SystemSounds.Exclamation,
                MessageDialogKind.Error => SystemSounds.Hand,
                MessageDialogKind.Question => SystemSounds.Question,
                _ => SystemSounds.Asterisk
            }).Play();

            return dialog.ShowDialog() == true;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
