using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;

namespace WMS.Desktop.ViewModels
{
    public partial class OperatorEditViewModel : ObservableObject
    {
        private readonly OperatorService _operatorService;
        private readonly Operator _currentOperator;
        private readonly bool _isEditMode;

        [ObservableProperty] private string _windowTitle;
        [ObservableProperty] private string _surname;
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _patronymic;

        public OperatorEditViewModel(OperatorService operatorService, Operator op = null)
        {
            _operatorService = operatorService;
            _currentOperator = op;
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
                            $"{Surname} {Name[0]}. {Patronymic[0]}."
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

                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (BusinessException domainEx)
            {
                MessageBox.Show(domainEx.Message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Системная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
