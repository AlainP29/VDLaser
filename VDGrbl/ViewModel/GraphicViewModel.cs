using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using VDLaser.Model;
using VDLaser.Service;
using VDLaser.Tools;

namespace VDLaser.ViewModel
{
    public class GraphicViewModel : ViewModelBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IGraphicService _graphicService;
        private string _groupBoxGraphicTitle;
        private PathGeometry _pathGeometry;
        private Brush _fill = Brushes.AliceBlue;
        private Brush _stroke = Brushes.White;
        private double _strokeThickness = 2;
        private PointCollection _gcodePoints = new PointCollection();
        private ObservableCollection<GraphicItems> _paths = new ObservableCollection<GraphicItems>();

        #region RelayCommand
        public RelayCommand TestGraphicCommand { get; private set; }
        private void GraphicRelayCommands()
        {
            TestGraphicCommand = new RelayCommand(TestGraphic, CanExecuteTestGraphicCommand);
        }
        #endregion

        #region Messenger
        /// <summary>
        /// Used to communicate between ViewModels: MainViewModel
        /// </summary>
        public void SendGraphicMessage()
        {
            MessengerInstance.Send<NotificationMessage>(new NotificationMessage("Draw"));
            logger.Debug("GraphicViewModel|Notification sent");
        }
        #endregion

        #region Property
        /// <summary>
        /// Get the GroupBoxGraphicTitle property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public string GroupBoxGraphicTitle
        {
            get
            {
                return _groupBoxGraphicTitle;
            }
            set
            {
                Set(ref _groupBoxGraphicTitle, value);
            }
        }
        /// <summary>
        /// Get the GCodePoints property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PointCollection GCodePoints
        {
            get
            {
                return _gcodePoints;
            }
            set
            {
                Set(ref _gcodePoints, value);
            }
        }
        /// <summary>
        /// Get the Geometry property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PathGeometry PathGeometry
        {
            get
            {
                return _pathGeometry;
            }
            set
            {
                Set(ref _pathGeometry, value);
            }
        }
        /// <summary>
        /// Get the Fill property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Brush Fill
        {
            get
            {
                return _fill;
            }
            set
            {
                Set(ref _fill, value);
                logger.Info("MainViewModel|Image Fill: {0}", value);
            }
        }
        /// <summary>
        /// Get the Stroke property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Brush Stroke
        {
            get
            {
                return _stroke;
            }
            set
            {
                Set(ref _stroke, value);
                logger.Info("MainViewModel|Image Stroke: {0}", value);
            }
        }
        /// <summary>
        /// Get the StrokeThickness property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double StrokeThickness
        {
            get
            {
                return _strokeThickness;
            }
            set
            {
                Set(ref _strokeThickness, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Image StrokeThickness: {0}", value);

            }
        }
        /// <summary>
        /// Get the Paths property. 
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<GraphicItems> GcodePaths
        {
            get
            {
                return _paths;
            }
            set
            {
                Set(ref _paths, value);
            }
        }
#endregion

        #region Constructor
        public GraphicViewModel(IGraphicService graphicService)
        {
            _graphicService = graphicService;
            if (_graphicService != null)
            {
                _graphicService.GetGraphic(
                           (item, error) =>
                           {
                               if (error != null)
                               {
                                   logger.Error("GraphicViewModel|Exception GetGraphic raised: " + error);
                                   return;
                               }
                               logger.Info("GraphicViewModel|Load Graphic window");
                               GroupBoxGraphicTitle = item.GraphicHeader;
                           });
                GraphicRelayCommands();
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Draw G-code file with a path.
        /// </summary>
        public void GCodeDrawing(PointCollection pc)
        {
            GraphicTool graphicTool = new GraphicTool(pc);

            logger.Info("GraphicViewModel|GrblTest Geometry");

            GcodePaths.Add(new GraphicItems
            {
                GraphicPathGeometry = graphicTool.Plotter(),
                GraphicFill = Fill,
                GraphicStroke = Brushes.Red,
                GraphicStrokeThickness = StrokeThickness,
            });
        }
        /// <summary>
        /// Sends 'draw' command.
        /// </summary>
        public void TestGraphic()
        {
            logger.Debug("GraphicViewModel|Send notification");
            SendGraphicMessage();
        }
        /// <summary>
        /// Allows/disallows TestGraphic.
        /// </summary>
        /// <returns></returns>
        public static bool CanExecuteTestGraphicCommand()
        {
            return true;
        }
        #endregion
    }
}
