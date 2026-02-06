using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Serilog;
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
    /// G
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

        #region Champs de suivi (Job)
        private CancellationTokenSource? _jobTokenSource;
        private DateTime _jobStartTime;
        private TimeSpan _estimatedTotalTime;
        private TimeSpan _pausedDuration = TimeSpan.Zero;
        private DateTime? _pauseStartTime;
        private bool JobSuccess=false;
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
        public string WidthHeight => Stats != null ? $"{Stats.Width:F2} x {Stats.Height:F2} mm" : "-";
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

            // Inscription aux événements
            _gcodeJobService.ProgressChanged += OnJobProgressChanged;
            _grblCoreService.StatusUpdated += OnGrblStatusUpdated;
            _gcodeJobService.ExecutionProgressChanged += OnJobExecutionChanged;
            _commandQueue.RxBufferSizeChanged += OnRxBufferSizeChanged;
            _gcodeJobService.StateChanged += OnJobStateChanged;

            WeakReferenceMessenger.Default.Register<ErrorModeChangedMessage>(this, (sender, msg) =>
            {
                // Optionnel : Mettre à jour une propriété locale si besoin pour UI
                _log.Debug("[GcodeFileViewModel] Mode synchronisé : {Mode}", msg.Mode);
            });
        }

        #region Commands
        /// <summary>
        /// Ouvre une boîte de dialogue pour sélectionner et charger un fichier G-code.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanLoadOrFrame))]
        private async Task OpenFileAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "G-Code files (*.nc;*.gcode;*.txt)|*.nc;*.gcode;*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await LoadGcodeAsync(openFileDialog.FileName);
            }
        }
        /// <summary>
        /// Démarre l'exécution du job G-code actuel.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanStart))]
        public async Task StartJobAsync()
        {
            if (GcodeLines.Count == 0)
            {
                _log.Warning("[GcodeFileViewModel] Impossible de démarrer: Aucun fichier");
                await _dialogService.ShowErrorAsync("Veuillez d'abord charger un fichier G-code.", "Aucun fichier");
                return;
            }

            if (!_grblCoreService.IsConnected)
            {
                _log.Warning("[GcodeFileViewModel] Impossible de démarrer: Erreur de connexion");
                await _dialogService.ShowErrorAsync("La machine n'est pas connectée.", "Erreur de connexion");
                return;
            }
            _jobTokenSource = new CancellationTokenSource();
            _consoleViewModel.BeginJob();
            try
            {
                _log.Information("[GcodeFileViewModel] Démarrage du job G-code : {Total} lignes.", TotalLines);
                ResetJobState();

                var rawLines = GcodeLines.Select(item => item.RawLine).ToList();
                bool jobSuccess = await _gcodeJobService.PlayAsync(rawLines, _jobTokenSource.Token);
                if (jobSuccess)
                {
                    ProgressPercentage = 100;
                    _consoleViewModel.EndJob(true);
                    JobSuccess = jobSuccess;
                    _log.Information("[GcodeFileViewModel] Job G-code terminé avec succès.");
                }
                else
                {
                    _log.Warning("[GcodeFileViewModel] Job G-code interrompu ou terminé avec des erreurs.");
                    ProgressPercentage = 0;
                    _consoleViewModel.EndJob(false);
                }
            }
            catch (OperationCanceledException)
            {
                _log.Information("[GcodeFileViewModel] Job annulé par l'utilisateur.");
                _consoleViewModel.EndJob(false);
            }
            catch (Exception ex)
            {
                _log.Error("[GcodeFileViewModel] Erreur fatale lors de l'exécution du job : {Message}", ex.Message);
                ProgressPercentage = 0;
                await _dialogService.ShowErrorAsync($"Erreur durant l'exécution : {ex.Message}");
                _consoleViewModel.EndJob(false);
            }
            finally
            {
                ActualTotalTime = DateTime.UtcNow - _jobStartTime - _pausedDuration;
                //JobSuccess = jobSuccess && _consoleViewModel.ErrorCount == 0;  // Utilise ErrorCount de console pour erreurs non bloquantes
                ErrorCount = _consoleViewModel.ErrorCount;
                GenerateEngravingReport();
                _consoleViewModel.EndJob(JobSuccess);
                CleanupJob();
            }
        }
        /// <summary> 
        /// Arrête l'exécution en cours et réinitialise le service de job. 
        /// </summary>
        [RelayCommand(CanExecute = nameof(IsJobRunning))]
        public void StopJob()
        {
            if (_jobTokenSource == null) return;

            _gcodeJobService.Stop();
            _jobTokenSource.Cancel();
            
            _log.Information("[GcodeFileViewModel] Action - Demande d'arrêt du job reçue.");
        }
        /// <summary> Met l'exécution du job en pause. </summary>
        [RelayCommand(CanExecute = nameof(CanPause))]
        public void PauseJob()
        { 
            _gcodeJobService.Pause();
            OnPropertyChanged(nameof(PauseIcon));
            OnPropertyChanged(nameof(PauseToolTip));
            _log.Information("[GcodeFileViewModel] Action - Mise en pause du job en cours.");
        }
        /// <summary> Reprend l'exécution du job mis en pause. </summary>
        [RelayCommand(CanExecute = nameof(CanResume))]
        public void ResumeJob()
        {
            _gcodeJobService.Resume();
            _log.Information("[GcodeFileViewModel] Action - Reprise du job en cours.");

        }
        /// <summary>
        /// Exécute un tracé rectangulaire (cadrage) correspondant aux dimensions maximales du G-code chargé.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanLoadOrFrame))]
        public async Task RunFrameAsync()
        {
            if (Stats == null || !_grblCoreService.IsConnected)
            {
                _log.Warning("[GcodeFileViewModel] Framing impossible : Stats nulles ou GRBL déconnecté.");
                return;
            }

            const string frameSpeed = "F2000";
            IsFraming = true;
            try
            {
                _log.Information("[GcodeFileViewModel] Début de la séquence de cadrage (Framing)");
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

                _log.Information("[GcodeFileViewModel] Séquence de cadrage terminée.");
            }
            catch (Exception ex)
            {
                _log.Error("[GcodeFileViewModel] Erreur lors du framing : {Message}", ex.Message);
            }
            finally
            {
                IsFraming = false;
                OnJobStateChanged(this, EventArgs.Empty);
            }
        }
        [RelayCommand]
        public void TogglePauseJob()
        {             if (IsPaused)
                ResumeJob();
            else
                PauseJob();
            OnPropertyChanged(nameof(PauseIcon));
            OnPropertyChanged(nameof(PauseToolTip));
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Charge et analyse le contenu d'un fichier G-code.
        /// </summary>
        private async Task LoadGcodeAsync(string filePath)
        {
            try
            {
                var result = await _gcodeFileService.LoadAsync(filePath);
                FileName = Path.GetFileName(filePath);
                Stats = result.Stats;
                _log.Information("[GcodeFileViewModel] Rawlines count : {line} ", result.RawLines.Count);
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
                _log.Information("[GcodeFileViewModel] Avant set GcodeLines - Count: {Count}", GcodeLines.Count);
                GcodeLines.Clear();
                GcodeLines = new ObservableCollection<GcodeItemViewModel>(tempList);
                _log.Information("[GcodeFileViewModel] Après set GcodeLines - Count: {Count}", GcodeLines.Count);
                TotalLines = GcodeLines.Count;
                
                RefreshUIProperties();
                _log.Information("[GcodeFileViewModel] Fichier G-code chargé : {FilePath} ({Lines} lignes)", filePath, GcodeLines.Count);
            }
            catch (Exception ex)
            {
                _log.Error("[GcodeFileViewModel] Erreur lors du chargement du fichier G-code : {Message}", ex.Message);
                await _dialogService.ShowErrorAsync($"Impossible de lire le fichier :\n{ex.Message}", "Erreur de chargement");
            }

        }
        /// <summary> Réinitialise les variables de suivi temporel avant un nouveau job. </summary>
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
        /// <summary> Libère les ressources du job et réinitialise les indicateurs visuels des lignes. </summary>
        private void CleanupJob()
        {
            _jobTokenSource?.Dispose();
            _jobTokenSource = null;
            foreach (var item in GcodeLines) item.IsSent = false;
        }
        /// <summary> Notifie l'UI que les propriétés dépendantes des Stats ont changé. </summary>
        private void RefreshUIProperties()
        {
            OnPropertyChanged(nameof(DimensionsX));
            OnPropertyChanged(nameof(DimensionsY));
            OnPropertyChanged(nameof(WidthHeight));
            OnPropertyChanged(nameof(EstimatedJobTime));
            OnPropertyChanged(nameof(GcodeLines));
        }
        /// <summary> Calcule le temps restant en fonction de la progression réelle. </summary>
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

            // On ne laisse pas le temps dépasser la progression réelle de l'envoi
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
        private void OnJobStateChanged(object? sender, EventArgs e)
        {
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
            report.AppendLine($"Rapport de gravure pour {FileName}:");
            report.AppendLine($"Succès : {(JobSuccess ? "Oui" : $"Non (avec {_consoleViewModel.ErrorCount} erreurs non bloquantes)")}");
            report.AppendLine($"Temps total réel : {ActualTotalTime:hh\\:mm\\:ss}");
            report.AppendLine($"Lignes exécutées : {LinesExecuted}/{TotalLines}");
            if (_consoleViewModel.ErrorCount > 0)
            {
                report.AppendLine("Erreurs détaillées :");
                foreach (var msg in _consoleViewModel.ErrorMessages)  // Si ajoutée
                    report.AppendLine($"- {msg}");
                report.AppendLine($"Dernière erreur : {_consoleViewModel.LastErrorMessage}");
            }

            _log.Information("[GcodeFileViewModel] {Report}", report.ToString());
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
