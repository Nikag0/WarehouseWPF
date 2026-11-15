using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WMS.Application.Services;
using WMS.Desktop.Services;

namespace WMS.Desktop.ViewModels.MenuViewModels
{
    public partial class ComponentsViewModel : ObservableObject
    {
        public ObservableCollection<ComponentViewDto> FilteredComponents { get; } = new();
        [ObservableProperty] private string _searchComponent = string.Empty;
        [ObservableProperty] private bool _isActiveTab;

        private readonly ComponentService _componentService;
        private readonly DialogService _dialogService;
        private CancellationTokenSource? _cts;

        public ComponentsViewModel(
            ComponentService componentService, 
            DialogService dialogService)
        {
            _componentService = componentService;
            _dialogService = dialogService;

            WeakReferenceMessenger.Default.Register<MessageService>(this, async (r, m) =>
            {
                if (IsActiveTab)
                {
                    await LoadDataAsync();
                }
            });
        }

        public async Task LoadDataAsync()
        {
            try
            {
                OnSearchComponentChanged(_searchComponent);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Критический сбой при загрузке данных: {ex.Message}");
            }
        }

        partial void OnSearchComponentChanged(string value)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            // Запоминаем токен этого запроса: поле _cts к моменту ответа может уже указывать на более новый поиск
            var token = _cts.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500, token);

                    var result = await _componentService.GetViewFilterAsync(searchText: value, maxCount: 100, token);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (!token.IsCancellationRequested)
                        {
                            FilteredComponents.Clear();
                            foreach (var component in result)
                            {
                                FilteredComponents.Add(component);
                            }
                        }
                    });
                }
                catch (OperationCanceledException) {}
                catch (Exception ex)
                {
                    // Без этого ошибка фоновой задачи потерялась бы, и пользователь просто не увидел бы результатов
                    _dialogService.ShowError($"Не удалось выполнить поиск: {ex.Message}");
                }
            });
        }
    }
}
