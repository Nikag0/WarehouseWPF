using CommunityToolkit.Mvvm.Input;
using WMS.Domain.ExceptionControl;
using WMS.Desktop.Views;
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
        public ObservableCollection<ComponentEditDto> FilteredComponents { get; } = new();
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
                OnSearchComponentChanged(SearchComponent);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Критический сбой при загрузке данных: {ex.Message}");
            }
        }

        [ObservableProperty] private bool _isEditing;

        partial void OnIsActiveTabChanged(bool value)
        {
            if (!value)
                IsEditing = false;
        }

        [RelayCommand]
        private void AddComponent()
        {
            if (!IsEditing) return;
            var vm = new ComponentEditViewModel(_componentService, _dialogService);
            var window = new ComponentEditWindow(vm) { Owner = WindowOwner.Current };

            window.ShowDialog();
        }

        [RelayCommand]
        private void EditComponent(ComponentEditDto component)
        {
            if (!IsEditing || component is null) return;

            var vm = new ComponentEditViewModel(_componentService, _dialogService, component);
            var window = new ComponentEditWindow(vm) { Owner = WindowOwner.Current };

            window.ShowDialog();
        }

        [RelayCommand]
        private async Task DeleteComponent(ComponentEditDto component)
        {
            if (!IsEditing || component is null || component.HasStock) return;

            if (!_dialogService.ShowConfirmation($"Удалить товар «{component.Name}» ({component.Article})?", "Удаление", "Удалить товар"))
                return;

            try
            {
                await _componentService.DeleteAsync(component.Id);

                FilteredComponents.Remove(component);
                WeakReferenceMessenger.Default.Send(new MessageService(true));
                _dialogService.ShowInfo("Компонент успешно удален.");
            }
            catch (BusinessException ex)
            {
                _dialogService.ShowWarning(ex.Message, "Предупреждение");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Компонент {component.Name} не удалён: не удалось сохранить изменения в базе.\n\n" +
                    $"Подробности: {ex.Message}");
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

                    var result = await _componentService.GetEditFilterAsync(searchText: value, maxCount: 100, token);

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
