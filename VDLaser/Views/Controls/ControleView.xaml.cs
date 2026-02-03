using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using VDLaser.ViewModels.Controls;

namespace VDLaser.Views.Controls
{
    /// <summary>
    /// Logique d'interaction pour ControleView.xaml
    /// </summary>
    public partial class ControleView : UserControl
    {
        public static readonly DependencyProperty ShowControlsProperty =
        DependencyProperty.Register("ShowControls", typeof(bool), typeof(ControleView), new PropertyMetadata(true));

        public bool ShowControls
        {
            get { return (bool)GetValue(ShowControlsProperty); }
            set { SetValue(ShowControlsProperty, value); }
        }
        public ControleView()
        {
            InitializeComponent();
            var serviceProvider = (App.Current as App)?.ServiceProvider;
            if (serviceProvider != null)
            {
                DataContext = serviceProvider.GetRequiredService<ControleViewModel>();
            }
        }
    }
}
