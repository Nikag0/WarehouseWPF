using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Application.Services;

namespace WMS.Desktop.Services
{
    public class WmsDataStore : IWmsDataStore
    {
        private readonly IServiceProvider _serviceProvider;
        public ObservableCollection<ComponentDTO> Components { get; } = new();

        public WmsDataStore(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var componentService = scope.ServiceProvider.GetRequiredService<ComponentService>();

            var items = await componentService.GetAllAsync();

            App.Current.Dispatcher.Invoke(() =>
            {
                Components.Clear();
                foreach (var item in items) Components.Add(item);
            });
        }

        public async Task RefreshComponentsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var componentService = scope.ServiceProvider.GetRequiredService<ComponentService>();
            var data = await componentService.GetAllAsync();

            App.Current.Dispatcher.Invoke(() =>
            {
                Components.Clear();
                foreach (var item in data) Components.Add(item);
            });
        }

    }
}
