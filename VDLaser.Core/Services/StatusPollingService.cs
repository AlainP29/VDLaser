using System;
using System.Security.Claims;
using System.Windows.Threading;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Services;
using static System.Formats.Asn1.AsnWriter;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.Core.Grbl.Services
{
    public sealed class StatusPollingService : IStatusPollingService, IDisposable
    {
        private readonly IGrblCoreService _core;
        private readonly ILogService _log;
        private readonly DispatcherTimer _timer;

        public bool IsRunning => _timer.IsEnabled;
        private volatile bool _isRequestPending;
        private int _consecutiveTimeouts = 0;
        private const int MaxConsecutiveTimeouts = 20;
        private int _ticksSinceLastRequest = 0;
        private const int TimeoutTicks = 20; // 10 ticks * 100ms = 1 seconde de timeout

        // =========================
        // CONSTRUCTOR
        // =========================
        public StatusPollingService(IGrblCoreService core, ILogService log)
        {
            _core = core ?? throw new ArgumentNullException(nameof(core));
            _log = log?? throw new ArgumentNullException(nameof(log));
            _core.ConnectionStateChanged += OnConnectionChanged;
            _core.StatusLineReceived += (_, _) => _isRequestPending = false;
            _core.StatusUpdated += OnStatusUpdated;
            _core.DataReceived += (s, e) => {
                if (e.Text.Contains("ok") || e.Text.Contains("unlock") || e.Text.Contains("$X"))
                {
                    _isRequestPending = false;
                }
            };
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_core.State.MachineState == MachState.Alarm ? 500 : 100) // 10 Hz (parfait) (200=>5Hz, 500=>2Hz)
            };

            _timer.Tick += OnTick;
            _log = log;
        }
        private void OnConnectionChanged(object? sender, bool connected)
        {
            if (connected)
                Start();
            else
                Stop();
        }
        private async void OnTick(object? sender, EventArgs e)
        {
            if (!_core.IsConnected)
                return;

            // PROTECTION 1 : Timeout de sécurité
            if (_isRequestPending)
            {
                _ticksSinceLastRequest++;
                if (_ticksSinceLastRequest >= TimeoutTicks)
                {
                    _log.Warning("[StatusPollingService] Timeout de réponse détecté ({0}ms). Forçage du verrou.", TimeoutTicks * 100);
                    _isRequestPending = false;
                    _ticksSinceLastRequest = 0;
                    _consecutiveTimeouts++;
                    if (_consecutiveTimeouts >= 5)
                    { 
                        _log.Warning("[StatusPollingservice] {Count} timeouts → Forcing status query retry.");
                        ForcePoll();
                        _timer.Interval = _core.State.MachineState == MachState.Alarm ? TimeSpan.FromMilliseconds(500) : TimeSpan.FromMilliseconds(100);
                    }
                }
                else
                {
                    _log.Debug("[Polling Tick] State={State}, Pending={Pending}, Timeouts={Count}", _core.State.MachineState, _isRequestPending, _consecutiveTimeouts);
                    return;
                }
            }
            try
            {
                //_log.Debug("[Polling Tick] State={State}, Pending={Pending}, Timeouts={Count}", _core.State.MachineState, _isRequestPending, _consecutiveTimeouts);
                _isRequestPending = true;
                _ticksSinceLastRequest = 0;
                await _core.SendRealtimeCommandAsync((byte)'?');
                _consecutiveTimeouts = 0; // Reset si réponse OK
            }
            catch (TimeoutException)
            {
                _consecutiveTimeouts++;
                _log.Debug("[StatusPolling] Timeout pendant polling ({Count}/{Max})", _consecutiveTimeouts, MaxConsecutiveTimeouts);

                if (_consecutiveTimeouts >= MaxConsecutiveTimeouts)
                {
                    _log.Warning("[StatusPolling] Trop de timeouts consécutifs → possible perte de connexion");
                    // Optionnel : déclencher une reconnexion ou alerte, mais PAS Disconnect ici
                }
            }
            catch (Exception ex)
            {
                _isRequestPending = false;
                _log.Error("[StatusPolling] TX error: {Msg}", ex.Message);
                _consecutiveTimeouts++;
            }
            finally
            {

                //_isRequestPending = false; // Toujours libérer pour le prochain tick
            }
        }

        // =========================
        // RX ACK
        // =========================
        private void OnStatusUpdated(object? sender, EventArgs e)
        {
            _isRequestPending = false;
        }
        // =========================
        // CONTROL
        // =========================
        public void Start()
        {
            if (_timer.IsEnabled)
                return;
            _log.Information("[StatusPollingService] Started");
            _timer.Start();
        }

        public void Stop()
        {
            if (!_timer.IsEnabled)
                return;
            _log.Information("[StatusPollingService] Stopped");
            _timer.Stop();
            _isRequestPending = false;
        }
        public void ForcePoll()
        {
            _isRequestPending = false;
        }
        public void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= OnTick;
        }
        // =========================
        // TEST UNIT HOOKS
        // =========================
        public void Tick_ForTests()//normalement internal mais pas possible avec DispatcherTimer
        {
            OnTick(null, null!);
        }

    }
}
