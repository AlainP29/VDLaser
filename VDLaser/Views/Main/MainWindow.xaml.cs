using System.ComponentModel;
using System.Windows;
using VDLaser.ViewModels.Controls;
using VDLaser.ViewModels.Main;

namespace VDLaser.Views.Main
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.ConnectionError += (_, msg) =>
            {
                MessageBox.Show(msg, "Erreur de connexion",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            };
            }
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
        }

        // Événement Loaded : Initialisations post-chargement de la fenêtre
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SerialPortSettingVM.RefreshPortsCommand.Execute(null);
            }
        }

        // Événement Closing : Gestion propre de la fermeture (déconnexion GRBL)
        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel && viewModel.IsConnected)
            {
                await viewModel.DisconnectCommand.ExecuteAsync(null);
            }
        }
        

}
}
