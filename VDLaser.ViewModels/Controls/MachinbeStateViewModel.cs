using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Gère l'affichage en temps réel de l'état de la machine (Positions, Vitesses, Buffers).
    /// </summary>
    public partial class MachineStateViewModel : ViewModelBase
    {
        #region private fields
        private readonly IGrblCoreService _core;
        private readonly ILogService _log; // Ajout du service de log
        private readonly IStatusPollingService _polling;
        private MachState _lastLoggedState = MachState.Undefined;
        #endregion

        #region Observables : État Machine
        [ObservableProperty] private MachState _machineState;
        [ObservableProperty] private SolidColorBrush _machineStateColor = Brushes.DarkGray;
        [ObservableProperty] private bool _isLaserPower = false;
        #endregion

        #region Observables : Positions Machine (Mpos)
        [ObservableProperty] private double _machineX;
        [ObservableProperty] private double _machineY;
        [ObservableProperty] private double _machineZ;
        #endregion

        #region Observables : Positions Travail (Wpos)
        [ObservableProperty] private double _workX;
        [ObservableProperty] private double _workY;
        [ObservableProperty] private double _workZ;
        #endregion

        #region Observables : Performances & Buffers
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

            _core.StatusUpdated += (_, _) => Refresh();
            _core.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IGrblCoreService.IsConnected))
                    Refresh();
            };

            Refresh();
            _log.Debug("[MachineStateViewModel] Initialized.");
        }


        /// <summary>
        /// Synchronise les propriétés du ViewModel avec l'état actuel du CoreService.
        /// </summary>
        private void Refresh()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var s = _core.State;

                if (s.MachineState != _lastLoggedState)
                {
                    _log.Information("[MachineStateViewModel] État changé : {Old} -> {New}", _lastLoggedState, s.MachineState);
                    _lastLoggedState = s.MachineState;
                    
                    _polling.ForcePoll();
                }
                

                MachineState =s.MachineState;
                MachineStateColor = s.MachineStatusColor;

                MachineX = s.MachinePosX;
                MachineY = s.MachinePosY;
                MachineZ = s.MachinePosZ;

                WorkX = s.WorkPosX;
                WorkY = s.WorkPosY;
                WorkZ = s.WorkPosZ;

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
    }
}
