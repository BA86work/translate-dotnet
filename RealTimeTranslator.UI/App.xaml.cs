using Microsoft.Extensions.DependencyInjection;
using RealTimeTranslator.UI.Views;
using System.Windows;
using Microsoft.Extensions.Configuration;
using RealTimeTranslator.Data;
using RealTimeTranslator.Services.Interfaces;
using RealTimeTranslator.Services.Implementations;
using RealTimeTranslator.UI.ViewModels;
using System.IO;
using Microsoft.Extensions.Logging;
using RealTimeTranslator.Core.Updates;
using System.Reflection;
using RealTimeTranslator.Core.Monitoring;
using RealTimeTranslator.Core.Analytics;
using RealTimeTranslator.Core.Backup;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Serilog;
using RealTimeTranslator.Core.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace RealTimeTranslator.UI
{
    public partial class App : Application
    {
        private readonly ILogger<App> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly UpdateService _updateService;
        private readonly IConfiguration _configuration;

        public App()
        {
            try
            {
                File.AppendAllText("startup.log", $"{DateTime.Now}: Starting application configuration...\n");
                
                // Get the executable's directory
                var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var exeDir = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;
                File.AppendAllText("startup.log", $"{DateTime.Now}: Executable directory: {exeDir}\n");

                // Configuration
                var builder = new ConfigurationBuilder()
                    .SetBasePath(exeDir)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                File.AppendAllText("startup.log", $"{DateTime.Now}: Loading configuration...\n");
                _configuration = builder.Build();

                var services = new ServiceCollection();
                File.AppendAllText("startup.log", $"{DateTime.Now}: Configuring services...\n");
                ConfigureServices(services);
                
                File.AppendAllText("startup.log", $"{DateTime.Now}: Building service provider...\n");
                _serviceProvider = services.BuildServiceProvider();

                File.AppendAllText("startup.log", $"{DateTime.Now}: Getting logger and update service...\n");
                _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
                _updateService = _serviceProvider.GetRequiredService<UpdateService>();
                
                File.AppendAllText("startup.log", $"{DateTime.Now}: Application configuration completed\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText("startup.log", $"{DateTime.Now}: CRITICAL ERROR in constructor: {ex.Message}\n{ex.StackTrace}\n");
                throw;
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Logging
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RealTimeTranslator",
                "logs",
                "app.log");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(Log.Logger);
            });
            
            services.AddSingleton<IConfiguration>(_configuration);

            // Database
            services.AddDbContext<TranslatorDbContext>(options =>
                options.UseSqlite(_configuration.GetConnectionString("Database")));

            // Services
            services.Configure<AzureTranslatorConfig>(_configuration.GetSection("AzureTranslator"));
            services.AddSingleton<ITranslationService, AzureTranslationService>();
            services.AddSingleton<IOcrService, TesseractOcrService>();
            services.AddSingleton<IScreenCaptureService, WindowsScreenCaptureService>();

            // Update Service
            services.AddSingleton<HttpClient>();
            services.AddSingleton<UpdateService>(sp => new UpdateService(
                sp.GetRequiredService<HttpClient>(),
                sp.GetRequiredService<ILogger<UpdateService>>(),
                _configuration.GetValue<string>("UpdateService:Url") ?? "https://api.example.com/updates"
            ));

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Views
            services.AddSingleton<MainWindow>();

            // Performance Monitoring
            services.AddSingleton<PerformanceMonitor>();

            // Analytics
            services.AddSingleton<AnalyticsService>();

            // Backup Service
            var backupPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RealTimeTranslator",
                "backups");
            services.AddSingleton<BackupService>();

            // Settings Service
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RealTimeTranslator",
                "settings.json");
            services.AddSingleton<SettingsService>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                _logger.LogInformation("Application starting...");
                base.OnStartup(e);

                try
                {
                    // Only check for updates if enabled in config
                    if (_configuration.GetValue<bool>("UpdateService:Enabled"))
                    {
                        _logger.LogInformation("Update checking is enabled, checking for updates...");
                        try 
                        {
                            // Add timeout for update check
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                            var updateInfo = await _updateService.CheckForUpdatesAsync()
                                .WaitAsync(TimeSpan.FromSeconds(5), cts.Token);
                            
                            if (updateInfo != null)
                            {
                                _logger.LogInformation("Update available: {Version}", updateInfo.Version);
                                var result = MessageBox.Show(
                                    $"New version {updateInfo.Version} is available. Would you like to update?",
                                    "Update Available",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Information);

                                if (result == MessageBoxResult.Yes)
                                {
                                    _logger.LogInformation("Starting update process...");
                                    await _updateService.DownloadAndInstallUpdateAsync(updateInfo)
                                        .WaitAsync(TimeSpan.FromSeconds(30), CancellationToken.None);
                                    Shutdown();
                                    return;
                                }
                            }
                        }
                        catch (TimeoutException tex)
                        {
                            _logger.LogWarning(tex, "Update check timed out, continuing startup");
                        }
                        catch (Exception updateEx)
                        {
                            _logger.LogWarning(updateEx, "Failed to check for updates, continuing startup");
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Update checking is disabled, skipping...");
                    }

                    _logger.LogInformation("Creating main window...");
                    // Show main window
                    var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    mainWindow.DataContext = mainViewModel;
                    _logger.LogInformation("Showing main window...");
                    mainWindow.Show();
                    Current.MainWindow = mainWindow; // Set the main window explicitly
                    _logger.LogInformation("Main window shown successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during startup");
                    var errorMessage = $"An error occurred during startup: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                    _logger.LogError("Error details: {ErrorMessage}", errorMessage);
                    MessageBox.Show(errorMessage, 
                        "Startup Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                var criticalError = $"Critical error during startup: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                // Can't use logger here as it might be the source of the error
                File.WriteAllText("critical_error.log", criticalError);
                MessageBox.Show(criticalError,
                    "Critical Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
} 