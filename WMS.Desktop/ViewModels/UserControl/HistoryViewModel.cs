using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Abstractions;

namespace WMS.Desktop.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private readonly IOperationRepository _operationRepository;

        private CancellationTokenSource? _cts;

        [ObservableProperty] private string _searchText = string.Empty;

        public ObservableCollection<OperationHistoryDto> HistoryItems { get; } = new();

        public HistoryViewModel(IOperationRepository operationRepository)
        {
            _operationRepository = operationRepository;
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

                    var data = await _operationRepository.GetFilteredHistoryAsync(value);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (!token.IsCancellationRequested)
                        {
                            HistoryItems.Clear();
                            foreach (var item in data)
                            {
                                HistoryItems.Add(item);
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

        public async Task LoadDataAsync()
        {
            var data = await _operationRepository.GetFilteredHistoryAsync(SearchText);

            HistoryItems.Clear();
            foreach (var item in data)
            {
                HistoryItems.Add(item);
            }
        }
    }
}
