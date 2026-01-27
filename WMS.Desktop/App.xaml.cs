using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using WMS.Application;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Desktop.ViewModels;
using WMS.Infrastructure;

namespace WMS.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            services.AddDbContext<WmsDbContext>(opt =>
                opt.UseNpgsql(
                    config.GetConnectionString("Warehouse")));

            // репозитории
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IComponentRepository, ComponentRepository>();
            services.AddScoped<ComponentService>();
            services.AddScoped<IOperationRepository, OperationRepository>();

            // application services
            services.AddScoped<IssueService>();
            services.AddScoped<InventoryService>();
            services.AddScoped<ReceiptService>();

            // view models
            services.AddTransient<ComponentsViewModel>();
            services.AddTransient<MainViewModel>();

            Services = services.BuildServiceProvider();

            var window = new Views.MainWindow();
            window.DataContext = Services.GetRequiredService<MainViewModel>();
            window.Show();
        }
    }
}
