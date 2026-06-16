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
using WMS.Application.Abstractions;
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
        public ObservableCollection<RackViewModel> Racks { get; } = new();
        public ObservableCollection<CellViewModel> Cells{ get; } = new();
        private Dictionary<RackType, List<CellLayout>> _cellLayouts;
        private Dictionary<string, RackLayout>? _rackLayoutDict;
        private List<Cell> _cells { get; } = new();

        public RackViewModel SelectedRack 
        {
            get => _selectedRack;
            set
            {
                _selectedRack = value;
                OnPropertyChanged();

                foreach (var rack in Racks)
                    rack.IsSelected = rack == value;

                _ = LoadCellsForSelectedRack(); 
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
                    LoadRacksAsync(),
                    LoadCellAsync(),
                    LoadOperators()
                );

                UpdateRackHighlights();
                UpdateCellHighlights();

                _cells.Clear();
                var cells = await _cellService.GetAllAsync();
                foreach (var item in cells)
                    _cells.Add(item);


                if (SelectedRack != null)
                {
                    SelectedRack = Racks.FirstOrDefault(r => r.Id == SelectedRack.Id);
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

        private async Task LoadRacksAsync()
        {
            try
            {
                if (_rackLayoutDict == null)
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RackDescription.json");

                    var jsonText = await File.ReadAllTextAsync(path);
                    var layouts = JsonSerializer.Deserialize<List<RackLayout>>(jsonText);

                    _rackLayoutDict = layouts?.ToDictionary(l => l.Code) ?? new();
                }

                var data = await _rackService.GetAllAsync();

                var freshRacks = new List<RackViewModel>();
                foreach (var rack in data)
                {
                    if (_rackLayoutDict.TryGetValue(rack.RackCode, out var layout))
                    {
                        freshRacks.Add(new RackViewModel(rack, layout));
                    }
                }

                Racks.Clear();
                foreach (var rackViewModel in freshRacks)
                {
                    Racks.Add(rackViewModel);
                }
            }
            catch (OverallDomainException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                _dialogService.ShowWarning($"Ошибка загрузки стеллажей: {ex.Message}");
            }

        }

        private async Task LoadCellAsync()
        {
            if (_cellLayouts != null) return;

            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CellDescription.json");

                var jsonText = await File.ReadAllTextAsync(path);

                var raw = JsonSerializer.Deserialize<List<CellLayoutRoot>>(jsonText);
                if (raw == null) return;

                var tempDict = new Dictionary<RackType, List<CellLayout>>();

                foreach (var item in raw)
                {
                    if (Enum.TryParse<RackType>(item.Type, ignoreCase: true, out var rackType))
                    {
                        tempDict[rackType] = item.Cells;
                    }
                }

                _cellLayouts = tempDict;
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                _dialogService.ShowWarning($"Ошибка загрузки  ячеек: {ex.Message}");
            }
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

            foreach (var rack in Racks)
            {
                rack.IsHighlighted = rackIds.Contains(rack.Id);
            }
        }

        private void UpdateCellHighlights()
        {
            var cellIds = IssueItems
                .Select(x => x.CellId)
                .ToHashSet();

            foreach (var cell in Cells)
            {
                cell.IsHighlighted = cellIds.Contains(cell.Id);
            }
        }

        private async Task LoadCellsForSelectedRack()
        {
            if (SelectedRack == null)
                return;

            if (_cellLayouts == null || !_cellLayouts.TryGetValue(SelectedRack.Type, out var layout))
            {
                Cells.Clear();
                return;
            }

            var layoutDict = layout.ToDictionary(l => l.Code);
            var freshCells = new List<CellViewModel>();

            var rackCells = _cells.Where(c => c.RackId == SelectedRack.Id);

            foreach (var cell in rackCells)
            {
                if (layoutDict.TryGetValue(cell.CellCode, out var cellLayout))
                {
                    freshCells.Add(new CellViewModel(cell, cellLayout));
                }
            }

            Cells.Clear();
            foreach (var cellViewModel in freshCells)
            {
                Cells.Add(cellViewModel);
            }

            UpdateCellHighlights();
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
