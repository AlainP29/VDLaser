using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using VDLaser.ViewModels.Controls;

namespace VDLaser.Views.Controls
{
    /// <summary>
    /// Logique d'interaction pour SerialPortSettingView.xaml
    /// </summary>
    public partial class SerialPortSettingView : UserControl
    {
        public SerialPortSettingView()
        {
            InitializeComponent();
            var serviceProvider = (App.Current as App)?.ServiceProvider;
            if (serviceProvider != null)
            {
                DataContext = serviceProvider.GetRequiredService<SerialPortSettingViewModel>();
            }
        }
    }
}
