using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
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
        public ICollectionView ComponentsView { get; }
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
                ComponentsView.Refresh();
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

            ComponentsView = CollectionViewSource.GetDefaultView(_dataStore.Components);

            ComponentsView.Filter = FilterComponent;

            _ = InitializeDataAsync();
        }

        private bool FilterComponent(object obj)
        {
            if (obj is not ComponentDTO component) return false;

            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            var text = SearchText.ToLower();

            return (component.Article != null && component.Article.ToLower().Contains(text)) ||
                   (component.Name != null && component.Name.ToLower().Contains(text)) ||
                   (component.Manufacturer != null && component.Manufacturer.ToLower().Contains(text));
        }

        private async Task InitializeDataAsync()
        {
            if (!_dataStore.Components.Any())
            {
                await RefreshAsync();
            }
        }

        public async Task RefreshAsync()
        {
            try
            {
                await _dataStore.RefreshComponentsAsync();
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

        //private void ApplyFilter()
        //{
        //    var query = _dataStore.Components.AsEnumerable();

        //    if (!string.IsNullOrWhiteSpace(SearchText))
        //    {
        //        var text = SearchText.ToLower();

        //        query = query.Where(c =>
        //                c.Article != null && c.Article.ToLower().Contains(text) ||
        //                c.Name != null && c.Name.ToLower().Contains(text) ||
        //                c.Manufacturer != null && c.Manufacturer.ToLower().Contains(text));
        //    }

        //    var resultList = query.ToList();

        //    FilteredComponents.Clear();

        //    foreach (var item in resultList) FilteredComponents.Add(item);

        //    OnPropertyChanged(nameof(FilteredComponents));
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
