using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using VDLaser.ViewModels.Base;
using VDLaser.ViewModels.Controls;

namespace VDLaser.Views.Controls
{
    /// <summary>
    /// Logique d'interaction pour ReceiveData.xaml
    /// </summary>
    public partial class ConsoleView : UserControl
    {
        public static readonly DependencyProperty ShowControlsProperty =
        DependencyProperty.Register("ShowControls", typeof(bool), typeof(ConsoleView), new PropertyMetadata(true));

        public bool ShowControls
        {
            get { return (bool)GetValue(ShowControlsProperty); }
            set { SetValue(ShowControlsProperty, value); }
        }
        public ConsoleView()
        {
            InitializeComponent();
            var serviceProvider = (App.Current as App)?.ServiceProvider;
            if (serviceProvider != null)
            {
                DataContext = serviceProvider.GetRequiredService<ConsoleViewModel>();
            }
        }
    }
}
