using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WMS.Application.WarehouseVisualization;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using Component = WMS.Domain.Component;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WMS.Desktop.ViewModels
{
    public partial class IssueViewModel : ObservableObject
    {
        public ObservableCollection<ViewItemDTO> FilteredStocks { get; } = new();
        public ObservableCollection<RackViewModel> RacksGrid { get; } = new();
        public ObservableCollection<CellViewModel> CellsGrid { get; } = new();
        public ObservableCollection<Operator> Operators { get; } = new();
        public ObservableCollection<ViewItemDTO> IssueItems { get; } = new();
        public RackViewModel SelectedRack 
        {
            get => _selectedRack;
            set
            {
                _selectedRack = value;
                OnPropertyChanged();

                foreach (var rack in RacksGrid)
                    rack.IsSelected = rack == value;

                SelectCellsForRack();
                UpdateCellHighlights();
            }
        }

        private readonly CellService _cellService;
        private readonly DialogService _dialogService;
        private readonly IssueService _issueService;
        private readonly OperatorService _operatorService;
        private readonly RackService _rackService;
        private readonly StockService _stockService;

        private CancellationTokenSource? _cts;
        
        private Dictionary<Guid, List<CellViewModel>> _groupedCellsCache = new(); // словарь всех ячеек, привязанных к RackId
        [ObservableProperty] private string _commentText = string.Empty;
        [ObservableProperty] private bool _isIssue;
        [ObservableProperty] private bool _isIssuePopupOpen;
        [ObservableProperty] private Operator? _operatorName;
        [ObservableProperty] private string _searchStock = string.Empty;

        private RackViewModel _selectedRack;
        private CellViewModel _selectedCell;

        public IssueViewModel(
                    StockService stockService,
            IssueService issueService,
            DialogService dialogService,
            OperatorService operatorService,
            CellService cellService,
            RackService rackService)
        {
            _issueService = issueService;
            _stockService = stockService;
            _dialogService = dialogService;
            _operatorService = operatorService;
            _cellService = cellService;
            _rackService = rackService;
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
            var invalidItems = IssueItems
                              .Where(x => x.OperationQuantity <= 0)
                              .ToList();

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

            if (OperatorName == null)
            {
                _dialogService.ShowWarning("Оператор не указан");
                return;
            }

            var itemsToReview = IssueItems.ToList();

            RacksGrid.All(r => r.ItemInCell = false);
            CellsGrid.All(r => r.ItemInCell = false);

            foreach (var item in itemsToReview)
            {
                SelectedRack = RacksGrid.First(r => r.Id == item.RackId);

                _selectedCell = CellsGrid.First(c => c.Id == item.CellId);
                _selectedCell.ItemInCell = true;

                var question = $"Получилось найти товар?\n\n• {item.ComponentName} Стеллаж:{item.RackCodeDisplay} Ячейка:{item.CellCodeDisplay} Количество: {item.OperationQuantity}";

                if (!_dialogService.ShowConfirmation(question))
                {
                    SelectedRack.ItemInCell = false;
                    _selectedCell.ItemInCell = false;
                    IssueItems.Remove(item);
                }
            }

            if (!IssueItems.Any())
            {
                _dialogService.ShowInfo("Выдача отменена, так как ни один товар не был найден.");
                ClearIssueItems();
                return;
            }

            try
            {
                var issueOperation = IssueItems
                    .Select(item => new ServiceItemDTO(
                        item.ComponentId,
                        item.StockId,
                        item.RackId,
                        item.CellId,
                        item.OperationQuantity))
                    .ToList();

                await _issueService.IssueAsync(issueOperation, OperatorName.FullName, CommentText);

                CommentText = string.Empty;
                SearchStock = string.Empty;
                IsIssue = true;
                _dialogService.ShowInfo("Выдача успешно выполнена.");
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
        }

        public async Task LoadDataAsync()
        {
            try
            {
                await Task.WhenAll(
                    CreateRacksGridAsync(),
                    LoadCellsLookupAsync(),
                    LoadOperators()
                );

                UpdateRackHighlights();
                UpdateCellHighlights();

                if (SelectedRack != null)
                {
                    SelectedRack = RacksGrid.FirstOrDefault(r => r.Id == SelectedRack.Id);
                }
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
        }

        [RelayCommand]
        public void RemoveIssueItem(object obj)
        {
            if (obj is not ViewItemDTO item)
                return;

            IssueItems.Remove(item);
            UpdateRackHighlights();
            UpdateCellHighlights();
        }

        [RelayCommand]
        public void SelectCell(CellViewModel cell)
        {
            return;
        }

        [RelayCommand]
        public async Task SelectRack(RackViewModel rack)
        {
            if (rack == null)
                return;

            SelectedRack = rack;
        }

        [RelayCommand]
        private async Task AddIssueItem(object obj)
        {
            if (obj is not ViewItemDTO item)
                return;

            if (IssueItems.Any(x => x.RackId == item.RackId))
                return;

            try
            {
                var stock = await _stockService.GetStockAsync(item.StockId);
                IssueItems.Add(stock);
                UpdateRackHighlights();
                UpdateCellHighlights();
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }

        }

        private async Task CreateRacksGridAsync()
        {
            try
            {
                var racksData = await _rackService.GetRacksWithLayoutsAsync();

                var preparedViewModels = await Task.Run(() =>
                {
                    var list = new List<RackViewModel>();
                    foreach (var item in racksData)
                    {
                        list.Add(new RackViewModel(item.Rack, item.Layout));
                    }
                    return list;
                });

                RacksGrid.Clear();
                foreach (var vm in preparedViewModels)
                {
                    RacksGrid.Add(vm);
                }
            }
            catch (FileNotFoundException ex)
            {
                _dialogService.ShowError($"{ex.Message}");
            }
        }

        private async Task LoadCellsLookupAsync()
        {
            try
            {
                var allCellsWithLayouts = await _cellService.GetCellsWithLayoutsAsync();

                _groupedCellsCache = allCellsWithLayouts.GroupBy(pair => pair.cell.RackId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(pair => new CellViewModel(pair.cell, pair.layout)).ToList()
                    );
            }
            catch (FileNotFoundException ex)
            {
                _dialogService.ShowError($"{ex.Message}");
            }
        }

        private async Task LoadOperators()
        {
            try
            {
                var data = await _operatorService.GetAllAsync();
                Operators.Clear();
                foreach (var @operator in data)
                    Operators.Add(@operator);
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
        }

        partial void OnSearchStockChanged(string value)
        {
            _cts?.Cancel();

            if (string.IsNullOrWhiteSpace(value))
            {
                FilteredStocks.Clear();
                IsIssuePopupOpen = false;
                return;
            }

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

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
            });
        }

        private void SelectCellsForRack()
        {
            if (SelectedRack == null)
                return;

            CellsGrid.Clear();

            if (_groupedCellsCache.TryGetValue(SelectedRack.Id, out var cachedCells))
            {
                foreach (var cellVm in cachedCells)
                {
                    CellsGrid.Add(cellVm);
                }
            }
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
    }
}
