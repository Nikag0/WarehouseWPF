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
        private readonly Func<Task> _onSuccessAsync;

        [ObservableProperty]
        private string _expectedConfirmationText;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        private string _userInput;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        private bool _isBusy;

        public DeleteConfirmationViewModel(DateTime from, DateTime to, Func<Task> onSuccessAsync)
        {
            _expectedConfirmationText = $"{from:dd.MM.yyyy}-{to:dd.MM.yyyy}";
            _onSuccessAsync = onSuccessAsync;
        }

        [RelayCommand(CanExecute = nameof(CanConfirm))]
        private async Task Confirm(Window window)
        {
            try
            {
                IsBusy = true;

                if (_onSuccessAsync != null)
                {
                    await _onSuccessAsync.Invoke();
                }

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
