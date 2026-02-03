using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using VDLaser.ViewModels.Controls;

namespace VDLaser.Views.Controls
{
    /// <summary>
    /// Logique d'interaction pour UserControl1.xaml
    /// </summary>
    public partial class GcodeSettingsView : UserControl
    {
        public GcodeSettingsView()
        {
            InitializeComponent();
            var serviceProvider = (App.Current as App)?.ServiceProvider;
            if (serviceProvider != null)
            {
                DataContext = serviceProvider.GetRequiredService<GcodeSettingsViewModel>();
            }
        }
    }
}
