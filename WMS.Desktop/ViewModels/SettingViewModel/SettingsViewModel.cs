using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
using WMS.Desktop.ViewModels.SettingViewModel;
using WMS.Desktop.Views;
using WMS.Desktop.Views.Settings;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WMS.Desktop.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Operator> _filteredOperators = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteHistoryCommand))]
        [NotifyPropertyChangedFor(nameof(DateFromInvalid), nameof(DateToInvalid), nameof(DateRangeError))]
        private DateTime? _dateFrom = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteHistoryCommand))]
        [NotifyPropertyChangedFor(nameof(DateFromInvalid), nameof(DateToInvalid), nameof(DateRangeError))]
        private DateTime? _dateTo = DateTime.Today;

        private bool DatesOutOfOrder => DateFrom.HasValue && DateTo.HasValue && DateFrom.Value.Date > DateTo.Value.Date;
        public bool DateFromInvalid => !DateFrom.HasValue || DateFrom.Value.Date > DateTime.Today || DatesOutOfOrder;
        public bool DateToInvalid => !DateTo.HasValue || DateTo.Value.Date > DateTime.Today || DatesOutOfOrder;

        public string DateRangeError
        {
            get
            {
                if (!DateFrom.HasValue || !DateTo.HasValue)
                    return "Укажите обе даты.";
                if (DateFrom.Value.Date > DateTime.Today || DateTo.Value.Date > DateTime.Today)
                    return "Даты периода не могут быть в будущем.";
                return DatesOutOfOrder ? "Дата «С» должна быть не позже даты «По»." : string.Empty;
            }
        }



        private readonly OperatorService _operatorService;
        private readonly DialogService _dialogService;
        private readonly HistoryService _historyService;
        private readonly IStockService _stockService;
        private readonly LedStripService _ledStripService;


        public SettingsViewModel(
            OperatorService operatorService,
            DialogService dialogService,
            HistoryService historyService,
            IStockService stockService,
            LedStripService ledStripService)
        {
            _operatorService = operatorService;
            _dialogService = dialogService;
            _historyService = historyService;
            _stockService = stockService;
            _ledStripService = ledStripService;
        }

        // Повторно отправляет контроллерам зоны подсветки — как при запуске программы.
        // Нужно, если контроллер был выключен при старте или перезагрузился во время работы.
        // Пока команда выполняется, кнопка неактивна, поэтому повторное нажатие не запустит её второй раз.
        [RelayCommand]
        private async Task ReinitializeControllers()
        {
            try
            {
                var report = await _ledStripService.InitializeAsync();

                // Об успехе сообщают лампы рядов (все загораются зелёным), окно показываем только при проблемах
                if (!report.IsConfigured)
                {
                    _dialogService.ShowWarning(
                        "В базе данных нет настроек подсветки (контроллеров, лент и зон ячеек).\n" +
                        "Перезагружать нечего: подсветка ячеек работать не будет.");
                }
                else if (!report.IsSuccess)
                {
                    _dialogService.ShowWarning(LightingReportFormatter.Describe(report));
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Не удалось загрузить настройки подсветки из базы данных: {ex.Message}");
            }
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var operatorsList = await _operatorService.GetAllAsync();

                FilteredOperators = new ObservableCollection<Operator>(operatorsList);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Критический сбой при загрузке данных: {ex.Message}");
            }
        }

        [RelayCommand]
        private void AddOperator()
        {
            var vm = new OperatorEditViewModel(_operatorService, _dialogService);
            var window = new OperatorEditWindow(vm) { Owner = WindowOwner.Current };

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
            var window = new OperatorEditWindow(vm) { Owner = WindowOwner.Current };

            if (window.ShowDialog() == true)
            {
                _ = LoadDataAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteOperator(Operator op)
        {
            if (op == null) return;

            if (!_dialogService.ShowConfirmation($"Удалить оператора {op.Surname} {op.Name}?", "Удаление", "Удалить оператора"))
                return;

            try
            {
                await _operatorService.DeleteAsync(op.Id);

                FilteredOperators.Remove(op);
                WeakReferenceMessenger.Default.Send(new MessageService(true));
                _dialogService.ShowInfo("Оператор успешно удален.");
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message, "Предупреждение");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Оператор {op.Surname} {op.Name} не удалён: не удалось сохранить изменения в базе.\n\n" +
                    $"Подробности: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ExportStockCsv()
        {
            var filePath = await ExportToCsvAsync(
                "Экспорт остатков в CSV",
                $"Остатки_на_{DateTime.Now:yyyy-MM-dd}",
                path => _stockService.ExportCsvAsync(path));

            if (filePath is not null)
                _dialogService.ShowInfo($"Остатки сохранены в файл {filePath}");
        }

        // Кнопка активна только при корректном периоде, поэтому DateFrom.Value и DateTo.Value ниже не пустые
        [RelayCommand(CanExecute = nameof(CanDeleteHistory))]
        private async Task DeleteHistory()
        {
            try
            {
                // Запоминаем период: даты на экране могут измениться, пока открыто окно подтверждения
                var dateFrom = DateFrom.Value;
                var dateTo = DateTo.Value;
                string? exportedFilePath = null;

                var confirmVm = new DeleteConfirmationViewModel(
                dateFrom,
                dateTo,
                onConfirmAsync: async isExportBeforeDelete =>
                {
                    if (isExportBeforeDelete)
                    {
                        exportedFilePath = await ExportToCsvAsync(
                            "Сохранение истории перед удалением",
                            $"История_{dateFrom:yyyy-MM-dd}_{dateTo:yyyy-MM-dd}",
                            path => _historyService.ExportCsvAsync(path, dateFrom, dateTo));

                        // Файл не сохранён — историю не удаляем, окно подтверждения остаётся открытым
                        if (exportedFilePath is null)
                            return false;
                    }

                    await _historyService.DeleteHistoryAsync(dateFrom, dateTo);
                    return true;
                });

                var dialog = new DeleteConfirmationWindow { DataContext = confirmVm, Owner = WindowOwner.Current };

                if (dialog.ShowDialog() == true)
                {
                    _dialogService.ShowInfo(exportedFilePath is null
                        ? "История успешно очищена."
                        : $"История сохранена в файл {exportedFilePath} и очищена.");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Не удалось очистить историю: {ex.Message}");
            }
        }

        private bool CanDeleteHistory()
        {
            return !DateFromInvalid && !DateToInvalid;
        }

        // Общий сценарий выгрузки таблицы в CSV.
        private async Task<string?> ExportToCsvAsync(string dialogTitle, string defaultFileName, Func<string, Task> writeAsync)
        {
            var filePath = _dialogService.ShowSaveCsvDialog(dialogTitle, defaultFileName);
            if (filePath is null)
                return null;

            var fileName = System.IO.Path.GetFileName(filePath);

            try
            {
                await writeAsync(filePath);
                return filePath;
            }
            // Занятый файл Windows сообщает как IOException или UnauthorizedAccessException (при замене файла)
            catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
            {
                _dialogService.ShowWarning(
                    $"Не удалось записать файл {fileName}: возможно, он открыт в другой программе (например, в Excel) " +
                    "или нет доступа к папке.\n" +
                    "Закройте файл или выберите другое имя или папку.", "Предупреждение");
                return null;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Не удалось сохранить файл {fileName}.\n\nПодробности: {ex.GetBaseException().Message}");
                return null;
            }
        }

    }
}
