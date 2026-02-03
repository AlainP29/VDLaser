using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VDLaser.Core.Gcode.Services;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;
using VDLaser.ViewModels.Controls;
using static VDLaser.Core.Grbl.Services.GrblCommandQueueService;
using static VDLaser.ViewModels.Controls.GcodeFileViewModel;

namespace VDLaser.ViewModels.Plotter
{
    public partial class PlotterViewModel : ViewModelBase
    {
        private readonly GCodePlotterService _plotterService;
        private readonly IGrblCoreService _grblService;
        private readonly ILogService _log;
        private readonly ILaserStateService _laserStateService;

        private bool _isUpdatingToolPath = false;
        private System.Timers.Timer? _debounceTimer;
        public ViewportState Viewport { get; } = new();

        public ViewportController ViewportController { get; }

        [ObservableProperty]
        private double _canvasWidth = 250;

        [ObservableProperty]
        private double _canvasHeight = 250;
        [ObservableProperty]
        private GcodeFileViewModel _fileViewModel;

        [ObservableProperty]
        private PathGeometry? _toolPathGeometry;
        [ObservableProperty]
        private RectangleGeometry? _boundingBoxGeometry;
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
        private int _machinePositionPointSize = 5;
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

        public PlotterViewModel(GCodePlotterService plotterService, GcodeFileViewModel fileViewModel, IGrblCoreService grblService, ILogService log, ILaserStateService laserStateService)
        {
            _plotterService = plotterService;
            _fileViewModel = fileViewModel;
            _grblService = grblService;
            _log = log;
            _laserStateService = laserStateService;
            Viewport = new ViewportState();
            ViewportController = new ViewportController(Viewport);

            // Abonnement aux événements
            _fileViewModel.PropertyChanged += OnFileViewModelPropertyChanged;
            _grblService.StatusUpdated += OnGrblStatusUpdated;
            _debounceTimer = new System.Timers.Timer(100); // 100ms debounce
            _debounceTimer.Elapsed += (s, e) =>
            {
                _debounceTimer.Stop();
                UpdateToolPathInternal();
            };
            WeakReferenceMessenger.Default.Register<GcodeLineSelectedMessage>(this, (r, m) =>
            {
                if (m.X.HasValue && m.Y.HasValue && FileViewModel.Stats != null)
                {
                    HighlightPoint = new Point(
            m.X.Value - FileViewModel.Stats.MinX,
            m.Y.Value - FileViewModel.Stats.MinY
            );
                }
                else
                {
                    HighlightPoint = null;
                }
            });
            _laserStateService.LaserPowerChanged += p => LaserPower = p;
            _laserStateService.LaserStateChanged += on => IsLaserOn = on;
            _laserStateService.LaserModeChanged += mode => LaserMode = mode;

        }
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
            _log.Debug("[PlotterViewModel ViewModel Status] Raw PowerLaser from GrblState: {PowerLaser}", _grblService.State.PowerLaser);
            if (int.TryParse(
                    _grblService.State.PowerLaser?.Replace("S:", "").Trim(),
                    out int power))
            {
                _log.Debug("[PlotterViewModel ViewModel Parse Success] Parsed LaserPower: {Power}", power);
                LaserPower = power;
            }
            else
            {
                _log.Warning("[PlotterViewModel ViewModel Parse Fail] Could not parse PowerLaser: {PowerLaser}. Defaulting to 0.", _grblService.State.PowerLaser);
                LaserPower = 0;
            }
            _log.Debug("Status update: Power={power}, On={on}, Mode={mode}",LaserPower,IsLaserOn,LaserMode);
        }

        private void OnFileViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GcodeFileViewModel.GcodeLines))
            {
                _log.Information("[PlotterViewModel] PropertyChanged détecté pour GcodeLines - Début debounce");
                _debounceTimer?.Stop();
                _debounceTimer?.Start(); // Relance le timer pour debounce
            }
        }
        private void UpdateToolPathInternal()
        {
            if (_isUpdatingToolPath)
            {
                _log.Warning("[PlotterViewModel] Appel UpdateToolPath ignoré (déjà en cours)");
                return;
            }
            _isUpdatingToolPath = true;
            try
            {
                _log.Information("[PlotterViewModel] Exécution unique de UpdateToolPath après debounce");
                UpdateToolPath();
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
            _log.Information("[PlotterViewModel] Début de UpdateToolPath() - GcodeLines.Count: {Count}", FileViewModel.GcodeLines?.Count ?? 0);
            IsSimulating = false;
            if (FileViewModel.GcodeLines == null || FileViewModel.GcodeLines.Count == 0)
            {
                _log.Warning("[PlotterView] Pas de géométrie engrave pour simulation");
                RapidPathGeometry = null;
                EngravePathGeometry = null;
                ToolPathBounds = Rect.Empty;
                BoundingBoxGeometry = null;
                return;
            }

            var commands = FileViewModel.GcodeLines.Select(item => item.Command).ToList();

            // Appel au service modifié
            var (rapidGeo, engraveGeo) = _plotterService.BuildGeometriesFromCommands(commands,
                FileViewModel.Stats.MinX,
                FileViewModel.Stats.MinY);

            CanvasWidth = FileViewModel.Stats.Width;
            CanvasHeight = FileViewModel.Stats.Height;

            rapidGeo.Freeze();
            engraveGeo.Freeze();

            RapidPathGeometry = rapidGeo;
            EngravePathGeometry = engraveGeo;
            _log.Information("[PlotterViewModel] Géométries | Rapid: {RapidFigs}/{RapidSegs} | Engrave: {EngraveFigs}/{EngraveSegs}",
                rapidGeo.Figures.Count, rapidGeo.Figures.Sum(f => f.Segments.Count),
                engraveGeo.Figures.Count, engraveGeo.Figures.Sum(f => f.Segments.Count));

            foreach(var fig in engraveGeo.Figures)
            {
                _log.Debug("[PlotterViewModel] Engrave Figure Start: {Start} Segments: {SegCount}", fig.StartPoint, fig.Segments.Count);
            }
            foreach (var fig in rapidGeo.Figures)
            {
                _log.Debug("[PlotterViewModel] rapid Figure Start: {Start} Segments: {SegCount}", fig.StartPoint, fig.Segments.Count);
            }
            // Calcul des bounds combinés
            var combinedGeo = new GeometryGroup();
            combinedGeo.Children.Add(rapidGeo);
            combinedGeo.Children.Add(engraveGeo);
            var bounds = combinedGeo.Bounds;
            ToolPathBounds = bounds;

            var bboxGeometry = new RectangleGeometry(bounds);
            bboxGeometry.Freeze();
            BoundingBoxGeometry = bboxGeometry;
            MachinePositionPointSize = (int)(Math.Max(1, Math.Min(bboxGeometry.Rect.Width, bboxGeometry.Rect.Height) * 0.05));

            GenerateAxisLabels();
        }
        /// <summary>
        /// Updates the tool path geometry (1 path) and related properties based on the current G-code lines in the file view
        /// model.
        /// </summary>
        /// <remarks>This method recalculates the tool path, bounding box, and canvas dimensions using the
        /// latest G-code data.  If there are no G-code lines available, the tool path and bounding box properties are
        /// reset to their default empty states.</remarks>
        private void UpdateToolOnePath()
        {
            IsSimulating = false;
            if (FileViewModel.GcodeLines == null || FileViewModel.GcodeLines.Count == 0)
            {
                ToolPathGeometry = null;
                ToolPathBounds = Rect.Empty;
                BoundingBoxGeometry = null;
                return;
            }

            var commands = FileViewModel.GcodeLines.Select(item => item.Command).ToList();

            var geometry = _plotterService.BuildGeometryFromCommands(commands,
            FileViewModel.Stats.MinX,
            FileViewModel.Stats.MinY);
            CanvasWidth = FileViewModel.Stats.Width;
            CanvasHeight = FileViewModel.Stats.Height;

            geometry.Freeze();
            ToolPathGeometry = geometry;

            var bounds = geometry.Bounds;
            ToolPathBounds = bounds;

            var bboxGeometry = new RectangleGeometry(bounds);
            bboxGeometry.Freeze();
            BoundingBoxGeometry = bboxGeometry;
            MachinePositionPointSize = (int)(Math.Max(1, Math.Min(bboxGeometry.Rect.Width, bboxGeometry.Rect.Height) * 0.05));
            GenerateAxisLabels();
        }

            [RelayCommand]
            private async Task SetOriginAsync()
            {
                if (!_grblService.IsConnected)
                {
                    return;
                }

                try
                {
                    await _grblService.SendCommandAsync("G10 L20 P1 X0 Y0");
                    await Task.Delay(500);
                    await _grblService.SendCommandAsync("?");

                    MachinePositionPoint = new Point(0, 0);
                }
                catch (Exception ex)
                {
                    // Gérer l'erreur (log via _log si injecté, ou MessageBox)
                }
            }
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
                    double val = minX + (stepX * i);
                    XAxisLabels.Add(val.ToString("F1"));
                }

                double minY = FileViewModel.Stats.MinY;
                double maxY = FileViewModel.Stats.MaxY;
                double stepY = (maxY - minY) / 4;

                for (int i = 4; i >= 0; i--)
                {
                    double val = minY + (stepY * i);
                    YAxisLabels.Add(val.ToString("F1"));
                }
            });
        }
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

        public event Action OnRequestAutoCenter;
        private void RequestAutoCenter() => OnRequestAutoCenter?.Invoke();
        [RelayCommand]
        public void AutoCenter()
        {
            RequestAutoCenter();
        }
       

        [RelayCommand]
        public void ToggleSimulation()
        {
            IsSimulating = !IsSimulating;
        }
        public event Action? OnRequestResetView;
        private void RequestResetView() => OnRequestResetView?.Invoke();

        [RelayCommand]
        public void ResetView()
        {
            OnRequestResetView?.Invoke();
        }

    }
}