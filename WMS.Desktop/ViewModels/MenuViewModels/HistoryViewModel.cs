using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WMS.Desktop.ViewModels.MenuViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private HistoryService _historyService;

        private CancellationTokenSource? _cts;

        private string _searchHistory;
        public string SearchHistory
        {
            get => _searchHistory;
            
            set 
            {
                if (_searchHistory == value) return;

                _searchHistory = value;

                OnPropertyChanged(nameof(SearchHistory));

                SearchHistoryChanged();
            }
        }

        private DateTime _dateFrom = DateTime.Today.AddDays(-1);
        public DateTime DateFrom
        {
            get => _dateFrom;

            set
            {
                if (_dateFrom == value) return;

                _dateFrom = value;

                OnPropertyChanged(nameof(DateFrom));

                SearchHistoryChanged();
            }
        }

        private DateTime _dateTo = DateTime.Today;
        public DateTime DateTo
        {
            get => _dateTo;

            set
            {
                if (_dateTo == value) return;

                _dateTo = value;

                OnPropertyChanged(nameof(DateTo));

                SearchHistoryChanged();
            }
        }

        private string _selectedOperationType;
        public string SelectedOperationType
        {
            get => _selectedOperationType;
            set
            {
                if (_selectedOperationType == value) return;
                _selectedOperationType = value;
                OnPropertyChanged(nameof(SelectedOperationType));
                SearchHistoryChanged(); 
            }
        }


        public ObservableCollection<HistoryDto> History { get; } = [];
        public ObservableCollection<string> Type { get; set; } = new();

        public HistoryViewModel(HistoryService historyService)
        {
            _historyService = historyService;
        }

        public async Task LoadDataAsync()
        {
            LoadType();
            SearchHistoryChanged();
        }

        private void SearchHistoryChanged()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var token = _cts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1000, token);

                    var data = await _historyService.GetFilteredAsync(
                        SearchHistory,
                        DateFrom,
                        DateTo,
                        LocationFormatter.StrToOperation(SelectedOperationType),
                        100,
                        _cts.Token);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (!token.IsCancellationRequested)
                        {
                            History.Clear();
                            foreach (var item in data)
                            {
                                History.Add(item);
                            }
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    // Задача отменена новым вводом текста — ничего не делаем
                }
            });
        }

        private void LoadType()
        {
            Type.Clear();
            Type.Add(LocationFormatter.OperationToStr(OperationType.All));     
            Type.Add(LocationFormatter.OperationToStr(OperationType.Receipt)); 
            Type.Add(LocationFormatter.OperationToStr(OperationType.Issue));   

            SelectedOperationType = Type[0];
        }
    }
}
