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
using WMS.Application.Services;
using WMS.Domain;
using Component = WMS.Domain.Component;

namespace WMS.Desktop.ViewModels
{
    public class ComponentsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Component> Components { get; } = new();
        public ObservableCollection<Component> FilteredComponents { get; } = new();

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

        public ICommand LoadCommand { get; set; }
        public ICommand AddCommand { get; set; }

        private readonly ComponentService _componentService;
        private bool _isLoading;
        private string _searchText;

        public ComponentsViewModel(ComponentService componentService)
        {
            _componentService = componentService;
            LoadCommand = new RelayCommand(LoadAsync);
            AddCommand = new RelayCommand(AddAsync);
        }

        public async Task LoadAsync()
        {
            if (_isLoading) return;

            try
            {
                _isLoading = true;
                Components.Clear();
                var items = await _componentService.GetAllAsync();
                foreach (var item in items)
                    Components.Add(item);

                ApplyFilter();
            }
            finally
            {
                _isLoading = false;
            }
        }

        public async Task AddAsync()
        {
            await _componentService.AddAsync(NewArticle, NewName, NewManufacturer);
            await LoadAsync();
        }

        private void ApplyFilter()
        {
            FilteredComponents.Clear();

            var query = Components.AsEnumerable();

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
