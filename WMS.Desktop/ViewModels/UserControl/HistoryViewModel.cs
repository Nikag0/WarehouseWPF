using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WMS.Desktop.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private HistoryService _historyService;

        private CancellationTokenSource? _cts;

        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private DateTime? dateFrom;
        [ObservableProperty] private DateTime? dateTo;
        [ObservableProperty] private OperationType? selectedOperationType;
        public ObservableCollection<HistoryDto> History { get; } = [];
        public ObservableCollection<OperationType> Type { get; } = [];

        public HistoryViewModel(HistoryService historyService)
        {
            _historyService = historyService;
        }

        public async Task LoadDataAsync()
        {
            LoadType();
            OnSearchTextChanged(SearchText);
        }

        partial void OnSearchTextChanged(string value)
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
                        SearchText,
                        DateFrom,
                        DateTo,
                        SelectedOperationType,
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
            Type.Add(OperationType.Issue);
            Type.Add(OperationType.Receipt);
        }
    }
}
