using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WMS.Application.Services;
using WMS.Domain;

namespace WMS.Desktop.ViewModels
{
    public class ComponentsViewModel : INotifyCollectionChanged
    {
        private readonly ComponentService _componentService;
        private bool _isLoading;

        public ObservableCollection<Component> Components { get; } = new();

        public string NewArticle { get; set; }
        public string NewName { get; set; }
        public string NewManufacturer { get; set; }

        public ICommand LoadCommand { get; set; }
        public ICommand AddCommand { get; set; }

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

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
    }
}
