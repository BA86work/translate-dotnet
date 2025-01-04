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
            // Configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
            _updateService = _serviceProvider.GetRequiredService<UpdateService>();
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
                "https://api.example.com/updates"  // URL สำหรับตรวจสอบอัพเดท
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during startup");
            }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var updateInfo = await _updateService.CheckForUpdatesAsync();
                if (updateInfo != null)
                {
                    var result = MessageBox.Show(
                        $"New version {updateInfo.Version} is available. Would you like to update?",
                        "Update Available",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _updateService.DownloadAndInstallUpdateAsync(updateInfo);
                        return;
                    }
                }

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during startup");
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
        }
    }
} 