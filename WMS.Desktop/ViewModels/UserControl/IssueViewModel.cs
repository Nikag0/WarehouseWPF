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
        private readonly IssueService _issueService;
        private readonly StockService _stockService;
        private readonly DialogService _dialogService;
        private readonly OperatorService _operatorService;
        private readonly CellService _cellService;
        private readonly RackService _rackService;

        private CancellationTokenSource? _cts;

        [ObservableProperty] private string _commentText = string.Empty;
        [ObservableProperty] private Operator? _operatorName;
        [ObservableProperty] private bool _isIssue;
        [ObservableProperty] private bool _isIssuePopupOpen;
        [ObservableProperty] private string _searchText = string.Empty;

        [ObservableProperty] private ObservableCollection<ViewItemDTO> _filteredStocks = new();

        public ObservableCollection<ViewItemDTO> IssueItems { get; } = new();
        public ObservableCollection<Operator> Operators { get; } = new();
        public ObservableCollection<RackViewModel> RacksGrid { get; } = new();
        public ObservableCollection<CellViewModel> CellsGrid{ get; } = new();
        private Dictionary<Guid, List<CellViewModel>> _groupedCellsCache = new(); // словарь всех ячеек, привязанных к RackId

        public RackViewModel SelectedRack 
        {
            get => _selectedRack;
            set
            {
                _selectedRack = value;
                OnPropertyChanged();

                foreach (var rack in RacksGrid)
                    rack.IsSelected = rack == value;

                _ = SelectCellsForRack(); 
            }
        }
        private RackViewModel _selectedRack;

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

            var itemsToRemove = new List<ViewItemDTO>();

            foreach (var item in itemsToReview)
            {
                var question = $"Получилось найти товар?\n\n• {item.ComponentName} Стеллаж:{item.RackCodeDisplay} Ячейка:{item.CellCodeDisplay} Количество: {item.OperationQuantity}";

                if (!_dialogService.ShowConfirmation(question))
                {
                    itemsToRemove.Add(item);
                }
            }

            foreach (var item in itemsToRemove)
            {
                IssueItems.Remove(item);
            }

            if (!IssueItems.Any())
            {
                _dialogService.ShowInfo("Выдача отменена, так как ни один товар не был найден.");
                return;
            }

            try
            {
                var issueOperation = IssueItems
                    .Select(item => new ServiceItemDTO(
                        item.ComponentId,
                        item.RackId,
                        item.CellId,
                        item.OperationQuantity))
                    .ToList();

                await _issueService.IssueAsync(issueOperation, OperatorName.FullName, CommentText);

                CommentText = string.Empty;
                IsIssue = true;
                _dialogService.ShowInfo("Выдача успешно выполнена.");
            }
            catch (WrongValueExeption ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (OverallDomainException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
        }

        [RelayCommand]
        private async void SelectRack(RackViewModel rack)
        {
            if (rack == null)
                return;

            SelectedRack = rack;
        }

        [RelayCommand]
        private void SelectCell(CellViewModel cell)
        {
            return;
        }

        [RelayCommand]
        private void AddIssueItem(object obj)
        {
            if (obj is not ViewItemDTO item)
                return;

            if (IssueItems.Any(x => x.ComponentId == item.ComponentId && x.CellId == item.CellId))
                return;

            IssueItems.Add(item);
            UpdateRackHighlights();
            UpdateCellHighlights();
        }

        [RelayCommand]
        private void RemoveIssueItem (object obj)
        {
            if (obj is not ViewItemDTO item)
                return;

            IssueItems.Remove(item);
            UpdateRackHighlights();
            UpdateCellHighlights();
        }

        [RelayCommand]
        private void ClearIssueItems()
        {
            IssueItems.Clear();
            UpdateRackHighlights();
            UpdateCellHighlights();
            IsIssue = false; 
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
            catch (OverallDomainException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
        }

        private async Task CreateRacksGridAsync()
        {
            try
            {
                var racksData = await _rackService.GetRacksWithLayoutsAsync();

                RacksGrid.Clear();

                foreach (var item in racksData)
                {
                    RacksGrid.Add(new RackViewModel(item.Rack, item.Layout));
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

        private async Task SelectCellsForRack()
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

            UpdateCellHighlights();
        }

        public async Task LoadOperators()
        {
            try
            {
                var data = await _operatorService.GetAllAsync();
                Operators.Clear();
                foreach (var @operator in data)
                    Operators.Add(@operator);
            }
            catch (OverallDomainException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
        }

        private void UpdateRackHighlights()
        {
            var rackIds = IssueItems
                .Select(x => x.RackId)
                .ToHashSet();

            foreach (var rack in RacksGrid)
            {
                rack.IsHighlighted = rackIds.Contains(rack.Id);
            }
        }

        private void UpdateCellHighlights()
        {
            var cellIds = IssueItems
                .Select(x => x.CellId)
                .ToHashSet();

            foreach (var cell in CellsGrid)
            {
                cell.IsHighlighted = cellIds.Contains(cell.Id);
            }
        }

        partial void OnSearchTextChanged(string value)
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
    }
}
