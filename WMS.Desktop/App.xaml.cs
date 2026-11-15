using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using WMS.Application;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Desktop.Services;
using WMS.Desktop.ViewModels;
using WMS.Desktop.ViewModels.MenuViewModels;
using WMS.Domain.ExceptionControl;
using WMS.Infrastructure;
using WMS.Infrastructure.Context;


namespace WMS.Desktop
{
    public partial class App : System.Windows.Application, IDesignTimeDbContextFactory<AppDbContext>
    {
        public static IServiceProvider? Services { get; private set; }

        public AppDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("Warehouse"));

            return new AppDbContext(optionsBuilder.Options);
        }

        // Защита от повторного показа окна ошибки, пока предыдущее ещё открыто
        private bool _isShowingError;

        // Запрет второй копии программы: две копии одновременно меняли бы остатки и управляли подсветкой.
        // Мьютекс принадлежит первой копии, пока она работает; Windows освобождает его и при аварийном завершении.
        private const string SingleInstanceMutexName = "WMS.Desktop.SingleInstance";
        private Mutex? _singleInstanceMutex;

        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isFirstInstance);
            if (!isFirstInstance)
            {
                mutex.Dispose();

                if (!ActivateRunningInstance())
                {
                    MessageBox.Show(
                        "Программа уже запущена. Дождитесь, пока откроется её окно.",
                        "Склад",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                Shutdown();
                return;
            }
            _singleInstanceMutex = mutex;

            // Глобальные обработчики ошибок: приложение не должно закрываться из-за необработанного исключения
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            // Весь запуск — в одном try: любая ошибка до появления главного окна должна закрывать программу
            // с понятным сообщением. Иначе её погасит глобальный обработчик, и процесс останется висеть без окон.
            try
            {
                Services = BuildServiceProvider(LoadConfiguration());

                await InitializeDatabaseAndControllersAsync();

                // Запуск интерфейса
                var window = new Views.MainWindow();
                window.DataContext = Services.GetRequiredService<MainViewModel>();
                window.Show();
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
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Освобождаем мьютекс в том же потоке, где он был захвачен при запуске
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();

            base.OnExit(e);
        }

        // Выводит на передний план окно уже запущенной копии. Возвращает false, если окна ещё нет
        // (например, первая копия пока подключается к базе).
        private static bool ActivateRunningInstance()
        {
            try
            {
                var current = Process.GetCurrentProcess();
                var running = Process.GetProcessesByName(current.ProcessName)
                    .FirstOrDefault(p => p.Id != current.Id && p.MainWindowHandle != IntPtr.Zero);

                if (running is null)
                    return false;

                // Если окно свёрнуто, разворачиваем его
                if (IsIconic(running.MainWindowHandle))
                    ShowWindow(running.MainWindowHandle, SwRestore);

                return SetForegroundWindow(running.MainWindowHandle);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private const int SwRestore = 9;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        // Читает appsettings.json рядом с программой. Ошибку в файле (например, лишнюю запятую после правки пароля)
        // переводит в понятное сообщение вместо технического текста парсера.
        private static IConfiguration LoadConfiguration()
        {
            try
            {
                return new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
            }
            catch (FileNotFoundException)
            {
                throw new InvalidOperationException(
                    "Не найден файл настроек appsettings.json. Он должен лежать в папке программы рядом с WMS.Desktop.exe.");
            }
            catch (Exception ex) when (ex is FormatException or InvalidDataException)
            {
                throw new InvalidOperationException(
                    "Файл настроек appsettings.json повреждён: в нём ошибка формата (например, лишняя или пропущенная запятая или кавычка). " +
                    $"Проверьте файл после последней правки.\n\nПодробности: {ex.GetBaseException().Message}");
            }
        }

        private static IServiceProvider BuildServiceProvider(IConfiguration config)
        {
            var services = new ServiceCollection();

            // Логирование
            services.AddLogging(builder =>
            {
                builder.AddDebug();
            });

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
            return services.BuildServiceProvider();
        }

        // Проверка подключения к БД, миграции и инициализация светодиодных лент
        private static async Task InitializeDatabaseAndControllersAsync()
        {
            using var scope = Services!.CreateScope();

            // Получаем фабрику контекстов для проверки подключения и миграций
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = contextFactory.CreateDbContext();

            // Проверяем, доступ к серверу БД
            if (!await db.Database.CanConnectAsync())
            {
                throw new InvalidOperationException("Не удалось установить соединение с сервером PostgreSQL. Проверьте строку подключения и запущен ли сервис базы данных.");
            }

            // Накатываем миграции при старте
            await db.Database.MigrateAsync();

            // Инициализируем светодиодные ленты
            var ledService = scope.ServiceProvider.GetRequiredService<LedStripService>();
            var ledResult = await ledService.InitializeSectorsAsync();

            if (ledResult == ControllersInitResult.Failed)
            {
                MessageBox.Show(
                    "Не удалось инициализировать LED-контроллеры. Проверьте подключение к плате.\n" +
                    "После устранения проблемы нажмите «Перезагрузить контроллеры» в Настройки → Действия.",
                    "Ошибка LED",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else if (ledResult == ControllersInitResult.NotConfigured)
            {
                MessageBox.Show(
                    "В базе данных нет настроек подсветки. Подсветка ячеек работать не будет.",
                    "Подсветка не настроена",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        // Ошибка в потоке интерфейса (команды, обработчики событий, async void). Её можно погасить и продолжить работу.
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            if (_isShowingError)
                return;

            _isShowingError = true;
            try
            {
                if (e.Exception is BusinessException businessEx)
                {
                    MessageBox.Show(businessEx.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(
                        $"Произошла непредвиденная ошибка:\n\n{e.Exception.Message}\n\n" +
                        "Последняя операция могла не выполниться. Проверьте результат перед повтором.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            finally
            {
                _isShowingError = false;
            }
        }

        // Ошибка в фоновой задаче, результат которой никто не дождался (например, Task.Run в поиске)
        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            System.Diagnostics.Debug.WriteLine($"Необработанная ошибка фоновой задачи: {e.Exception}");
        }

        // Ошибка в фоновом потоке. Здесь .NET всё равно завершит процесс, можно только сообщить пользователю.
        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var message = (e.ExceptionObject as Exception)?.Message ?? e.ExceptionObject?.ToString();

            MessageBox.Show(
                $"Критическая ошибка, приложение будет закрыто:\n\n{message}",
                "Критическая ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
