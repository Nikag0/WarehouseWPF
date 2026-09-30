using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WMS.Application.Services;
using WMS.Desktop.Services;
using WMS.Desktop.Views;
using WMS.Domain;

namespace WMS.Desktop.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<ComponentDTO> _filteredComponents = new();
        [ObservableProperty] private ObservableCollection<Operator> _filteredOperators = new();
        [ObservableProperty] private string _searchComponent = string.Empty;

        private CancellationTokenSource? _cts;


        private readonly ComponentService _componentService;
        private readonly OperatorService _operatorService;
        private readonly DialogService _dialogService;
        public SettingsViewModel(
            ComponentService componentService,
            OperatorService operatorService,
            DialogService dialogService)
        {
            _componentService = componentService;
            _operatorService = operatorService;
            _dialogService = dialogService;
        }

        public async Task LoadDataAsync()
        {
            try
            {
                OnSearchComponentChanged(_searchComponent);
                var operatorsList = await _operatorService.GetAllAsync();

                FilteredOperators = new ObservableCollection<Operator>(operatorsList);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Критический сбой при загрузке данных: {ex.Message}");
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

            if (_dialogService.ShowConfirmation($"Удалить оператора {op.Surname} {op.Name}?", "Удаление"))
            {
                var result = await _operatorService.DeleteAsync(op.Id);

                if (result.IsSuccess)
                {
                    FilteredOperators.Remove(op);
                    _dialogService.ShowInfo("Оператор успешно удален.");
                }
                else
                {
                    _dialogService.ShowWarning(result.Error, "Предупреждение");
                }
            }
        }

        partial void OnSearchComponentChanged(string value)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500, _cts.Token);

                    var result = await _componentService.GetFilteredAsync(searchText: value, maxCount: 100, _cts.Token);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (!_cts.Token.IsCancellationRequested)
                        {
                            FilteredComponents.Clear();
                            foreach (var component in result)
                            {
                                FilteredComponents.Add(component);
                            }
                        }
                    });
                }
                catch (OperationCanceledException) { }
            });
        }
    }
}
