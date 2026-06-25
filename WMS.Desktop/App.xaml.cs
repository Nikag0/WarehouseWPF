using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Desktop.ViewModels;
using WMS.Infrastructure;
using WMS.Infrastructure.Migrations;
using Microsoft.Extensions.Logging;


namespace WMS.Desktop
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider? Services { get; private set; }

        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddDebug(); // Логи будут сыпаться во вкладку Output (Вывод) в Visual Studio
            });

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            services.AddDbContextFactory<WmsDbContext>(opt =>
                opt.UseNpgsql(config.GetConnectionString("Warehouse")));

            // репозитории
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IComponentRepository, ComponentRepository>();
            services.AddScoped<IOperationRepository, HistoryRepository>();
            services.AddScoped<IRackRepository, RackRepository>();
            services.AddScoped<IOperatorRepository, OperatorRepository>();

            // application services
            services.AddScoped<ComponentService>();
            services.AddScoped<IssueService>();
            services.AddScoped<InventoryService>();
            services.AddScoped<ReceiptService>();
            services.AddScoped<StockService>();
            services.AddScoped<RackService>();
            services.AddScoped<DialogService>();
            services.AddScoped<OperatorService>();
            services.AddScoped<WarehouseService>();

            // view models
            services.AddSingleton<MainViewModel>();
            services.AddTransient<NotificationViewModel>();
            services.AddTransient<ComponentsViewModel>(); 
            services.AddTransient<ReceiptViewModel>();    
            services.AddTransient<IssueViewModel>();      
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ComponentEditViewModel>();
            services.AddTransient<OperatorEditViewModel>();
            services.AddTransient<HistoryViewModel>();

            // views
            services.AddTransient<Views.SettingsView>();

            // Func
            services.AddTransient<Func<Views.SettingsView>>(provider => () => provider.GetRequiredService<Views.SettingsView>());

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
