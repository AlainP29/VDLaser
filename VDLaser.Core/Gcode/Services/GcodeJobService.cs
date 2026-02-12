using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Grbl.Commands;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;

namespace VDLaser.Core.Gcode.Services;
/// <summary>
/// Provides services for executing, controlling, and monitoring G-code jobs on a GRBL-based CNC controller.
/// </summary>
/// <remarks><para> <b>GcodeJobService</b> manages the lifecycle of a G-code job, including starting, pausing,
/// resuming, and stopping execution. It tracks job progress, handles error conditions according to the configured error
/// handling mode, and raises events to notify subscribers of state and progress changes. </para> <para> This service is
/// designed for use with GRBL-compatible devices and coordinates with command queue and core GRBL services to ensure
/// reliable job execution. It is not thread-safe; callers should ensure that method calls are appropriately
/// synchronized if accessed from multiple threads. </para></remarks>
public sealed class GcodeJobService : IGcodeJobService
{
    #region Fields & Dependencies

    private readonly IGrblCommandQueue _commandQueue;
    private readonly ILogService _log;
    private readonly IGrblCoreService _grblCore;
    private int _linesSent;
    private int _linesExecuted;
    private int _totalLines;
    private int _linesFailed;
    #endregion

    #region Events & State
    private CancellationTokenSource? _cts;
    private TaskCompletionSource<int>? _commandTaskSource;
    //private ManualResetEventSlim _pauseEvent = new(true);
    public event EventHandler? StateChanged;
    public event EventHandler<GcodeJobProgress>? ProgressChanged;
    public event EventHandler<GcodeJobProgress>? ExecutionProgressChanged;
    private TaskCompletionSource _pauseTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public record GcodeJobProgress(int CurrentLine, int TotalLines);

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    
    private readonly HashSet<int> _ignorableErrorCodes = new() {-1, 1, 2, 3, 4, 7, 11, 14, 16, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38 };
    public GcodeErrorHandlingMode ErrorHandlingMode { get; set; }
    = GcodeErrorHandlingMode.Strict;
    #endregion

    #region Constructor

    public GcodeJobService(
        IGrblCommandQueue commandQueue,
        ILogService log,
        IGrblCoreService grblCore)
    {
        _commandQueue = commandQueue;
        _grblCore = grblCore;
        _log = log;
        _commandQueue.CommandExecuted += OnCommandExecuted;
        _log.Information("[JOB][STATE] Démarrage — ModeErreur={Mode}", ErrorHandlingMode);

        _pauseTcs.SetResult();
    }
    #endregion

    #region Public API
    /// <summary>
    /// Plays a G-code job asynchronously.
    /// </summary>
    /// <param name="gcodeLines"></param>
    /// <param name="externalToken"></param>
    /// <returns></returns>
    public async Task<bool> PlayAsync(IEnumerable<string> gcodeLines, CancellationToken externalToken)
    {

        if (IsRunning) return false;

        var lines = gcodeLines.ToList();
        int totalLines = lines.Count;
        int currentLine = 0;
        _linesSent = 0;
        _linesExecuted = 0;
        _linesFailed = 0;

        _log.Information(
            "[JOB][STATE] Démarrage — Lignes={Total}, ModeErreur={Mode}",
            _totalLines,
            ErrorHandlingMode);

        _totalLines = lines.Count;
        //_commandTaskSource = new TaskCompletionSource<int>();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        IsRunning = true;
        bool success = true;

        // --- Boucle principale d’envoi G-code ---
        // Chaque ligne est envoyée, attendue, puis analysée selon le mode d’erreur.

        try
        {
            foreach (var line in gcodeLines)
            {
                _cts.Token.ThrowIfCancellationRequested();

                //_pauseEvent.Wait(_cts.Token);
                await _pauseTcs.Task;

                _commandTaskSource = new TaskCompletionSource<int>(
    TaskCreationOptions.RunContinuationsAsynchronously);

                if (_log.IsCncEnabled)
                {
                    _log.Debug(
                        "[JOB][CNC][TX] {Line}",
                        line);
                }

                var result = await _commandQueue.EnqueueAsync(
                    line,
                    source: "Job",
                    waitForOk: true,
                    _cts.Token);

                if (_log.IsCncEnabled)
                {
                    _log.Debug(
                        "[JOB][CNC] Résultat commande — Result={Result}, Ligne={Line}",
                        result,
                        line);
                }

                if (result == GrblCommandResult.Error)
                {
                    var completed = await Task.WhenAny(
                        _commandTaskSource.Task,
                        Task.Delay(500));

                    int errorCode = completed == _commandTaskSource.Task
                        ? _commandTaskSource.Task.Result
                        : -1;
                    int? rxErrorCode = _grblCore.GetLastRxErrorCode();
                    bool errorAfterOk = _grblCore.IsLastErrorAfterOk();

                    int effectiveErrorCode = rxErrorCode ?? errorCode;


                    bool isBlocking = ErrorHandlingMode == GcodeErrorHandlingMode.Strict || (
        effectiveErrorCode >= 0 && !_ignorableErrorCodes.Contains(effectiveErrorCode) && !errorAfterOk);

                    if (isBlocking)
                    {
                        _log.Error("[JOB][ERROR] STOP — Code={Code}, Mode={Mode}, Ligne={Line}",
                            effectiveErrorCode,
                            ErrorHandlingMode,
                            line);
                        Stop();
                        success = false;
                        break;
                    }

                    _log.Warning(
                             "[JOB][ERROR] CONTINUE — Code={Code}, Mode={Mode}, Ligne={Line}",
                            effectiveErrorCode,
                            ErrorHandlingMode,
                            line);

                    if (_log.IsCncEnabled)
                    {
                        _log.Debug(
                            "[JOB][CNC][ERROR] Raw={Raw}, Rx={Rx}, AfterOk={AfterOk}, Effective={Effective}, Blocking={Blocking}",
                            errorCode,
                            rxErrorCode,
                            errorAfterOk,
                            effectiveErrorCode,
                            isBlocking);
                    }

                    await _commandQueue.EnqueueAsync("M5", "Recovery", waitForOk: false);
                }

                _linesSent++;
                ProgressChanged?.Invoke(this, new GcodeJobProgress(_linesSent, _totalLines)); // Progression envoi
            }
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, timeoutCts.Token);

            while (_linesSent < _totalLines) { await Task.Delay(100, _cts.Token); }

            var executionTimeout = DateTime.UtcNow.AddSeconds(60);
            while (_linesExecuted < _totalLines)
            {
                if (_grblCore.State.MachineState == GrblState.MachState.Idle && _linesExecuted >= _totalLines)
                    break;
                if (DateTime.UtcNow > executionTimeout)
                {
                    _log.Error(
                        "[JOB][ERROR] Timeout exécution — Executed={Executed}/{Total}",
                        _linesExecuted,
                        _totalLines);
                    success = false;
                    break;
                }
                await Task.Delay(200, _cts.Token);
            }
            if (success)
            {
                _log.Information(
    "[JOB][STATE] Terminé — Succès={Success}, Executed={Executed}, Failed={Failed}",
    success,
    _linesExecuted,
    _linesFailed);

                await _commandQueue.EnqueueAsync("S0 M5", "JobCleanup", waitForOk: true, _cts.Token);
                await _commandQueue.EnqueueAsync("G90 G0 X0 Y0", "JobCleanup", waitForOk: true, _cts.Token);
            }
            else
            {
                _log.Warning(
                    "[JOB][STATE] Job incomplet — Cleanup partiel (M5 uniquement)");
                await _commandQueue.EnqueueAsync("S0 M5", "JobCleanup", waitForOk: true, _cts.Token);  // Toujours arrêter le laser
            }
            await Task.Delay(200);
        }
        catch (OperationCanceledException)
        {
            _log.Information("[JOB][STATE] Annulé par l'utilisateur.");

        }
        finally
        {
            Cleanup();
        }
        return success;
    }

    public void Pause()
    {
        if (!IsRunning || IsPaused)
            return;

        IsPaused = true;
        //_pauseEvent.Reset();
        _pauseTcs=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        StateChanged?.Invoke(this, EventArgs.Empty);
        _log.Information("[JOB][STATE] Pause demandée.");
    }

    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;
        //_pauseEvent.Set();
        _pauseTcs.TrySetResult();
        StateChanged?.Invoke(this, EventArgs.Empty);
        _log.Information("[JOB][STATE] Reprise demandée.");
    }

    public void Stop()
    {
        if (!IsRunning)
            return;

        _cts?.Cancel();

        if (_commandTaskSource != null && !_commandTaskSource.Task.IsCompleted)
        {
            _commandTaskSource.TrySetCanceled();
        }

        IsRunning = false;
        _commandQueue.Reset();
        StateChanged?.Invoke(this, EventArgs.Empty);
        _log.Information("[GcodeJobService][STATE] Stop demandé.");
    }
    #endregion

    #region Internal Callbacks
    /// <summary>
    /// Callback central de synchronisation RX/TX
    /// Appelé pour chaque réponse GRBL (ok / error / alarm)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCommandExecuted(object? sender, GrblCommandEventArgs e)
    {
        if (_log.IsCncEnabled)
        {
            _log.Debug(
                "[JOB][CNC][RX] ({Source}) {Response}",
                e.Source,
                e.Response);
        }


        if (e.Response!=null && e.Response.StartsWith("ALARM:"))
        {
            _log.Fatal(
    "[JOB][ERROR][ALARM] {Response} — Arrêt immédiat du job",
    e.Response);

            Stop();
            return;
        }
        if (_commandTaskSource == null || _commandTaskSource.Task.IsCompleted)
            return;

        bool isError = e.Response?.StartsWith("error:") == true;
        int errorCode = -1;

        if (isError)
        {
            if (int.TryParse(e.Response.Split(':')[1], out int code))
            {
                errorCode = code;
                e.ErrorCode = code;
            }
            _commandTaskSource?.TrySetResult(e.ErrorCode);
        }
        else if (e.Response == "ok")
        {
            _commandTaskSource?.TrySetResult(0);
        }

        if (IsRunning && e.Source == "Job")
        {
            bool shouldIncrement = !isError || _ignorableErrorCodes.Contains(errorCode);

            if (shouldIncrement)
            {
                _linesExecuted++;
                ExecutionProgressChanged?.Invoke(this, new GcodeJobProgress(_linesExecuted, _totalLines));
                if (_log.IsCncEnabled)
                {
                    _log.Debug(
                        "[JOB][CNC] Ligne exécutée — Status={Status}, {Current}/{Total}",
                        isError ? "Erreur ignorée" : "OK",
                        _linesExecuted,
                        _totalLines);
                }


            }
            else if (isError)
            {
                _linesFailed++;
                _linesExecuted++; //ajout de pour refléter la ligne exécutée malgré l'erreur
                ExecutionProgressChanged?.Invoke(this, new GcodeJobProgress(_linesExecuted, _totalLines)); // Mise à jour de la progression malgré l'erreur
            }
        }
    }
    private void OnConnectionLost(object? sender, EventArgs e)
    {
        if (IsRunning)
        {
            _log.Error(
                "[JOB][ERROR] Perte de connexion — Arrêt immédiat du job");
            Stop();
        }
    }
    #endregion

    #region Cleanup & Disposal

    public void Cleanup()
    {
        IsRunning = false;
        IsPaused = false;
        _linesExecuted = 0;

        //_pauseEvent.Set();
        _cts?.Dispose();
        _cts = null;
        StateChanged?.Invoke(this, EventArgs.Empty); 
    }
    public void Dispose()
    {
        _commandQueue.CommandExecuted -= OnCommandExecuted;
    }
    #endregion
}

