using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Gcode.Parsers;
using VDLaser.Core.Gcode.Services;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Parsers;
using VDLaser.Core.Grbl.Services;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Services;
using VDLaser.Infrastructure.Logging;
using VDLaser.ViewModels.Controls;
using VDLaser.ViewModels.Main;
using VDLaser.ViewModels.Plotter;
using VDLaser.ViewModels.Settings;
using VDLaser.Views.Controls;
using VDLaser.Views.Main;

namespace VDLaser
{
    public partial class App : Application
    {
        private readonly IHost _host;
        public IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            #region Logging Configuration with Serilog
            Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()

    // =========================
    // NORMAL
    // =========================
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(Matching.WithProperty<string>("SourceContext", sc => sc.Contains("CNC")))
        .MinimumLevel.Information()
        .WriteTo.File(
            "logs/vdlaser-normal.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate:
            "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        ))

    // =========================
    // CNC
    // =========================
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt =>
            evt.Level == LogEventLevel.Debug &&
            evt.RenderMessage().Contains("[JOB][CNC]"))
        .WriteTo.File(
            "logs/vdlaser-cnc.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate:
            "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}"
        ))

    // =========================
    // SUPPORT
    // =========================
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt =>
            evt.Level >= LogEventLevel.Warning &&
            evt.RenderMessage().Contains("[JOB]"))
        .WriteTo.File(
            "logs/vdlaser-support.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        ))

    .CreateLogger();
            #endregion

            _host = Host.CreateDefaultBuilder().UseSerilog()
                .ConfigureServices(services =>
                {
                    // ===============================
                    // CORE SERVICES (Singleton)
                    // ===============================
                    services.AddSingleton<ILogService, SerilogLogService>();
                    services.AddSingleton<ISerialPortService, SerialPortService>();
                    services.AddSingleton<IGrblCoreService, GrblCoreService>();
                    services.AddSingleton<IGrblCommandQueue, GrblCommandQueueService>();
                    services.AddSingleton<ISettingService, SettingService>();
                    services.AddSingleton<IGcodeFileService, GcodeFileService>();
                    services.AddSingleton<IGcodeJobService, GcodeJobService>();
                    services.AddSingleton<IGrblCommandQueue, GrblCommandQueueService>();
                    services.AddSingleton<IStatusPollingService, StatusPollingService>();
                    services.AddSingleton<IGcodeParser, GcodeParser>();
                    services.AddSingleton<IGcodeAnalyzer, GcodeAnalyzer>();
                    services.AddSingleton<IDialogService, WpfDialogService>();
                    services.AddSingleton<ILaserStateService, LaserStateService>();
                    // ===============================
                    // PARCERS (Singleton)
                    // ===============================
                    services.AddSingleton<IGrblSubParser, GrblSettingsParser>();
                    services.AddSingleton<IGrblSubParser, GrblStateParser>();
                    services.AddSingleton<IGrblSubParser, GrblInfoParser>();
                    services.AddSingleton<IGrblSubParser, GrblResponseParser>();
                    services.AddSingleton<IConsoleParserService, ConsoleParserService>();
                    services.AddSingleton<GCodePlotterService>();

                    // ===============================
                    // VIEW MODELS (Transient)
                    // ===============================
                    services.AddSingleton<LoggingSettingsViewModel>();
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<MachineStateViewModel>();
                    services.AddSingleton<ConsoleViewModel>();
                    services.AddSingleton<GcodeFileViewModel>();
                    services.AddSingleton<JoggingViewModel>();
                    services.AddSingleton<ControleViewModel>();
                    services.AddSingleton<GrblSettingsViewModel>();
                    services.AddSingleton<SerialPortSettingViewModel>();
                    services.AddSingleton<PlotterViewModel>();
                    services.AddSingleton<GcodeSettingsViewModel>();
                    services.AddSingleton<SoftwareSettingViewModel>();


                    // ===============================
                    // VIEWS (Transient)
                    // ===============================
                    services.AddTransient<SerialPortSettingView>();
                    services.AddTransient<PlotterView>();
                    services.AddTransient<MachineStateView>();
                    services.AddSingleton<MainWindow>();
                    
                })
                .Build();
            ServiceProvider = _host.Services; 
        }
        public void SwitchLanguage(string lang)
        {
            var dict = new ResourceDictionary();
            dict.Source = new Uri($"Resources/Languages/Strings.{lang}.xaml", UriKind.Relative);

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // Resolve MainWindow via DI
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();

            // Inject MainWindowViewModel via DI
            mainWindow.DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();

            mainWindow.Show();
            base.OnStartup(e);

            

            Log.Debug("VDLaser démarré - Logs configurés pour Debug et au-dessus.");
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}