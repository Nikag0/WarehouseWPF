using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WMS.Application.Services;

namespace WMS.Desktop.ViewModels
{
    public partial class ComponentsViewModel : ObservableObject
    {
        private readonly ComponentService _componentService;
        private readonly DialogService _dialogService;
        private List<ComponentDTO> _allComponents = new();

        [ObservableProperty] private string _searchComponent = string.Empty;

        public ObservableCollection<ComponentDTO> FilteredComponents { get; } = new();

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
                var componentsResult = await _componentService.GetAllAsync();

                if (componentsResult.IsSuccess)
                {
                    _allComponents = componentsResult.Value.ToList();
                    FilterComponents();
                }
                else
                {
                    _dialogService.ShowWarning(componentsResult.Error, "Ошибка загрузки каталога");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrror($"Критический сбой при загрузке данных: {ex.Message}");
            }
        }

        partial void OnSearchComponentChanged(string value)
        {
            FilterComponents();
        }

        private void FilterComponents()
        {
            FilteredComponents.Clear();

            var query = _allComponents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchComponent))
            {
                var text = SearchComponent.ToLower().Trim();

                query = query.Where(c =>
                        (c.Article != null && c.Article.ToLower().Contains(text)) ||
                        (c.Name != null && c.Name.ToLower().Contains(text)) ||
                        (c.Manufacturer != null && c.Manufacturer.ToLower().Contains(text)));
            }

            foreach (var item in query)
            {
                FilteredComponents.Add(item);
            }
        }
    }
}
