using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WMS.Application.Services;
using WMS.Desktop.Views;
using WMS.Domain;

namespace WMS.Desktop.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ComponentService _componentService;
        private readonly OperatorService _operatorService;
        private readonly DialogService _dialogService;

        [ObservableProperty] private ObservableCollection<ComponentDTO> _filteredComponents = new();
        [ObservableProperty] private ObservableCollection<Operator> _filteredOperators = new();

        public SettingsViewModel(
            ComponentService componentService,
            OperatorService operatorService,
            DialogService dialogService)
        {
            _componentService = componentService;
            _operatorService = operatorService;
            _dialogService = dialogService;

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var componentsResult = await _componentService.GetAllAsync();
                var operatorsList = await _operatorService.GetAllAsync();

                if (componentsResult.IsSuccess)
                {
                    FilteredComponents = new ObservableCollection<ComponentDTO>(componentsResult.Value);
                }
                else
                {
                    _dialogService.ShowWarning(componentsResult.Error, "Ошибка загрузки каталога");
                }

                FilteredOperators = new ObservableCollection<Operator>(operatorsList);
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrror($"Критический сбой при загрузке данных: {ex.Message}");
            }
        }

        [RelayCommand]
        private void AddComponent()
        {
            var vm = new ComponentEditViewModel(_componentService, _dialogService);
            var window = new ComponentEditWindow(vm) { Owner = App.Current.MainWindow };

            if (window.ShowDialog() == true)
            {
                _ = LoadDataAsync();
            }
        }

        [RelayCommand]
        private void EditComponent(ComponentDTO component)
        {
            if (component == null) return;

            var vm = new ComponentEditViewModel(_componentService, _dialogService, component);
            var window = new ComponentEditWindow(vm) { Owner = App.Current.MainWindow };

            if (window.ShowDialog() == true)
            {
                _ = LoadDataAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteComponent(ComponentDTO component)
        {
            if (component == null) return;

            if (_dialogService.ShowConfirmation($"Удалить компонент {component.Name}?", "Удаление"))
            {
                var result = await _componentService.DeleteAsync(component.Id);

                if (result.IsSuccess)
                {
                    FilteredComponents.Remove(component);
                    _dialogService.ShowInfo("Компонент успешно удален.");
                }
                else
                {
                    _dialogService.ShowWarning(result.Error, "Предупреждение");
                }
            }
        }

        [RelayCommand]
        private void AddOperator()
        {
            var vm = new OperatorEditViewModel(_operatorService);
            var window = new OperatorEditWindow(vm) { Owner = App.Current.MainWindow };

            if (window.ShowDialog() == true)
            {
                _ = LoadDataAsync();
            }
        }

        [RelayCommand]
        private void EditOperator(Operator op)
        {
            if (op == null) return;

            var vm = new OperatorEditViewModel(_operatorService, op);
            var window = new OperatorEditWindow(vm) { Owner = App.Current.MainWindow };

            if (window.ShowDialog() == true)
            {
                _ = LoadDataAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteOperator(Operator op)
        {
            if (op == null) return;
            var result = MessageBox.Show($"Удалить {op.Surname} {op.Name}?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // await _operatorService.DeleteAsync(op.Id);
                FilteredOperators.Remove(op);
            }
        }
    }
}
