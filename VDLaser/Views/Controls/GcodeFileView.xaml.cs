using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using VDLaser.ViewModels.Controls;

namespace VDLaser.Views.Controls
{
    /// <summary>
    /// Logique d'interaction pour GCodeView.xaml
    /// </summary>
    public partial class GcodeFileView : UserControl
    {
        public GcodeFileView()
        {
            InitializeComponent();

            var serviceProvider = (App.Current as App)?.ServiceProvider;
            if (serviceProvider != null)
            {
                DataContext = serviceProvider.GetRequiredService<GcodeFileViewModel>();
            }

            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

}
