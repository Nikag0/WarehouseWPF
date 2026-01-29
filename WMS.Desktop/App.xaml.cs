using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Desktop.ViewModels;
using WMS.Infrastructure;

namespace WMS.Desktop
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider? Services { get; private set; }

        protected async override void OnStartup(StartupEventArgs e)
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
            services.AddScoped<IOperationRepository, OperationRepository>();
            services.AddScoped<ICellRepository, CellRepository>();

            // application services
            services.AddScoped<ComponentService>();
            services.AddScoped<IssueService>();
            services.AddScoped<InventoryService>();
            services.AddScoped<ReceiptService>();
            services.AddScoped<StockService>();
            services.AddScoped < CellService>();

            // view models
            services.AddTransient<MainViewModel>();
            services.AddTransient<StockViewModel>();
            services.AddTransient<ComponentsViewModel>();

            Services = services.BuildServiceProvider();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WmsDbContext>();

            await db.Database.MigrateAsync();

            var window = new Views.MainWindow();
            window.DataContext = Services.GetRequiredService<MainViewModel>();
            window.Show();
        }
    }
}
