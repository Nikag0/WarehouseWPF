using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Desktop.Services;
using WMS.Desktop.Views;
using WMS.Domain;

namespace WMS.Desktop.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<ComponentEditDto> _filteredComponents = new();
        [ObservableProperty] private ObservableCollection<Operator> _filteredOperators = new();
        [ObservableProperty] private string _searchComponent = string.Empty;

        private CancellationTokenSource? _cts;


        private readonly ComponentService _componentService;
        private readonly OperatorService _operatorService;
        private readonly DialogService _dialogService;
        private readonly IStockService _stockService;


        public SettingsViewModel(
            ComponentService componentService,
            OperatorService operatorService,
            DialogService dialogService,
            IStockService stockService)
        {
            _componentService = componentService;
            _operatorService = operatorService;
            _dialogService = dialogService;
            _stockService = stockService;
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
        private void EditComponent(ComponentEditDto component)
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
        private async Task DeleteComponent(ComponentEditDto component)
        {
            if (component == null) return;

            if (_dialogService.ShowConfirmation($"Удалить компонент {component.Name}?", "Удаление"))
            {
                var result = await _componentService.DeleteAsync(component.Id);

                if (result.IsSuccess)
                {
                    FilteredComponents.Remove(component);
                    WeakReferenceMessenger.Default.Send(new MessageService(true));
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
            var vm = new OperatorEditViewModel(_operatorService, _dialogService);
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

            var vm = new OperatorEditViewModel(_operatorService, _dialogService, op);
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
                    WeakReferenceMessenger.Default.Send(new MessageService(true));
                    _dialogService.ShowInfo("Оператор успешно удален.");
                }
                else
                {
                    _dialogService.ShowWarning(result.Error, "Предупреждение");
                }
            }
        }

        [RelayCommand]
        private void ExportCsv()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Экспорт остатков в CSV",
                Filter = "CSV файлы (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"Остатки_на_{DateTime.Now:yyyy-MM-dd}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = saveFileDialog.FileName;

                Task.Run(async () =>
                {
                    try
                    {
                        await _stockService.ExportCsvAsync(selectedFilePath);

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _dialogService.ShowInfo($"Файл {System.IO.Path.GetFileName(selectedFilePath)} сохранён \n" +
                                $"в папку {selectedFilePath}");
                        });

                    }
                    catch (System.IO.IOException)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _dialogService.ShowWarning($"Вы попытались сохранить файл с именем {System.IO.Path.GetFileName(selectedFilePath)}.\n" +
                                $"Файл с таким именем уже существует и открыт в другой программе. \n" +
                                $"Для сохранения переименуйте файл или закройте программу.", "Предупреждение");
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _dialogService.ShowWarning($"Ошибка экспорта {ex.Message}", "Предупреждение");
                        });
                    }
                });
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

                    var result = await _componentService.GetEditFilterAsync(searchText: value, maxCount: 100, _cts.Token);

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
