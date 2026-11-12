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

namespace WMS.Desktop.ViewModels
{
    public partial class OperatorEditViewModel : ObservableObject
    {
        private readonly OperatorService _operatorService;
        private readonly DialogService _dialogService;
        private readonly Operator _currentOperator;
        private readonly bool _isEditMode;

        [ObservableProperty] private string _windowTitle;
        [ObservableProperty] private string _surname;
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _patronymic;

        public OperatorEditViewModel(
            OperatorService operatorService,
            DialogService dialogService, 
            Operator op = null)
        {
            _operatorService = operatorService;
            _currentOperator = op;
            _dialogService = dialogService;
            _isEditMode = op != null;

            WindowTitle = _isEditMode ? "Редактирование оператора" : "Добавление оператора";

            if (_isEditMode)
            {
                Surname = op.Surname;
                Name = op.Name;
                Patronymic = op.Patronymic;
            }
        }

        [RelayCommand]
        private async Task Save(Window window)
        {
            try
            {
                if (_isEditMode)
                {
                    await _operatorService.UpdateAsync(
                        new OperatorDTO(
                            _currentOperator.Id,
                            Surname,
                            Name,
                            Patronymic,
                            string.Empty // FullName вычисляет сущность Operator, сервису оно не нужно
                        ));
                }
                else
                {
                    await _operatorService.AddAsync(
                        Surname,
                        Name,
                        Patronymic
                    );
                }

                _dialogService.ShowInfo("Данные успешно сохранены.");
                if (window != null)
                {
                    WeakReferenceMessenger.Default.Send(new MessageService(true));
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message, "Предупреждение");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Критическая ошибка сохранения: {ex.Message}", "Ошибка системы");
            }
        }
    }
}
