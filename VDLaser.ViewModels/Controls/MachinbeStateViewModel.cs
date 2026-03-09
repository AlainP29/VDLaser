using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.ViewModels.Controls
{/// <summary>
 /// Monitors and displays real-time machine status, including coordinates, 
 /// speeds, and buffer states.
 /// </summary>
    public partial class MachineStateViewModel : ViewModelBase
    {
        #region Fields & Services
        private readonly IGrblCoreService _core;
        private readonly ILogService _log;
        private readonly IStatusPollingService _polling;
        private MachState _lastLoggedState = MachState.Undefined;
        #endregion

        #region Observables: Machine Status
        [ObservableProperty] private MachState _machineState;
        [ObservableProperty] private SolidColorBrush _machineStateColor = Brushes.DarkGray;
        [ObservableProperty] private bool _isLaserPower = false;
        #endregion

        #region Observables: Machine Positions (MPos)
        [ObservableProperty] private double _machineX;
        [ObservableProperty] private double _machineY;
        [ObservableProperty] private double _machineZ;
        #endregion

        #region Observables: Work Positions (WPos)
        [ObservableProperty] private double _workX;
        [ObservableProperty] private double _workY;
        [ObservableProperty] private double _workZ;
        #endregion

        #region Observables: Performance & Buffers
        [ObservableProperty] private string _feedRate = "0";
        [ObservableProperty] private string _spindleSpeed = "0";
        [ObservableProperty] private string _laserPower = "0";

        [ObservableProperty] private int _rxBuffer;
        [ObservableProperty] private int _plannerBuffer;
        #endregion


        public MachineStateViewModel(IGrblCoreService core,ILogService log,IStatusPollingService polling)
        {
            _core = core ?? throw new ArgumentNullException(nameof(core));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _polling = polling ?? throw new ArgumentNullException(nameof(polling));

            _core.StatusUpdated += OnStatusUpdated;
            _core.PropertyChanged += OnCorePropertyChanged;

            Refresh();
            LogContextual(_log, "Initialized", "Machine state monitoring active");
        }

        #region Logic
        /// <summary>
        /// Synchronizes ViewModel properties with the current CoreService state.
        /// </summary>
        private void Refresh()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var s = _core.State;

                if (s.MachineState != _lastLoggedState)
                {
                    LogContextual(_log, "StateChanged", $"Machine: {_lastLoggedState} -> {s.MachineState}");
                    _lastLoggedState = s.MachineState;
                    
                    _polling.ForcePoll();
                }

                // Update Status & Colors
                MachineState = s.MachineState;
                MachineStateColor = s.MachineStatusColor;

                // Update Coordinates
                MachineX = s.MachinePosX;
                MachineY = s.MachinePosY;
                MachineZ = s.MachinePosZ;
                WorkX = s.WorkPosX;
                WorkY = s.WorkPosY;
                WorkZ = s.WorkPosZ;

                // Update Real-time Data
                FeedRate = s.MachineFeed;
                SpindleSpeed = s.MachineSpeed;
                LaserPower = s.MachineSpeed;
                RxBuffer= s.RxBuffer;
                PlannerBuffer= s.PlannerBuffer;

                if(LaserPower != "0")
                IsLaserPower = true;
                else IsLaserPower = false;
            });
        }
        #endregion

        #region Event Handlers
        private void OnStatusUpdated(object? sender, EventArgs e) => Refresh();

        private void OnCorePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IGrblCoreService.IsConnected))
                Refresh();
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _core.StatusUpdated -= OnStatusUpdated;
                _core.PropertyChanged -= OnCorePropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}
