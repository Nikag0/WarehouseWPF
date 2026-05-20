using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using static System.Net.Mime.MediaTypeNames;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels
{
    public partial class ComponentsViewModel : ObservableObject
    {

        [ObservableProperty] private ObservableCollection<ComponentDTO> _filteredComponents = new();
        public string NewArticle { get; set; }
        public string NewName { get; set; }
        public string NewManufacturer { get; set; }
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
        public bool IsAdding
        {
            get => _isAdding;
            set
            {
                _isAdding = value;
                OnPropertyChanged();
            }
        }

        private List<ComponentDTO> components = new();
        private readonly ComponentService _componentService;
        private readonly DialogService _dialogService;
        private string _searchText;
        private bool _isAdding;

        public ICommand LoadCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand DeletCommand { get; set; }
        public ICommand AddModeOnCommand { get; set; }
        public ICommand AddModeOffCommand { get; set; }

        public ComponentsViewModel(
            ComponentService componentService, 
            DialogService dialogService)
        {
            _componentService = componentService;
            _dialogService = dialogService;

            AddCommand = new RelayCommand(AddAsync);
            AddModeOnCommand = new RelayCommand(_ =>{IsAdding = true;});
            AddModeOffCommand = new RelayCommand(_ =>{IsAdding = false;});

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Исправлено: получаем объект Result из сервиса компонентов
                var componentsResult = await _componentService.GetAllAsync();

                if (componentsResult.IsSuccess)
                {
                    // Читаем данные через .Value, как заложено в паттерне Result<T>
                    FilteredComponents = new ObservableCollection<ComponentDTO>(componentsResult.Value);
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

        private void ApplyFilter()
        {
            FilteredComponents.Clear();

            var query = components.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.ToLower();

                query = query.Where(c =>
                        c.Article != null && c.Article.ToLower().Contains(text) ||
                        c.Name != null && c.Name.ToLower().Contains(text) ||
                        c.Manufacturer != null && c.Manufacturer.ToLower().Contains(text));
            }

            foreach (var item in query)
                FilteredComponents.Add(item);
        }

        public async Task AddAsync()
        {
            try 
            {
                await _componentService.AddAsync(NewArticle, NewName, NewManufacturer);
                IsAdding = false;

                NewArticle = string.Empty;
                NewName = string.Empty;
                NewManufacturer = string.Empty;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
