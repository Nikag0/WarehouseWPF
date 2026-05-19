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

        [ObservableProperty] private ObservableCollection<ComponentDTO> _filteredComponents = new();
        [ObservableProperty] private ObservableCollection<Operator> _filteredOperators = new();

        public SettingsViewModel(ComponentService componentService, OperatorService operatorService)
        {
            _componentService = componentService;
            _operatorService = operatorService;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var componentsList = await _componentService.GetAllAsync();
            var operatorsList = await _operatorService.GetAllAsync();

            FilteredComponents = new ObservableCollection<ComponentDTO>(componentsList);
            FilteredOperators = new ObservableCollection<Operator>(operatorsList);
        }

        // ================= ДЕЙСТВИЯ КАТАЛОГА =================

        [RelayCommand]
        private void AddComponent()
        {
            var vm = new ComponentEditViewModel(_componentService);
            var window = new ComponentEditWindow(vm) { Owner = App.Current.MainWindow };

            if (window.ShowDialog() == true)
            {
                _ = LoadDataAsync(); // Перечитываем базу, если добавили элемент
            }
        }

        [RelayCommand]
        private void EditComponent(ComponentDTO component)
        {
            if (component == null) return;

            var vm = new ComponentEditViewModel(_componentService, component);
            var window = new ComponentEditWindow(vm) { Owner = App.Current.MainWindow };

            if (window.ShowDialog() == true)
            {
                _ = LoadDataAsync(); // Перечитываем базу, если сохранили изменения
            }
        }

        [RelayCommand]
        private async Task DeleteComponent(ComponentDTO component)
        {
            if (component == null) return;
            var result = MessageBox.Show($"Удалить {component.Name}?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // await _componentService.DeleteAsync(component.Id);
                FilteredComponents.Remove(component);
            }
        }

        // ================= ДЕЙСТВИЯ ОПЕРАТОРОВ =================

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
