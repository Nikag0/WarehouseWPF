using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels
{
    public class ComponentsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ComponentDTO> FilteredComponents { get; } = new();

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

        private readonly IWmsDataStore _dataStore;
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
            IWmsDataStore dataStore,
            ComponentService componentService, 
            DialogService dialogService)
        {
            _dataStore = dataStore;
            _componentService = componentService;
            _dialogService = dialogService;

            LoadCommand = new RelayCommand(RefreshAsync);
            AddCommand = new RelayCommand(AddAsync);
            AddModeOnCommand = new RelayCommand(_ =>{IsAdding = true;});
            AddModeOffCommand = new RelayCommand(_ =>{IsAdding = false;});

            _dataStore.Components.CollectionChanged += OnComponentsCacheChanged;

            _ = InitializeDataAsync();
        }

        private async Task InitializeDataAsync()
        {
            if (!_dataStore.Components.Any())
            {
                await RefreshAsync();
            }
            else
            {
                ApplyFilter();
            }
        }

        private void OnComponentsCacheChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        public async Task RefreshAsync()
        {

            try
            {
                await _dataStore.RefreshComponentsAsync();

                ApplyFilter();
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

        public async Task AddAsync()
        {
            try 
            {
                await _componentService.AddAsync(NewArticle, NewName, NewManufacturer);
                await _dataStore.RefreshComponentsAsync();
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

        private void ApplyFilter()
        {
            var query = _dataStore.Components.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.ToLower();

                query = query.Where(c =>
                        c.Article != null && c.Article.ToLower().Contains(text) ||
                        c.Name != null && c.Name.ToLower().Contains(text) ||
                        c.Manufacturer != null && c.Manufacturer.ToLower().Contains(text));
            }

            var resultList = query.ToList();

            FilteredComponents.Clear();

            foreach (var item in resultList) FilteredComponents.Add(item);

            OnPropertyChanged(nameof(FilteredComponents));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
