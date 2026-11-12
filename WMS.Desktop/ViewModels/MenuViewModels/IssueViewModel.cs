using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Text;
using WMS.Application.Abstractions;
using WMS.Application.DTO;
using WMS.Application.Services;
using WMS.Desktop.Models;
using WMS.Desktop.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels.MenuViewModels
{
    public partial class IssueViewModel : ObservableObject
    {
        private readonly IStockService _stockService;

        public ObservableCollection<StockViewDTO> FilteredStocks { get; } = new();
        public ObservableCollection<RackViewModel> RacksGrid { get; } = new();
        public ObservableCollection<CellViewModel> CellsGrid { get; } = new();
        public ObservableCollection<Operator> Operators { get; } = new();
        public ObservableCollection<StockViewDTO> IssueItems { get; } = new();
        public RackViewModel SelectedRack 
        {
            get => _selectedRack;
            set
            {
                _selectedRack = value;
                OnPropertyChanged();

                foreach (var rack in RacksGrid)
                    rack.IsSelected = rack == value;

                LoadCells();
                UpdateCellHighlights();
            }
        }
        public CellViewModel SelectedCell
        {
            get => _selectedCell;
            set
            {
                _selectedCell = value;
                OnPropertyChanged();

                if (SelectedRack != null)
                {
                    foreach (var cell in CellsGrid)
                        cell.IsSelected = cell == value;
                }
            }
        }

        private readonly DialogService _dialogService;
        private readonly OperatorService _operatorService;
        private readonly WarehouseService _warehouseService;
        private readonly WarehouseVisualizationService _warehouseVisualizationService;
        private readonly LedStripService _ledStripService;

        private CancellationTokenSource? token;
        
        [ObservableProperty] private string _commentText = string.Empty;
        [ObservableProperty] private bool _isIssue;
        [ObservableProperty] private bool _isIssuePopupOpen;
        [ObservableProperty] private Operator? _operatorName;
        [ObservableProperty] private string _searchStock = string.Empty;
        [ObservableProperty] private bool _isActiveTab;

        private RackViewModel _selectedRack;
        private CellViewModel _selectedCell;
        public IssueViewModel(
            IStockService stockService,
            DialogService dialogService,
            OperatorService operatorService,
            WarehouseService warehouseService,
            WarehouseVisualizationService warehouseVisualizationService,
            LedStripService ledStripService)
        {
            _stockService = stockService;
            _dialogService = dialogService;
            _operatorService = operatorService;
            _warehouseService = warehouseService;
            _warehouseVisualizationService = warehouseVisualizationService;
            _ledStripService = ledStripService;

            WeakReferenceMessenger.Default.Register<MessageService>(this, async (r, m) =>
            {
                if (IsActiveTab)
                {
                    await LoadDataAsync();
                }
            });
        }

        [RelayCommand]
        public void ClearIssueItems()
        {
            IssueItems.Clear();
            UpdateRackHighlights();
            UpdateCellHighlights();
            IsIssue = false;
        }

        [RelayCommand]
        public async Task IssueAsync()
        {
            // Защита от повторной выдачи.
            if (IsIssue)
            {
                _dialogService.ShowInfo("Эта выдача уже выполнена. Очистите список, чтобы начать новую.");
                return;
            }

            if (IssueItems.Count == 0)
            {
                _dialogService.ShowWarning("Список выдачи пуст.");
                return;
            }

            // Проверка нехвати остатков.
            var invalidItems = IssueItems
                              .Where(x => x.OperationQuantity <= 0)
                              .ToList();

            // Проверка на количесвто выдачи.
            if (invalidItems.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("Количество товаров для выдачи должно быть больше 0:");
                sb.AppendLine();

                foreach (var item in invalidItems)
                {
                    sb.AppendLine($"• {item.ComponentName}");
                }

                _dialogService.ShowWarning(sb.ToString());
                sb.Clear();
                return;
            }

            // Проверка кол-ва выдачи в бд
            var shortagedItems = IssueItems.Where(x => x.Quantity < x.OperationQuantity).ToList();
            if (shortagedItems.Any())
            {
                var firstShort = shortagedItems.First();
                _dialogService.ShowWarning($"{firstShort.ComponentName} не хватает. Осталось: {firstShort.Quantity}. В выдаче: {firstShort.OperationQuantity}");
                return;
            }

            if (OperatorName == null)
            {
                _dialogService.ShowWarning("Оператор не указан");
                return;
            }

            // Интерактивный обход стеллажей.
            // На схеме: рамка IsSelected, заливка ItemInCell.
            var itemsToReview = IssueItems.ToList();

            // Если контроллер подсветки не ответил один раз, дальше выдача идёт без подсветки,
            // чтобы не ждать таймаут на каждой ячейке.
            var isLedAvailable = true;

            foreach (var item in itemsToReview)
            {
                // Переносим рамку на стеллаж и ячейку текущего товара. Если их нет в схеме,
                // рамка снимается, чтобы не указывать на место предыдущего товара.
                SelectedRack = RacksGrid.FirstOrDefault(r => r.Id == item.RackId);
                SelectedCell = CellsGrid.FirstOrDefault(c => c.Id == item.CellId);

                var question = $"Товар найден?\n\n• {item.ComponentName} Стеллаж:{item.RackCodeDisplay} Ячейка:{item.CellCodeDisplay} Количество: {item.OperationQuantity}";

                // Включили подсветку на ячейке.
                if (isLedAvailable && !await SwitchCellLightAsync(item.CellId, isOn: true))
                {
                    isLedAvailable = false;
                    _dialogService.ShowWarning(
                        "Подсветка ячейки не сработала: контроллер не отвечает.\n\n" +
                        "Выдача продолжится без подсветки. Находите ячейки по номеру стеллажа и ячейки.");
                }

                if (!_dialogService.ShowConfirmation(question))
                {
                    // Товар не найден — убираем его из выдачи и снимаем заливку с его ячейки и стеллажа
                    IssueItems.Remove(item);
                    UpdateRackHighlights();
                    UpdateCellHighlights();
                }

                // Выключили подсветку на ячейке.
                if (isLedAvailable && !await SwitchCellLightAsync(item.CellId, isOn: false))
                {
                    isLedAvailable = false;
                    _dialogService.ShowWarning(
                        $"Не удалось выключить подсветку ячейки {item.CellCodeDisplay}: контроллер не отвечает.\n\n" +
                        "Выдача продолжится без подсветки.");
                }
            }

            if (!IssueItems.Any())
            {
                _dialogService.ShowInfo("Выдача отменена, так как ни один товар не был найден.");
                ClearIssueItems();
                return;
            }

            // Регистрируем выдачу.
            try
            {
                var issueOperation = IssueItems
                    .Select(item => new IssueStockDto(
                        item.StockId,
                        item.OperationQuantity,
                        item.RackId,
                        item.RackCodeDisplay,
                        item.CellId,
                        item.CellCodeDisplay,
                        item.ComponentId,
                        item.ComponentName))
                    .ToList();

                await _stockService.IssueAsync(issueOperation, OperatorName.FullName, CommentText);

                CommentText = string.Empty;
                SearchStock = string.Empty;
                IsIssue = true;
                _dialogService.ShowInfo("Выдача успешно выполнена.");
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                // Все изменения сохраняются одной транзакцией, поэтому при ошибке остатки не меняются
                _dialogService.ShowError(
                    "Выдача не выполнена: не удалось сохранить данные в базе.\n" +
                    "Остатки не изменились, список выдачи сохранён — можно повторить попытку.\n\n" +
                    $"Подробности: {ex.Message}");
            }
        }

        // Включает или выключает подсветку и GPIO ячейки. Возвращает false, если контроллер не ответил.
        private async Task<bool> SwitchCellLightAsync(Guid cellId, bool isOn)
        {
            try
            {
                var isLightOk = isOn
                    ? await _ledStripService.TurnOnSectorAsync(cellId)
                    : await _ledStripService.TurnOffSectorAsync(cellId);

                var isGpioOk = await _ledStripService.SetGPIOAsync(cellId, isOn ? 1 : 0);

                return isLightOk && isGpioOk;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task LoadDataAsync()
        {
            try
            {
                await Task.WhenAll(
                    LoadWarehouseAsync(),
                    LoadOperatorsAsync()
                );

                OnSearchStockChanged(SearchStock);
                UpdateRackHighlights();
                UpdateCellHighlights();
                await RefreshIssueItemsAsync();
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Не удалось загрузить данные: {ex.Message}");
            }
        }

        [RelayCommand]
        public void RemoveIssueItem(object obj)
        {
            if (obj is not StockViewDTO item)
                return;

            IssueItems.Remove(item);
            UpdateRackHighlights();
            UpdateCellHighlights();
        }

        [RelayCommand]
        public void SelectCell(CellViewModel cell)
        {
            if (cell == null)
                return;

            SelectedCell = cell;
        }

        [RelayCommand]
        public void SelectRack(RackViewModel rack)
        {
            if (rack == null)
                return;

            SelectedRack = rack;
        }

        [RelayCommand]
        private async Task AddIssueItem(object obj)
        {
            if (obj is not StockViewDTO item)
                return;

            if (IssueItems.Any(x => x.StockId == item.StockId))
                return;

            try
            {
                var stock = await _stockService.GetStockByIdAsync(item.StockId);
                if (stock is null)
                {
                    _dialogService.ShowWarning($"{item.ComponentName}: товара больше нет в этой ячейке.");
                    return;
                }

                IssueItems.Add(stock);
                UpdateRackHighlights();
                UpdateCellHighlights();
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Не удалось добавить товар в выдачу: {ex.Message}");
            }
        }

        private async Task LoadWarehouseAsync()
        {
            var racks = await _warehouseService.GetRacksAsync();
            var racksView = await _warehouseVisualizationService.GetWarehouseAsync(racks);

            RacksGrid.Clear();

            foreach (var rack in racksView)
            {
                RacksGrid.Add(rack);
            }
        }

        private void LoadCells()
        {
            CellsGrid.Clear();

            if (SelectedRack is null)
                return;

            foreach (var cell in SelectedRack.Cells)
            {
                CellsGrid.Add(cell);
            }
        }

        private async Task LoadOperatorsAsync()
        {
            var data = await _operatorService.GetAllAsync();
            Operators.Clear();
            foreach (var @operator in data)
                Operators.Add(@operator);
        }

        // Поиск по остаткам.
        partial void OnSearchStockChanged(string value)
        {
            this.token?.Cancel();

            if (string.IsNullOrWhiteSpace(value))
            {
                FilteredStocks.Clear();
                IsIssuePopupOpen = false;
                return;
            }

            this.token = new CancellationTokenSource();
            var token = this.token.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500, token);

                    var suggestions = await _stockService.GetFilteredStockAsync(value, maxCount: 15);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (!token.IsCancellationRequested)
                        {
                            FilteredStocks.Clear();
                            foreach (var item in suggestions)
                            {
                                FilteredStocks.Add(item);
                            }

                            IsIssuePopupOpen = FilteredStocks.Any();
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    // Задача отменена новым вводом текста — ничего не делаем
                }
                catch (Exception ex)
                {
                    // Без этого ошибка фоновой задачи потерялась бы, и пользователь просто не увидел бы результатов
                    _dialogService.ShowError($"Не удалось выполнить поиск: {ex.Message}");
                }
            });
        }

        private void UpdateCellHighlights()
        {
            var cellIds = IssueItems
                .Select(x => x.CellId)
                .ToHashSet();

            foreach (var cell in CellsGrid)
            {
                cell.ItemInCell = cellIds.Contains(cell.Id);
            }
        }

        private void UpdateRackHighlights()
        {
            var rackIds = IssueItems
                .Select(x => x.RackId)
                .ToHashSet();

            foreach (var rack in RacksGrid)
            {
                rack.ItemInCell = rackIds.Contains(rack.Id);
            }
        }

        // Обновляет остатки в списке выдачи. Позиции, которых больше нет на складе, убираются из списка.
        private async Task RefreshIssueItemsAsync()
        {
            if (IssueItems.Count == 0 || IsIssue) return;

            var removedNames = new List<string>();

            for (int i = IssueItems.Count - 1; i >= 0; i--)
            {
                var current = IssueItems[i];
                var updated = await _stockService.GetStockByIdAsync(current.StockId);

                if (updated is null)
                {
                    removedNames.Add(current.ComponentName);
                    IssueItems.RemoveAt(i);
                    continue;
                }

                // Сохраняем количество, которое оператор уже ввёл для выдачи
                IssueItems[i] = updated with { OperationQuantity = current.OperationQuantity };
            }

            if (removedNames.Count > 0)
            {
                UpdateRackHighlights();
                UpdateCellHighlights();
                _dialogService.ShowWarning(
                    "Эти товары закончились и убраны из списка выдачи:\n\n• " + string.Join("\n• ", removedNames));
            }
        }
    }
}
