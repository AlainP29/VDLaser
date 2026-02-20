using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using VDLaser.Core.Gcode.Services;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;
using VDLaser.ViewModels.Controls;
using static VDLaser.Core.Grbl.Services.GrblCommandQueueService;
using static VDLaser.ViewModels.Controls.GcodeFileViewModel;

namespace VDLaser.ViewModels.Plotter
{
    /// <summary>
    /// ViewModel responsible for rendering the G-code toolpath and machine state.
    /// Manages coordinate transformations, laser head visualization, and UI axis labels.
    /// </summary>
    public partial class PlotterViewModel : ViewModelBase
    {
        #region Fields and Services
        private readonly GCodePlotterService _plotterService;
        private readonly IGrblCoreService _grblService;
        private readonly ILogService _log;
        private readonly ILaserStateService _laserStateService;

        private bool _isUpdatingToolPath = false;
        private System.Timers.Timer? _debounceTimer;
        #endregion

        #region Properties
        public ViewportState Viewport { get; } = new();
        public ViewportController ViewportController { get; }

        [ObservableProperty]
        private double _canvasWidth = 400;

        [ObservableProperty]
        private double _canvasHeight = 300;
        [ObservableProperty]
        private GcodeFileViewModel _fileViewModel;

        [ObservableProperty]
        private PathGeometry? _toolPathGeometry;
        [ObservableProperty]
        private RectangleGeometry? _boundingBoxGeometry;
        [ObservableProperty]
        private bool _isHovered = false;
        [ObservableProperty]
        private ObservableCollection<string> _xAxisLabels = new();
        
        [ObservableProperty]
        private ObservableCollection<string> _yAxisLabels = new();
        [ObservableProperty]
        private Rect? _toolPathBounds;
        [ObservableProperty] private string _currentMachinePosition = "X:0 Y:0";

        [ObservableProperty]
        private bool _isSimulating;
        [ObservableProperty]
        private double _simulationSpeed = 1.0; // Par défaut vitesse x1

        // Propriété pour la position réelle (utilisée dans la vue pour positionner l'élément)
        [ObservableProperty]
        private Point _machinePositionPoint = new Point(0, 0);
        [ObservableProperty]
        private int _machinePositionPointSize = 6;
        [ObservableProperty]
        private Point? _highlightPoint;
        
        [ObservableProperty]
        private PathGeometry? _rapidPathGeometry; // Gris : rapides/sans gravure

        [ObservableProperty]
        private PathGeometry? _engravePathGeometry; // Bleu : gravure active
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LaserHeadBrush))]
        private int _laserPower;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LaserHeadBrush))]
        private int _maxLaserPower=2500;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LaserHeadBrush))]
        private bool _isLaserOn;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LaserHeadBrush))]
        private LaserMode _laserMode;
        #endregion


        public PlotterViewModel(GCodePlotterService plotterService, GcodeFileViewModel fileViewModel, IGrblCoreService grblService, ILogService log, ILaserStateService laserStateService)
        {
            _plotterService = plotterService;
            _fileViewModel = fileViewModel;
            _grblService = grblService;
            _log = log;
            _laserStateService = laserStateService;

            Viewport = new ViewportState();
            ViewportController = new ViewportController(Viewport,_log);

            InitializeSubscriptions();
            GenerateDefaultAxisLabels();
            _log.Information("[Plotter] ViewModel successfully initialized.");
        }

        private void InitializeSubscriptions()
        {
            FileViewModel.PropertyChanged += OnFileViewModelPropertyChanged;
            _grblService.StatusUpdated += OnGrblStatusUpdated;
            _debounceTimer = new System.Timers.Timer(100); // 100ms debounce
            _debounceTimer.Elapsed += (s, e) =>
            {
                _debounceTimer.Stop();
                UpdateToolPathInternal();
            };

            _laserStateService.LaserPowerChanged += OnLaserPowerChangedEvent;
            _laserStateService.LaserStateChanged += OnLaserStateChangedEvent;
            _laserStateService.LaserModeChanged += OnLaserModeChangedEvent;

            WeakReferenceMessenger.Default.Register<GcodeLineSelectedMessage>(this, (r, m) =>
            {
                if (m.X.HasValue && m.Y.HasValue && FileViewModel.Stats != null)
                {
                    Point newPoint = new Point(
                        m.X.Value - FileViewModel.Stats.MinX,
                        m.Y.Value - FileViewModel.Stats.MinY
                    );

                    HighlightPoint = newPoint;
                }
                else
                {
                    HighlightPoint = null;
                }
            });
           
        }

        #region Event Handlers
        private void OnGrblStatusUpdated(object? sender, EventArgs e)
        {
            var minX = FileViewModel.Stats?.MinX ?? 0;
            var minY = FileViewModel.Stats?.MinY ?? 0;
            CurrentMachinePosition = $"X:{_grblService.State.MachinePosX:F3} Y:{_grblService.State.MachinePosY:F3}";

            if (FileViewModel.Stats != null)
            {
                double x = _grblService.State.WorkPosX - FileViewModel.Stats.MinX;
                double y = _grblService.State.WorkPosY - FileViewModel.Stats.MinY;
                MachinePositionPoint = new Point(x, y);
                
            }
            else
            {
                MachinePositionPoint = new Point(
                    _grblService.State.WorkPosX - minX,
                    _grblService.State.WorkPosY - minY);
            }
            IsLaserOn = LaserPower > 0 && LaserMode != LaserMode.Off;
            _log.Debug("[Plotter] [ViewModel Status] Raw PowerLaser from GrblState: {PowerLaser}", _grblService.State.PowerLaser);
            if (int.TryParse(
                    _grblService.State.PowerLaser?.Replace("S:", "").Trim(),
                    out int power))
            {
                _log.Debug("[Plotter] [ViewModel Parse Success] Parsed LaserPower: {Power}", power);
                LaserPower = power;
            }
            else
            {
                _log.Warning("[Plotter] [ViewModel Parse Fail] Could not parse PowerLaser: {PowerLaser}. Defaulting to 0.", _grblService.State.PowerLaser);
                LaserPower = 0;
            }
            _log.Debug("[Plotter] Status update: Power={power}, On={on}, Mode={mode}", LaserPower,IsLaserOn,LaserMode);
        }
        private void OnFileViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GcodeFileViewModel.GcodeLines))
            {
                _log.Debug("[Plotter] GcodeLines changed, triggering debounced update.");
                _debounceTimer?.Stop();
                _debounceTimer?.Start(); // Relance le timer pour debounce
            }
        }
        private void OnLaserPowerChangedEvent(int p) => LaserPower = p;
        private void OnLaserStateChangedEvent(bool on) => IsLaserOn = on;
        private void OnLaserModeChangedEvent(LaserMode mode) => LaserMode = mode;
        #endregion

        #region Tool Path Update Logic
        /// <summary>
        /// updates the tool path geometry and related properties with error handling and reentrancy protection.
        /// </summary>
        private void UpdateToolPathInternal()
        {
            if (_isUpdatingToolPath) return;

            _isUpdatingToolPath = true;
            try
            {
                UpdateToolPath();
            }
            catch (Exception ex)
            {
                _log.Error("[Plotter] Error updating tool path: {Message}", ex.Message);
            }
            finally
            {
                _isUpdatingToolPath = false;
            }
        }
        /// <summary>
        /// Updates the tool path geometry (2 paths) and related properties based on the current G-code lines in the file view
        /// model.
        /// </summary>
        /// <remarks>This method recalculates the tool path, bounding box, and canvas dimensions using the
        /// latest G-code data.  If there are no G-code lines available, the tool path and bounding box properties are
        /// reset to their default empty states.</remarks>
        private void UpdateToolPath()
        {
            IsSimulating = false;
            if (FileViewModel.GcodeLines == null || FileViewModel.GcodeLines.Count == 0)
            {
                ResetGeometries();
                return;
            }

            var commands = FileViewModel.GcodeLines.Select(item => item.Command).ToList();

            var (rapidGeo, engraveGeo) = _plotterService.BuildGeometriesFromCommands(commands,
                FileViewModel.Stats.MinX,
                FileViewModel.Stats.MinY);

            rapidGeo.Freeze();
            engraveGeo.Freeze();

            RapidPathGeometry = rapidGeo;
            EngravePathGeometry = engraveGeo;

            var combinedGeo = new GeometryGroup();
            combinedGeo.Children.Add(rapidGeo);
            combinedGeo.Children.Add(engraveGeo);
            ToolPathBounds = combinedGeo.Bounds;

            BoundingBoxGeometry = new RectangleGeometry(ToolPathBounds.Value);
            BoundingBoxGeometry.Freeze();

            GenerateAxisLabels();
            _log.Information("[Plotter] Toolpath updated with {Count} commands.", commands.Count);
        }

        private void ResetGeometries()
        {
            RapidPathGeometry = null;
            EngravePathGeometry = null;
            ToolPathBounds = Rect.Empty;
            BoundingBoxGeometry = null;
        }
        #endregion

        #region Commands
        [RelayCommand]
            private async Task SetOriginAsync()
            {
            if (!_grblService.IsConnected) return;

                try
                {
                    _log.Information("[Plotter] Setting machine work origin (G10 L20 X0 Y0).");
                    await _grblService.SendCommandAsync("G10 L20 P1 X0 Y0");
                    await Task.Delay(500);
                    await _grblService.SendCommandAsync("?");

                    MachinePositionPoint = new Point(0, 0);
                }
            catch (Exception ex) 
            { 
                _log.Error("[Plotter] SetOrigin command failed."); 
            }
        }
        [RelayCommand]
        public void AutoCenter()
        {
            RequestAutoCenter();
            GenerateAxisLabels();
        }


        [RelayCommand]
        public void ToggleSimulation()
        {
            IsSimulating = !IsSimulating;
        }
        public event Action OnRequestAutoCenter;

        public event Action? OnRequestResetView;
        private void RequestAutoCenter() => OnRequestAutoCenter?.Invoke();
        private void RequestResetView() => OnRequestResetView?.Invoke();

        [RelayCommand]
        public void ResetView()
        {
            OnRequestResetView?.Invoke();
            GenerateDefaultAxisLabels();
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Generates and updates the axis label collections for the X and Y axes based on the current statistics in the
        /// associated file view model.
        /// </summary>
        private void GenerateAxisLabels()
        {
            if (FileViewModel.Stats == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                XAxisLabels.Clear();
                YAxisLabels.Clear();

                double minX = FileViewModel.Stats.MinX;
                double maxX = FileViewModel.Stats.MaxX;
                double stepX = (maxX - minX) / 4;

                for (int i = 0; i <= 4; i++)
                {
                    double val = double.Round(minX + (stepX * i));
                    XAxisLabels.Add(val.ToString("F0"));
                }

                double minY = FileViewModel.Stats.MinY;
                double maxY = FileViewModel.Stats.MaxY;
                double stepY = (maxY - minY) / 4;

                for (int i = 4; i >= 0; i--)
                {
                    double val = double.Round(minY + (stepY * i));
                    YAxisLabels.Add(val.ToString("F0"));
                }
                XAxisLabels.Remove("0");
                XAxisLabels.Remove("0");
            });
        }
        public void GenerateDefaultAxisLabels()
        {
            XAxisLabels.Clear();
            YAxisLabels.Clear();
            for (int i = 50; i <= 400; i += 50)
            {
                XAxisLabels.Add(i.ToString("F0"));
            }
            for (int i = 300; i > 0; i -= 50)
            {
                YAxisLabels.Add(i.ToString("F0"));
            }
        }

        /// <summary>
        /// Helper to determine the visual brush of the laser head based on power and mode.
        /// </summary>
        public Brush LaserHeadBrush
        {
            get
            {
                if (!IsLaserOn)
                    return Brushes.Gray;

                return LaserMode switch
                {
                    LaserMode.Constant => CreateBlueBrush(),
                    LaserMode.Dynamic => CreateOrangeBrush(),
                    _ => Brushes.Gray
                };
            }

        }

        private Brush CreateBlueBrush()
        {
            if (MaxLaserPower <= 0)
                return Brushes.DodgerBlue;

            byte intensity = (byte)Math.Clamp(
                LaserPower * 255 / MaxLaserPower,
                60, 255);

            return new SolidColorBrush(Color.FromRgb(0, intensity, 255));
        }

        private Brush CreateOrangeBrush()
        {
            return new SolidColorBrush(Color.FromRgb(255, 165, 0));
        }
        #endregion

        #region Disposal
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FileViewModel.PropertyChanged -= OnFileViewModelPropertyChanged;
                _grblService.StatusUpdated -= OnGrblStatusUpdated;
                _debounceTimer?.Stop();
                _debounceTimer?.Dispose();
                _laserStateService.LaserPowerChanged -= OnLaserPowerChangedEvent;
                _laserStateService.LaserStateChanged -= OnLaserStateChangedEvent;
                _laserStateService.LaserModeChanged -= OnLaserModeChangedEvent;
                WeakReferenceMessenger.Default.Unregister<GcodeLineSelectedMessage>(this);
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}