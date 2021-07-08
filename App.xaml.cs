using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VDLaser.View;
using VDLaser.Service;
using VDLaser.ViewModel;
using VDLaser.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace VDLaser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Add Host builder, DI and Service Provider
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost host;
        public static IServiceProvider ServiceProvider { get; set; }

        public App()
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => { ConfigureServices(context.Configuration, services); })
                .ConfigureLogging(logBuilder =>
                {
                    logBuilder.ClearProviders();
                    logBuilder.SetMinimumLevel(LogLevel.Information);
                })
                .UseNLog()
                .Build();
            ServiceProvider = host.Services;
        }

        private void ConfigureServices(IConfiguration configuration,
        IServiceCollection services)
        {
            services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
            services.AddScoped<IDataService, DataService>();
            services.AddScoped<IGraphicService, GraphicService>();
            services.AddScoped<IGrblService, GrblService>();
            services.AddScoped<ISettingService, SettingService>();

            //Register all View Models
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<GraphicViewModel>();
            services.AddSingleton<GrblViewModel>();
            services.AddSingleton<SettingViewModel>();

            //Register all Windows of the application.
            services.AddTransient<MainWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await host.StartAsync();

            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (host)
            {
                await host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
        }
    }
}
