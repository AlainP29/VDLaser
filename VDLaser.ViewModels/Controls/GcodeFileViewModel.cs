using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using VDLaser.Core.Gcode;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;
using static VDLaser.Core.Gcode.Services.GcodeJobService;
using static VDLaser.ViewModels.Controls.GcodeSettingsViewModel;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Manages the loading, display, and execution of G-code files, including job control (start, pause, stop) and real-time progress tracking.
    /// </summary>
    public partial class GcodeFileViewModel : ViewModelBase
    {
        #region Fields & Services
        private readonly IGrblCommandQueue _commandQueue;
        private readonly ILogService _log;
        private readonly IGrblCoreService _grblCoreService;
        private readonly IGcodeFileService _gcodeFileService;
        private readonly IGcodeJobService _gcodeJobService;
        private readonly IDialogService _dialogService;
        private readonly ConsoleViewModel _consoleViewModel;
        #endregion

        #region Job properties
        private CancellationTokenSource? _jobTokenSource;
        private DateTime _jobStartTime;
        private TimeSpan _estimatedTotalTime;
        private TimeSpan _pausedDuration = TimeSpan.Zero;
        private DateTime? _pauseStartTime;
        private bool JobSuccess=false;
        private bool _isDisplayingError = false;
        private bool _isRecoveryExecuted = false;
        #endregion

        #region Observable Properties
        [ObservableProperty]
        private ObservableCollection<GcodeItemViewModel> _gcodeLines = new();
        [ObservableProperty]
        private GcodeStats? _stats;
        [ObservableProperty]
        private double _progressPercentage;
        [ObservableProperty]
        private int _currentLineIndex;
        [ObservableProperty]
        private string _fileName = "File name";
        [ObservableProperty]
        private string _estimatedJobTime = "00:00:00";
        [ObservableProperty]
        private int _totalLines=0;
        [ObservableProperty] 
        private int _rxBuffer;
        [ObservableProperty]
        private string _rxBufferDisplay;
        [ObservableProperty] 
        private int _plannerBuffer;
        [ObservableProperty]
        private string _plannerBufferDisplay;
        [ObservableProperty] 
        private int _plannerBufferCount;
        [ObservableProperty] 
        private int _linesExecuted;
        [ObservableProperty]
        private bool _isSent;
        [ObservableProperty]
        private GcodeItemViewModel? _selectedItem;
        [ObservableProperty]
        private TimeSpan _actualTotalTime = TimeSpan.Zero;
        [ObservableProperty]
        private int _errorCount;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenFileCommand))]
        [NotifyCanExecuteChangedFor(nameof(RunFrameCommand))]
        private bool _isFraming;
        public record GcodeLineSelectedMessage(double? X, double? Y);

        #endregion

        #region Computed Properties (UI State)

        // Propriétés d'affichage pour les dimensions
        public string DimensionsX => Stats != null ? $"{Stats.MinX:F2} à {Stats.MaxX:F2} mm" : "-";
        public string DimensionsY => Stats != null ? $"{Stats.MinY:F2} à {Stats.MaxY:F2} mm" : "-";
        public string WidthHeight => Stats != null ? $"{Stats.Width:F0} x {Stats.Height:F0}" : "-";
        public bool IsPaused => _gcodeJobService.IsPaused;
        public bool IsJobRunning => _gcodeJobService.IsRunning;
        public bool IsJobNotRunning => !_gcodeJobService.IsRunning;
        private bool CanLoadOrFrame => !IsJobRunning;
        private bool CanStart => !IsJobRunning && GcodeLines.Count > 0 && _grblCoreService.IsConnected;
        private bool CanPause => IsJobRunning && _grblCoreService.State.MachineState != GrblState.MachState.Hold;
        private bool CanResume => IsJobRunning && _grblCoreService.State.MachineState == GrblState.MachState.Hold;
        public string PauseToolTip => IsPaused ? "Reprendre la gravure" : "Mettre en pause";
        public string PauseIcon => IsPaused ? "/Resources/Assets/iconResume40.png" : "/Resources/Assets/iconPause40.png";
        #endregion

        public GcodeFileViewModel(IGcodeFileService gcodeFileService,
            IGcodeJobService gcodeJobService, IGrblCommandQueue commandQueue, ILogService log, IGrblCoreService grblCoreService, IDialogService dialogService, ConsoleViewModel consoleViewModel)
        {
            _gcodeFileService = gcodeFileService ?? throw new ArgumentNullException(nameof(gcodeFileService));
            _gcodeJobService = gcodeJobService ?? throw new ArgumentNullException(nameof(gcodeJobService));
            _commandQueue = commandQueue ?? throw new ArgumentNullException(nameof(commandQueue));
            _grblCoreService = grblCoreService ?? throw new ArgumentNullException(nameof(grblCoreService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(log));
            _consoleViewModel= consoleViewModel ?? throw new ArgumentNullException(nameof(consoleViewModel));

            _gcodeJobService.ProgressChanged += OnJobProgressChanged;
            _grblCoreService.StatusUpdated += OnGrblStatusUpdated;
            _gcodeJobService.ExecutionProgressChanged += OnJobExecutionChanged;
            _commandQueue.RxBufferSizeChanged += OnRxBufferSizeChanged;
            _gcodeJobService.StateChanged += OnJobStateChanged;

            WeakReferenceMessenger.Default.Register<ErrorModeChangedMessage>(this, (sender, msg) =>
            {
                _log.Debug("[GCODEFILE] Sync mode : {Mode}", msg.Mode);
            });
            _log.Information("[GCODEFILE] Initialized");

        }

        #region Commands
        /// <summary>
        /// Open a file dialog to select a G-code file, then load and parse it. This command is disabled while a job is running or during framing to prevent conflicts.
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanLoadOrFrame))]
        private async Task OpenFileAsync()
        {
            LogContextual(_log, "OpenFileAsync", "Opening file dialog");
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "G-Code files (*.nc;*.gcode)|*.nc;*.gcode|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await LoadGcodeAsync(openFileDialog.FileName);
                LogContextual(_log, "OpenFileAsync", $"Loaded: {FileName}");
            }
        }
        /// <summary>
        /// Starts the execution of the loaded G-code job asynchronously.
        /// </summary>
        /// <remarks>The job will only start if a G-code file is loaded and the machine is connected. If
        /// these conditions are not met, an error dialog is displayed and the job does not start. Progress and job
        /// state are updated throughout execution. If the job is cancelled or an error occurs, the operation is safely
        /// terminated and the user is notified. This method is intended to be used as a command in UI
        /// scenarios.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [RelayCommand(CanExecute = nameof(CanStart))]
        public async Task StartJobAsync()
        {
            LogContextual(_log, "StartJob", $"Starting job: {FileName}");
            if (GcodeLines.Count == 0)
            {
                _log.Warning("[GCODEFILE] No file");
                await _dialogService.ShowErrorAsync("Please load a gcode file.", "No file");
                return;
            }

            if (!_grblCoreService.IsConnected)
            {
                _log.Warning("[GCODEFILE] Not possible to start: Connection error");
                await _dialogService.ShowErrorAsync("The machine is not connected.", "Connection error");
                return;
            }
            _jobTokenSource = new CancellationTokenSource();
            _consoleViewModel.BeginJob();
            try
            {
                _log.Information("[GCODEFILE] Start job G-code : {Total} lines.", TotalLines);
                ResetJobState();

                var rawLines = GcodeLines.Select(item => item.RawLine).ToList();
                bool jobSuccess = await _gcodeJobService.PlayAsync(rawLines, _jobTokenSource.Token);
                if (jobSuccess)
                {
                    ProgressPercentage = 100;
                    _consoleViewModel.EndJob(true);
                    JobSuccess = jobSuccess;
                    _log.Information("[GCODEFILE] G-code Job success.");
                }
                else
                {
                    _log.Warning("[GCODEFILE] G-code Job stopped or finished with errors.");
                    ProgressPercentage = 0;
                    _consoleViewModel.EndJob(false);
                }
            }
            catch (OperationCanceledException)
            {
                _log.Error("[GCODEFILE] User stopped Job.");
                _consoleViewModel.EndJob(false);
            }
            catch (Exception ex)
            {
                _log.Error("[GCODEFILE] Fatal error during job execution : {Message}", ex.Message);
                ProgressPercentage = 0;
                await _dialogService.ShowErrorAsync($"Error during executionn : {ex.Message}");
                _consoleViewModel.EndJob(false);
            }
            finally
            {
                ActualTotalTime = DateTime.UtcNow - _jobStartTime - _pausedDuration;
                ErrorCount = _consoleViewModel.ErrorCount;
                GenerateEngravingReport();
                _consoleViewModel.EndJob(JobSuccess);
                CleanupJob();
            }
        }
        /// <summary>
        /// Stops the currently running job immediately if a job is active.
        /// </summary>
        /// <remarks>This method cancels the ongoing job operation and signals any associated services to
        /// halt processing. Can only be executed when a job is running, as determined by the IsJobRunning property.
        /// Calling this method when no job is active has no effect.</remarks>
        [RelayCommand(CanExecute = nameof(IsJobRunning))]
        public void StopJob()
        {
            LogContextual(_log, "StopJob", "User requested immediate stop");
            if (_jobTokenSource == null) return;

            _gcodeJobService.Stop();
            _jobTokenSource.Cancel();
        }
        /// <summary>
        /// Put the current job on hold, allowing for a temporary pause in execution. Can only be executed when a job is actively running and not already paused. When invoked, this method signals the job service to enter a paused state, which should halt the sending of G-code commands to the machine until resumed. The UI should reflect the paused state through properties like PauseIcon and PauseToolTip, which are updated after pausing.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanPause))]
        public void PauseJob()
        {
            LogContextual(_log, "PauseJob", "User requested job pause");

            _gcodeJobService.Pause();
            OnPropertyChanged(nameof(PauseIcon));
            OnPropertyChanged(nameof(PauseToolTip));
        }
        /// <summary>
        /// Resumes a paused job, allowing execution to continue from the point it was paused. Can only be executed when a job is actively running and currently in a paused state. When invoked, this method signals the job service to exit the paused state and resume sending G-code commands to the machine. The UI should update accordingly to reflect the resumed state through properties like PauseIcon and PauseToolTip, which are updated after resuming.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanResume))]
        public void ResumeJob()
        {
            LogContextual(_log, "ResumeJob", "User resume current job");
            _gcodeJobService.Resume();

        }
        /// <summary>
        /// Exécute un tracé rectangulaire (cadrage) correspondant aux dimensions maximales du G-code chargé.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanLoadOrFrame))]
        public async Task RunFrameAsync()
        {
            LogContextual(_log, "RunFrameAsync", "User requested a framing");

            if (Stats == null || !_grblCoreService.IsConnected)
            {
                _log.Warning("[GCODEFILE] Framing impossible : Stats null ou GRBL disconnected.");
                return;
            }

            const string frameSpeed = "F2000";
            IsFraming = true;
            try
            {
                ProgressPercentage = 0;
                await _commandQueue.EnqueueAsync("G90", "Frame");
                _grblCoreService.RaiseDataReceived($">> G90");
                ProgressPercentage = 20;
                await _commandQueue.EnqueueAsync("M3 S10", "Frame");
                _grblCoreService.RaiseDataReceived($">> M3 S10");
                ProgressPercentage = 40;
                await _commandQueue.EnqueueAsync($"G0 X{Stats.MinX.ToString(CultureInfo.InvariantCulture)} Y{Stats.MinY.ToString(CultureInfo.InvariantCulture)} {frameSpeed}", "Frame");
                _grblCoreService.RaiseDataReceived($">> G0 X{Stats.MinX.ToString(CultureInfo.InvariantCulture)} Y{Stats.MinY.ToString(CultureInfo.InvariantCulture)} {frameSpeed}");
                ProgressPercentage = 60;
                await _commandQueue.EnqueueAsync($"G0 X{Stats.MinX.ToString(CultureInfo.InvariantCulture)} Y{Stats.MaxY.ToString(CultureInfo.InvariantCulture)} {frameSpeed}", "Frame");
                _grblCoreService.RaiseDataReceived($">> G0 X{Stats.MinX.ToString(CultureInfo.InvariantCulture)} Y{Stats.MaxY.ToString(CultureInfo.InvariantCulture)} {frameSpeed}");
                ProgressPercentage = 70;
                await _commandQueue.EnqueueAsync($"G0 X{Stats.MaxX.ToString(CultureInfo.InvariantCulture)} Y{Stats.MaxY.ToString(CultureInfo.InvariantCulture)} {frameSpeed}", "Frame");
                _grblCoreService.RaiseDataReceived($">> X{Stats.MaxX.ToString(CultureInfo.InvariantCulture)} Y{Stats.MaxY.ToString(CultureInfo.InvariantCulture)} {frameSpeed}");
                await _commandQueue.EnqueueAsync($"G0 X{Stats.MaxX.ToString(CultureInfo.InvariantCulture)} Y{Stats.MinY.ToString(CultureInfo.InvariantCulture)} {frameSpeed}", "Frame");
                ProgressPercentage = 80;
                _grblCoreService.RaiseDataReceived($">> G0 X{Stats.MaxX.ToString(CultureInfo.InvariantCulture)} Y{Stats.MinY.ToString(CultureInfo.InvariantCulture)} {frameSpeed}");
                await _commandQueue.EnqueueAsync($"G0 X{Stats.MinX.ToString(CultureInfo.InvariantCulture)} Y{Stats.MinY.ToString(CultureInfo.InvariantCulture)} {frameSpeed}", "Frame");
                ProgressPercentage = 90;
                _grblCoreService.RaiseDataReceived($">> G0 X{Stats.MinX.ToString(CultureInfo.InvariantCulture)} Y{Stats.MinY.ToString(CultureInfo.InvariantCulture)} {frameSpeed}");
                await _commandQueue.EnqueueAsync("M5", "Frame",false);
                ProgressPercentage = 100;

                _grblCoreService.RaiseDataReceived($">> M5");
            }
            catch (Exception ex)
            {
                _log.Error("[GCODEFILE] Error during framing : {Message}", ex.Message);
            }
            finally
            {
                IsFraming = false;
                OnJobStateChanged(this, EventArgs.Empty);
            }
        }
        [RelayCommand]
        public void TogglePauseJob()
        {             
            if (IsPaused)
                ResumeJob();
            else
                PauseJob();
            OnPropertyChanged(nameof(PauseIcon));
            OnPropertyChanged(nameof(PauseToolTip));
        }
        [RelayCommand(CanExecute = nameof(CanRecover))]
        private async Task RecoverFromErrorAsync()
        {
            try
            {
                _isRecoveryExecuted= true;
                _log.Information("[GCODEFILE][JOB] Initiation of the recovery procedure.");

                await _grblCoreService.SendCommandAsync("M5");
                await _grblCoreService.SendCommandAsync("$X");

                _commandQueue.ClearQueue();

                await _grblCoreService.SendCommandAsync("G90 G0 X0 Y0");

                _log.Information("[GCODEFILE][JOB] Machine secured and returned to Origin.");

                await _dialogService.ShowInfoAsync(
            "The security procedure has been executed:\n" +
            "- Laser off (M5)\n" +
            "- Alarm unlocked ($X)\n" +
            "- Return to origin (G0 X0 Y0)",
            "Recovery complete");
                _isDisplayingError = false;
            }
            catch (Exception ex)
            {
                _log.Error("[GCODEFILE][JOB] Error during retrieval.", ex);
                _isRecoveryExecuted= false;
                await _dialogService.ShowErrorAsync("[GcodeFileVM][JOB] Error during retrieval : " + ex.Message);
            }
            finally
            {
                LinesExecuted = 0;
                EstimatedJobTime="00:00:00";
                RecoverFromErrorCommand.NotifyCanExecuteChanged();
            }
        }
        /// <summary>
        /// Can recover if the job is not running, there was a job in progress (LinesExecuted > 0) 
        /// and we are in an error state (e.g., GRBL is in Alarm/Error state). 
        /// This allows the user to attempt a recovery without needing to reset the entire application. 
        /// The actual check for GRBL's error state would be done in the command execution, but this method ensures we only enable recovery when it makes sense.
        /// </summary>
        /// <returns></returns>
        private bool CanRecover()
        {
            return !_gcodeJobService.IsRunning && !JobSuccess && LinesExecuted > 0 && !_isRecoveryExecuted;
        }
        #endregion

        #region Private Methods
        private async Task LoadGcodeAsync(string filePath)
        {
            try
            {
                var result = await _gcodeFileService.LoadAsync(filePath);
                FileName = Path.GetFileName(filePath);
                Stats = result.Stats;
                var tempList = new List<GcodeItemViewModel>();
                int lineNumber = 1;

                for (int i = 0; i < result.RawLines.Count; i++)
                {
                    var rawLine = result.RawLines[i].Trim();
                    var command = result.ParsedCommands[i];

                    if (!command.IsEmpty)
                    {
                        tempList.Add(new GcodeItemViewModel(lineNumber++, rawLine, command,_log));
                    }
                }
                GcodeLines.Clear();
                GcodeLines = new ObservableCollection<GcodeItemViewModel>(tempList);
                TotalLines = GcodeLines.Count;
                
                RefreshUIProperties();
            }
            catch (Exception ex)
            {
                _log.Error("[GCODEFILE] Erro when loading G-code file : {Message}", ex.Message);
                await _dialogService.ShowErrorAsync($"Impossible to load file :\n{ex.Message}", "Error during loading");
            }
        }
        private void ResetJobState()
        {
            _jobStartTime = DateTime.UtcNow;
            _estimatedTotalTime = Stats?.EstimatedTime ?? TimeSpan.Zero;
            _pausedDuration = TimeSpan.Zero;
            _pauseStartTime = null;
            ProgressPercentage = 0;
            EstimatedJobTime = _estimatedTotalTime.ToString(@"hh\:mm\:ss");
            
            JobSuccess = false;
            ActualTotalTime = TimeSpan.Zero;
            _consoleViewModel.ResetErrorsForJob();
        }
        private void CleanupJob()
        {
            _jobTokenSource?.Dispose();
            _jobTokenSource = null;
            foreach (var item in GcodeLines) item.IsSent = false;
        }
        private void RefreshUIProperties()
        {
            OnPropertyChanged(nameof(DimensionsX));
            OnPropertyChanged(nameof(DimensionsY));
            OnPropertyChanged(nameof(WidthHeight));
            OnPropertyChanged(nameof(EstimatedJobTime));
            OnPropertyChanged(nameof(GcodeLines));
        }
        private void UpdateRemainingTime(int linesExecuted, int totalLines)
        {
            if (totalLines == 0 || linesExecuted == 0)
            {
                EstimatedJobTime = "--:--:--";
                return;
            }

            double progressRatio = (double)linesExecuted / totalLines;
            var elapsed = DateTime.UtcNow - _jobStartTime - _pausedDuration;

            var newEstimatedTotal = TimeSpan.FromTicks((long)(elapsed.Ticks / progressRatio));
            _estimatedTotalTime = newEstimatedTotal;

            var remaining = _estimatedTotalTime - elapsed;
            if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;

            EstimatedJobTime = (remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining).ToString(@"hh\:mm\:ss");
        }
        private void UpdateProgressRunning()
        {
            if (_estimatedTotalTime == TimeSpan.Zero) return;

            var elapsed = DateTime.UtcNow - _jobStartTime - _pausedDuration;
            var timeProgress = Math.Clamp(elapsed.TotalSeconds / _estimatedTotalTime.TotalSeconds, 0.0, 1.0);

            int estimatedRemainingLines = _grblCoreService.State.PlannerBuffer;
            double bufferAdjustment = (double)estimatedRemainingLines / TotalLines;
            timeProgress = Math.Max(timeProgress, 1.0 - bufferAdjustment);

            double sendProgress = ProgressPercentage / 100.0;
            if (timeProgress > sendProgress) timeProgress = sendProgress;

            ProgressPercentage = timeProgress * 100.0;
        }
        #endregion

        #region Event Handlers
        private void OnJobProgressChanged(object? sender, GcodeJobProgress e)
        {
            CurrentLineIndex = e.CurrentLine;

            if (CurrentLineIndex > 0 && CurrentLineIndex <= GcodeLines.Count)
            {
                GcodeLines[CurrentLineIndex - 1].IsSent = true;
            }
        }
        private async void OnJobStateChanged(object? sender, EventArgs e)
        {
            if (!_gcodeJobService.IsRunning && LinesExecuted > 0 && LinesExecuted < TotalLines && !JobSuccess)
            {
                if (!_isDisplayingError && !_isRecoveryExecuted)
                {
                    _isDisplayingError = true;
                    await _grblCoreService.SendCommandAsync("M5");
                    _log.Warning("[GCODEFILE] [SAFETY] Job interrupted abnormally. M5 order sent automatically.");
                    await _dialogService.ShowErrorAsync("Strict mode interrupted the work due to a GRBL error. The laser was shut down. Clic on Recovery button only if the path to origin is cleared",
                        "Critical Stop");
                }
            }
            else if (_gcodeJobService.IsRunning)
            {
                _isDisplayingError = false;
                _isRecoveryExecuted= false;
            }
            var dispatcher = System.Windows.Application.Current?.Dispatcher;

            if (dispatcher != null)
            {
                dispatcher.Invoke(() =>
                {
                    StartJobCommand.NotifyCanExecuteChanged();
                    PauseJobCommand.NotifyCanExecuteChanged();
                    StopJobCommand.NotifyCanExecuteChanged();
                    OpenFileCommand.NotifyCanExecuteChanged();
                    RunFrameCommand.NotifyCanExecuteChanged();
                    ResumeJobCommand.NotifyCanExecuteChanged();
                    TogglePauseJobCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(IsJobRunning));
                    OnPropertyChanged(nameof(IsJobNotRunning));
                    OnPropertyChanged(nameof(PauseIcon));
                    OnPropertyChanged(nameof(PauseToolTip));
                    RecoverFromErrorCommand.NotifyCanExecuteChanged();
                });
            }
        }
        private void OnJobExecutionChanged(object? sender, GcodeJobProgress e)
        {
            LinesExecuted = e.CurrentLine;
            ProgressPercentage = e.TotalLines == 0 ? 0 : (double)e.CurrentLine / e.TotalLines * 100.0;
            UpdateRemainingTime(e.CurrentLine, e.TotalLines);
        }
        private void OnGrblStatusUpdated(object? sender, EventArgs e)
        {
            var state = _grblCoreService.State;

            switch (state.MachineState)
            {
                case GrblState.MachState.Run:
                    if (_pauseStartTime.HasValue)
                    {
                        _pausedDuration += DateTime.UtcNow - _pauseStartTime.Value;
                        _pauseStartTime = null;
                    }

                    if (_estimatedTotalTime == TimeSpan.Zero) return;
                    UpdateProgressRunning();
                    break;

                case GrblState.MachState.Hold:
                    if (!_pauseStartTime.HasValue)
                        _pauseStartTime = DateTime.UtcNow;
                    
                    break;

                case GrblState.MachState.Idle:
                    /*if (!_gcodeJobService.IsRunning)
                        return;

                    if (_gcodeJobService.IsRunning && _linesExecuted >= TotalLines)
                    {
                        _commandQueue.FlushCurrentAsOk("Grbl Idle");
                        ProgressPercentage = 100;
                        EstimatedJobTime = "00:00:00";
                    }
                    break;*/
                    OnJobStateChanged(this, EventArgs.Empty);
                    return;
            }
        }
        private void OnRxBufferSizeChanged(object? sender, int currentByteCount)
        {
            RxBuffer = currentByteCount;
            RxBufferDisplay = $"{currentByteCount}";
        }
        partial void OnSelectedItemChanged(GcodeItemViewModel? value)
        {
            if (value != null)
            {
                WeakReferenceMessenger.Default.Send(new GcodeLineSelectedMessage(value.Command.X, value.Command.Y));
            }
        }
        private void GenerateEngravingReport()
        {
            var report = new StringBuilder();
            report.AppendLine($"Job report for {FileName}:");
            report.AppendLine($"Success : {(JobSuccess ? "Yes" : $"No (with {_consoleViewModel.ErrorCount} completed with error)")}");
            report.AppendLine($"Total time : {ActualTotalTime:hh\\:mm\\:ss}");
            report.AppendLine($"Lines executed : {LinesExecuted}/{TotalLines}");
            if (_consoleViewModel.ErrorCount > 0)
            {
                report.AppendLine("Error details :");
                foreach (var msg in _consoleViewModel.ErrorMessages)
                    report.AppendLine($"- {msg}");
                report.AppendLine($"Last error : {_consoleViewModel.LastErrorMessage}");
            }

            _log.Information("[GCODEFILE] {Report}", report.ToString());
            LogContextual(_log, "JobReport", report.ToString());
        }
        
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gcodeJobService.ProgressChanged -= OnJobProgressChanged;
                _grblCoreService.StatusUpdated -= OnGrblStatusUpdated;
                _gcodeJobService.ExecutionProgressChanged -= OnJobExecutionChanged;
                _gcodeJobService.StateChanged -= OnJobStateChanged;
                _commandQueue.RxBufferSizeChanged -= OnRxBufferSizeChanged;
            }

            base.Dispose(disposing);
        }

    }
}
