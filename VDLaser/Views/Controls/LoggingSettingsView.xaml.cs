using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using VDLaser.ViewModels.Controls;

namespace VDLaser.Views.Controls
{
    /// <summary>
    /// Logique d'interaction pour LoggingSettingsView.xaml
    /// </summary>
    public partial class LoggingSettingsView : UserControl
    {
        public LoggingSettingsView()
        {
            InitializeComponent();
            var serviceProvider = (App.Current as App)?.ServiceProvider;
            if (serviceProvider != null)
            {
                DataContext = serviceProvider.GetRequiredService<LoggingSettingsViewModel>();
            }
        }
    }
}
