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
        private readonly ComponentService _service;

        public ObservableCollection<Component> Components { get; }
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public string NewArticle { get; set; }
        public string NewName { get; set; }

        public ICommand LoadCommand { get; set; }
        public ICommand AddCommand { get; set; }

        public ComponentsViewModel(ComponentService service)
        {
            _service = service;
            Components = new();
            LoadCommand = new RelayCommand(LoadAsync);
            AddCommand = new RelayCommand(AddAsync);
        }

        public async Task LoadAsync()
        {
            Components.Clear();
            var items = await _service.GetAllAsync();
            foreach (var item in items)
                Components.Add(item);
        }

        public async Task AddAsync()
        {
            await _service.AddAsync(NewArticle, NewName);
            await LoadAsync();
        }
    }
}
