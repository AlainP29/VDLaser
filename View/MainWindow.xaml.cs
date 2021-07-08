using System.Windows;

namespace VDLaser.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }
        /*
        #region
        //: implement for DI should move to viewmodel?
        private readonly IDataService dataService;
        private readonly AppSettings settings;

        public MainWindow(IDataService dataService,
                          IOptions<AppSettings> settings)
        {
            InitializeComponent();

            this.dataService = dataService;
            this.settings = settings.Value;
        }
        #endregion*/
    }
}
