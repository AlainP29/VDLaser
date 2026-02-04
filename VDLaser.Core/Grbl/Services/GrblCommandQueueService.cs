using VDLaser.Core.Console;
using VDLaser.Core.Grbl.Commands;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;

namespace VDLaser.Core.Grbl.Services
{
    public sealed class GrblCommandQueueService : IGrblCommandQueue
    {
        private readonly ILogService _log;
        private readonly Queue<GrblCommand> _queue = new();
        private readonly IGrblCoreService _core;
        private readonly IConsoleParserService _consoleParser;
        private readonly object _sync = new();
        private readonly SemaphoreSlim _signal = new(0, int.MaxValue);
        private GrblCommand? _current;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
        public event EventHandler<GrblCommandEventArgs>? CommandExecuted;
        public bool IsBusy => _current != null;
        private int _currentBufferSize = 0;
        private int _currentRxBytes = 0;
        public event EventHandler<int>? RxBufferSizeChanged;
        private readonly Queue<int> _commandLengths = new(); // Pour savoir combien d'octets retirer au "ok"
        public event EventHandler<int>? BufferSizeChanged; // Notifie la taille actuelle en octets
        public event Action<int>? LaserPowerCommandSent;
        public event Action<bool>? LaserStateCommandSent;
        public enum LaserMode
        {
            Off,
            Constant,   // M3
            Dynamic     // M4
        }

        public event Action<LaserMode>? LaserModeCommandSent;

        public GrblCommandQueueService(IGrblCoreService core, ILogService log, IConsoleParserService consoleParser)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _core = core ?? throw new ArgumentNullException(nameof(core));
            _consoleParser = consoleParser;

            _core.DataReceived += OnDataReceived;
            Task.Run(ProcessQueueAsync);
        }
        public async Task<GrblCommandResult> EnqueueAsync( string command, string? source = null, bool waitForOk = true, CancellationToken ct = default)
            {
                var cmd = new GrblCommand(command, source, waitForOk);
                lock (_sync)
                {
                    _queue.Enqueue(cmd);
                }
                _signal.Release();

                using (ct.Register(() => cmd.Completion.TrySetResult(GrblCommandResult.Cancelled)))
                {
                var resultTask = cmd.Completion.Task;
                var effectiveTimeout = source == "Job" ? TimeSpan.FromMinutes(10) : _timeout;
                var timeoutTask = Task.Delay(effectiveTimeout, ct).ContinueWith(_ => GrblCommandResult.Timeout);

                var completedTask = await Task.WhenAny(resultTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    _log.Warning("[GrblCommandQueueService] [{Source}] Timeout on {Cmd}", source, command);
                    return GrblCommandResult.Timeout;
                }
                return await resultTask;
                }
            }

        public Task SendRealtimeAsync(byte realtimeCommand, string? source = null)
        {
            _log.Debug("[GrblCommandQueueService] [RT:{Source}] 0x{Cmd:X2}", source, realtimeCommand);
            _core.SendRealtimeCommandAsync(realtimeCommand);
            return Task.CompletedTask;
        }
        private async Task ProcessQueueAsync()
        {
            while (true)
            {
                await _signal.WaitAsync();

                lock (_sync)
                {
                    if (!_queue.TryDequeue(out _current))
                        continue;
                }
                if (_current == null) continue;

                int lineLength = _current.Command.Length + 1; // +1 pour le caractère \n

                _commandLengths.Enqueue(lineLength);
                lock (_sync)
                {
                    _currentRxBytes += lineLength;
                    RxBufferSizeChanged?.Invoke(this, _currentRxBytes);
                    if (_current != null)
                    {
                        _consoleParser.BeginCommand(_current.Command);
                        if (_consoleParser.CurrentPendingCommand != null)
                            _consoleParser.CurrentPendingCommand.Source = ConsoleSource.Job;

                        NotifyLaserState(_current.Command);
                        _core.SendLine(_current.Command);
                        _log.Debug("[GrblCommandQueueService] {Source} >> {Cmd}", _current.Source, _current.Command);
                    }
                }
                lock (_sync)
                {
                    if (_current != null)
                    {
                        if(!_current.WaitForOk)
                        {
                            _currentRxBytes = Math.Max(0, _currentRxBytes - lineLength);
                            RxBufferSizeChanged?.Invoke(this, _currentRxBytes);
                            _current.Completion.TrySetResult(GrblCommandResult.Ok);
                            _current = null;
                        }
                    }
                    else { _log.Debug("[GrblCommandQueueService] Commande déjà complétée par GRBL (race condition)."); }
                }
            }
        }
        public void FlushCurrentAsOk(string reason = "Idle")
        {
            lock (_sync)
            {
                if (_current == null)
                    return;

                _log.Debug("[GrblCommandQueueService] Flush current command as OK ({Reason}): {Cmd}",
                    reason, _current.Command);

                _current.Completion.TrySetResult(GrblCommandResult.Ok);
                _current = null;
                _signal.Release();
            }
        }

        public void OnDataReceived(object? sender, DataReceivedEventArgs e)
        {
            lock (_sync)
            {
                if (_current == null)
                {
                    //_log.Warning("[GrblCommandQueueService] Réponse GRBL reçue sans commande active : {Line}", e.Text);  // Log pour debug
                    return;
                }

                var line = e.Text.Trim();

            if (line == "ok" && _current.WaitForOk)
            {
                    int cmdSize = _current.Command.Length + 1;
                    _currentRxBytes = Math.Max(0, _currentRxBytes - cmdSize);
                    RxBufferSizeChanged?.Invoke(this, _currentRxBytes);
                    CommandExecuted?.Invoke(this, new GrblCommandEventArgs(_current.Command, _current.Source));
                    _current.Completion.TrySetResult(GrblCommandResult.Ok);
                    if (_commandLengths.TryDequeue(out int length))
                    {
                        _currentBufferSize -= length;
                    }
                    _current = null;
                _signal.Release();
            }
            else if (line.StartsWith("error", StringComparison.OrdinalIgnoreCase))
            {
                    CommandExecuted?.Invoke(this, new GrblCommandEventArgs(_current.Command, _current.Source));
                    _current.Completion.TrySetResult(GrblCommandResult.Error);
                _current = null;
                _signal.Release();
            }
                else if (line.StartsWith("ALARM", StringComparison.OrdinalIgnoreCase))
                {
                    _current?.Completion.TrySetResult(GrblCommandResult.Error); // Ou Cancelled
                    _current = null;
                    _signal.Release(); // LIBÈRE LE BLOCAGE
                }
            }
        }
        public void Reset()
        {
            lock (_sync)
            {
                _current?.Completion.TrySetResult(GrblCommandResult.Cancelled);
                _current = null;

                while (_queue.TryDequeue(out var cmd))
                    cmd.Completion.TrySetResult(GrblCommandResult.Cancelled);

            }
            while (_signal.WaitAsync(0).Result) { }
        }
        private void NotifyLaserState(string command)
        {
            if (TryParseLaserPower(command, out int power))
            {
                LaserPowerCommandSent?.Invoke(power);
                LaserStateCommandSent?.Invoke(power > 0);
            }

            // --- Mode laser ---
            if (command.StartsWith("M3", StringComparison.OrdinalIgnoreCase))
            {
                LaserModeCommandSent?.Invoke(LaserMode.Constant);
                LaserStateCommandSent?.Invoke(true);
            }
            else if (command.StartsWith("M4", StringComparison.OrdinalIgnoreCase))
            {
                LaserModeCommandSent?.Invoke(LaserMode.Dynamic);
                LaserStateCommandSent?.Invoke(true);
            }
            else if (command.StartsWith("M5", StringComparison.OrdinalIgnoreCase))
            {
                LaserModeCommandSent?.Invoke(LaserMode.Off);
                LaserStateCommandSent?.Invoke(false);
                LaserPowerCommandSent?.Invoke(0);
            }
            _log.Debug("[GrblCommandQueueService] Laser Mode and Power notified for command: {Command}", command);
        }


        private static bool TryParseLaserPower(string command, out int power)
        {
            power = 0;

            int sIndex = command.IndexOf('S');
            if (sIndex < 0)
                return false;

            int start = sIndex + 1;
            int end = start;

            while (end < command.Length && char.IsDigit(command[end]))
                end++;

            return int.TryParse(command[start..end], out power);
        }

    }
}
