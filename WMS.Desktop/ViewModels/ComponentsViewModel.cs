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
using static System.Net.Mime.MediaTypeNames;
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

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                components.Clear();
                var items = await _componentService.GetAllAsync();
                foreach (var item in items)
                    components.Add(item);

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
