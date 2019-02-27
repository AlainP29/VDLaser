using System.Windows;
using VDGrbl.ViewModel;

namespace VDGrbl.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }

        private void LoadSettingsView_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}