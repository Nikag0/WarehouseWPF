using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using WMS.Application.Services;
using WMS.Desktop.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;


namespace WMS.Desktop.ViewModels.SettingViewModel
{
    public partial class DeleteConfirmationViewModel : ObservableObject
    {
        // Принимает флаг «сохранить в CSV». Возвращает false, если пользователь передумал (например, закрыл окно выбора файла).
        private readonly Func<bool, Task<bool>> _onConfirmAsync;

        [ObservableProperty]
        private string _expectedConfirmationText;

        [ObservableProperty]
        private bool _isExportBeforeDelete;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        private string _userInput;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        private bool _isBusy;

        public DeleteConfirmationViewModel(DateTime from, DateTime to, Func<bool, Task<bool>> onConfirmAsync)
        {
            _expectedConfirmationText = $"{from:dd.MM.yyyy}-{to:dd.MM.yyyy}";
            _onConfirmAsync = onConfirmAsync;
        }

        [RelayCommand(CanExecute = nameof(CanConfirm))]
        private async Task Confirm(Window window)
        {
            try
            {
                IsBusy = true;

                if (_onConfirmAsync != null && !await _onConfirmAsync.Invoke(IsExportBeforeDelete))
                    return;

                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanConfirm()
        {
            return !IsBusy && string.Equals(UserInput?.Trim(), ExpectedConfirmationText, StringComparison.OrdinalIgnoreCase);
        }
    }
}
