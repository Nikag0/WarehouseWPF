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
using WMS.Desktop.ViewModels.MenuViewModels;
using WMS.Application;
using System;
using WMS.Desktop.Services;


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

            services.AddDbContextFactory<AppDbContext>(opt =>
                opt.UseNpgsql(config.GetConnectionString("Warehouse")));

            // репозитории
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IComponentRepository, ComponentRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<IOperatorRepository, OperatorRepository>();
            services.AddScoped<IRackRepository, RackRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<ISectorRepository, SectorRepository>();

            // application services
            services.AddScoped<ComponentService>();
            services.AddScoped<StockService>();
            services.AddScoped<DialogService>();
            services.AddScoped<OperatorService>();
            services.AddScoped<WarehouseService>();
            services.AddScoped<WarehouseVisualizationService>();
            services.AddScoped<HistoryService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<LedStripService>();

            services.AddSingleton<ITcpPacketSender>(new TcpPacketSender(TimeSpan.FromSeconds(3)));

            // view models
            services.AddSingleton<MainViewModel>();
            services.AddTransient<ComponentsViewModel>(); 
            services.AddTransient<ReceiptViewModel>();    
            services.AddTransient<IssueViewModel>();      
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ComponentEditViewModel>();
            services.AddTransient<OperatorEditViewModel>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<NotificationViewModel>();

            // views
            services.AddTransient<Views.SettingsView>();
            services.AddTransient<Views.WarehouseView>();

            // Func
            services.AddTransient<Func<Views.SettingsView>>(provider => () => provider.GetRequiredService<Views.SettingsView>());
            services.AddTransient<Func<Views.WarehouseView>>(provider => () => provider.GetRequiredService<Views.WarehouseView>());

            Services = services.BuildServiceProvider();


            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.Database.MigrateAsync();

            var ledService = scope.ServiceProvider.GetRequiredService<LedStripService>();
            var success = await ledService.InitializeSectorsAsync();

            if (!success)
            {
                MessageBox.Show(
                    "Не удалось инициализировать LED-контроллеры. Проверьте подключение к плате.",
                    "Ошибка LED",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            var window = new Views.MainWindow();
            window.DataContext = Services.GetRequiredService<MainViewModel>();
            window.Show();
        }
    }
}
