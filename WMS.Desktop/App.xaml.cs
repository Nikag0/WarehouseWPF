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
using WMS.Infrastructure.Context;


namespace WMS.Desktop
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider? Services { get; private set; }

        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            // Логирование
            services.AddLogging(builder =>
            {
                builder.AddDebug();
            });

            // Файл конфигурации
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // Создаём контекст через фабрику
            services.AddDbContextFactory<AppDbContext>(opt =>
               opt.UseNpgsql(config.GetConnectionString("Warehouse")));

            var dbContextHolder = new DbContextHolder();
            services.AddSingleton<IDbContextAccessor>(dbContextHolder);
            services.AddSingleton(dbContextHolder);

            // Ипользование фабрики контекста и IUnitOfWork
            services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory>();

            // Репозитории
            services.AddTransient<IStockRepository, StockRepository>();
            services.AddTransient<IComponentRepository, ComponentRepository>();
            services.AddTransient<IHistoryRepository, HistoryRepository>();
            services.AddTransient<IOperatorRepository, OperatorRepository>();
            services.AddTransient<IRackRepository, RackRepository>();
            services.AddTransient<ISectorRepository, SectorRepository>();

            // Сервисы
            services.AddTransient<IStockService, StockService>();
            services.AddTransient<ComponentService>();
            services.AddTransient<DialogService>();
            services.AddTransient<OperatorService>();
            services.AddTransient<WarehouseService>();
            services.AddTransient<WarehouseVisualizationService>();
            services.AddTransient<HistoryService>();
            services.AddTransient<NotificationService>();
            services.AddTransient<LedStripService>();

            services.AddSingleton<ITcpPacketSender>(new TcpPacketSender(TimeSpan.FromSeconds(3)));

            /// 5. View Models и Views
            services.AddSingleton<MainViewModel>(); // Главная
            services.AddTransient<ComponentsViewModel>();
            services.AddTransient<ReceiptViewModel>();
            services.AddTransient<IssueViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ComponentEditViewModel>();
            services.AddTransient<OperatorEditViewModel>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<NotificationViewModel>();

            services.AddTransient<Views.SettingsView>();
            services.AddTransient<Views.WarehouseView>();

            services.AddTransient<Func<Views.SettingsView>>(provider => () => provider.GetRequiredService<Views.SettingsView>());
            services.AddTransient<Func<Views.WarehouseView>>(provider => () => provider.GetRequiredService<Views.WarehouseView>());

            // Сборка DI контейнера
            Services = services.BuildServiceProvider();

            try
            {
                using (var scope = Services.CreateScope())
                {
                    // Получаем фабрику контекстов для проверки подключения и миграций
                    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
                    using var db = contextFactory.CreateDbContext();

                    // Проверяем, доступ к серверу БД
                    if (!await db.Database.CanConnectAsync())
                    {
                        throw new Exception("Не удалось установить соединение с сервером PostgreSQL. Проверьте строку подключения и запущен ли сервис базы данных.");
                    }

                    // Накатываем миграции при старте
                    await db.Database.MigrateAsync();

                    // Инициализируем светодиодные ленты
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
                }
            }
            catch (Exception ex)
            {
                // Показываем красивое и информативное окно пользователю
                MessageBox.Show(
                    $"Критическая ошибка при запуске приложения:\n\n{ex.Message}\n\nПриложение будет закрыто.",
                    "Ошибка инициализации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(-1);
                return;
            }

            // Запуск интерфейса
            var window = new Views.MainWindow();
            window.DataContext = Services.GetRequiredService<MainViewModel>();
            window.Show();
        }
    }
}
