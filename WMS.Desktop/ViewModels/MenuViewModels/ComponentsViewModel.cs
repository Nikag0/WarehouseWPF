using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WMS.Application.Services;
using WMS.Desktop.Services;

namespace WMS.Desktop.ViewModels.MenuViewModels
{
    public partial class ComponentsViewModel : ObservableObject
    {
        public ObservableCollection<ComponentDTO> FilteredComponents { get; } = new();
        [ObservableProperty] private string _searchComponent = string.Empty;

        private readonly ComponentService _componentService;
        private readonly DialogService _dialogService;
        private CancellationTokenSource? _cts;

        public ComponentsViewModel(
            ComponentService componentService, 
            DialogService dialogService)
        {
            _componentService = componentService;
            _dialogService = dialogService;
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

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500, _cts.Token);

                    var result = await _componentService.GetFilteredAsync(searchText: value, maxCount: 100, _cts.Token);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (!_cts.Token.IsCancellationRequested)
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
            });
        }
    }
}
