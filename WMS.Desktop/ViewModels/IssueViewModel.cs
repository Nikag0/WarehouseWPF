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

        //Изменения кончлились

        public ObservableCollection<ViewItemDTO> IssueItems { get; } = new();
        public ObservableCollection<RackViewModel> Racks { get; } = new();
        public List<Cell> _cells { get; } = new();
        public ObservableCollection<CellViewModel> Cells{ get; } = new();
        private Dictionary<RackType, List<CellLayout>> _cellLayouts;
        public ObservableCollection<Operator> Operators { get; } = new();

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

        private readonly List<ViewItemDTO> _stocks = new();
        private bool _isLoading;

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

            var sb = new StringBuilder();

            if (invalidItems.Any())
            {
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

            sb.AppendLine("Вы уверены, что хотите выполнить выдачу?");
            sb.AppendLine();
            sb.AppendLine("Список товаров:");

            foreach (var item in IssueItems)
            {
                sb.AppendLine($"• {item.ComponentName} Количество: {item.OperationQuantity}");
                sb.AppendLine();
            }

            if (!_dialogService.ShowConfirmation(sb.ToString()))
                return;

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

                await LoadStocks();
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

        public async Task LoadWindow()
        {
            if (_isLoading) return;

            _isLoading = true;

            try
            {
                await LoadStocks();

                await LoadRacks();
                UpdateRackHighlights();

                _cells.Clear();
                var cells = await _cellService.GetAllAsync();
                foreach (var item in cells)
                    _cells.Add(item);

                LoadCellLayouts();
                UpdateCellHighlights();

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
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadStocks()
        {
            _stocks.Clear();
            _stocks.AddRange(await _stockService.GetAllAsync());
        }

        public async Task LoadOperators()
        {
            try
            {
                Operators.Clear();
                var items = await _operatorService.GetAllAsync();
                foreach (var item in items)
                    Operators.Add(item);
            }
            catch (OverallDomainException ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadRacks()
        {
            Racks.Clear();

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RackDescription.json");

            var layouts = JsonSerializer.Deserialize<List<RackLayout>>(
                File.ReadAllText(path));

            var layoutDict = layouts.ToDictionary(l => l.Code);

            var racksDb = await _rackService.GetAllAsync();

            foreach (var rack in racksDb)
            {
                if (!layoutDict.TryGetValue(rack.RackCode, out var layout))
                    continue;

                Racks.Add(new RackViewModel(rack, layout));
            }
        }

        private void LoadCellLayouts()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CellDescription.json");

            var raw = JsonSerializer.Deserialize<List<CellLayoutRoot>>(File.ReadAllText(path));

            _cellLayouts = raw.ToDictionary(
                x => Enum.Parse<RackType>(x.Type),
                x => x.Cells);
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

            Cells.Clear();

            if (!_cellLayouts.TryGetValue(SelectedRack.Type, out var layout))
                return;

            var rackCells = _cells.Where(c => c.RackId == SelectedRack.Id);

            foreach (var cell in rackCells)
            {
                var cellLayout = layout.FirstOrDefault(l => l.Code == cell.CellCode);
                if (cellLayout == null)
                    continue;

                Cells.Add(new CellViewModel(cell, cellLayout));
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
