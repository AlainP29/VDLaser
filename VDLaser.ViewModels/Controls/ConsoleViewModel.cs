using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Data;
using VDLaser.Core.Console;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Represents the view model for the console interface, providing collections and commands for displaying,
    /// filtering, and exporting console messages in both raw and structured formats.
    /// </summary>
    /// <remarks>ConsoleViewModel manages the state and presentation of console output, including error
    /// tracking, filtering options, and export functionality. It supports both raw and structured message modes,
    /// allowing users to view and interact with console data according to their preferences. The view model integrates
    /// with services for logging, parsing, and core communication, and exposes commands for clearing and exporting
    /// console logs. Thread safety is maintained for UI updates via SynchronizationContext. This class implements
    /// IDisposable to detach event handlers and release resources when no longer needed.</remarks>
    public partial class ConsoleViewModel : ViewModelBase, IDisposable
    {
        #region Fields & Dependencies
        private readonly ILogService _log;
        private readonly IConsoleParserService _parser;
        private readonly IGrblCoreService _coreService;
        private readonly SynchronizationContext? _syncContext;
        private bool _disposed;
        #endregion

        #region Collections
        public ObservableCollection<string> RawLines { get; } = new();
        public ObservableCollection<ConsoleItem> StructuredLines { get; } = new();
        public ObservableCollection<string> ErrorMessages { get; } = new();
        public ICollectionView RawView { get; }
        public ICollectionView StructuredView { get; }
        #endregion

        #region UI Properties
        [ObservableProperty]
        private bool _isRawMode = false;
        public bool IsStructuredMode => !IsRawMode;
        [ObservableProperty]
        private int _maxLines = 500;
        [ObservableProperty]
        private bool _isAutoScrollPaused;
        [ObservableProperty]
        private int _errorCount;
        [ObservableProperty]
        private string _lastErrorMessage = "No error";
        #endregion

        #region Filter Properties
        [ObservableProperty] private bool _showCommands = true;
        [ObservableProperty] private bool _showResponses = true;
        [ObservableProperty] private bool _showStatus = false;
        [ObservableProperty] private bool _showErrors = true;
        [ObservableProperty] private bool _showWarnings = true;
        [ObservableProperty] private bool _showSystem = true;
        [ObservableProperty] private bool _showJob = true;
        [ObservableProperty] private bool _showDebug = false;
        #endregion
        
        public ConsoleViewModel(
            IGrblCoreService coreService,
            ILogService log,
            IConsoleParserService parser)
        {
            _coreService = coreService;
            _parser = parser;
            _log = log;
            _syncContext = SynchronizationContext.Current;

            RawView = CollectionViewSource.GetDefaultView(RawLines);
            StructuredView = CollectionViewSource.GetDefaultView(StructuredLines);
            StructuredView.Filter = o => FilterItem((ConsoleItem)o);

            _coreService.DataReceived += OnDataReceived;
            _log.Information("[CONSOLE] Initialized");
        }



        #region Data Processing
        private void OnDataReceived(object? sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Text))
                return;

            string raw = e.Text.Trim();

            _syncContext?.Post(_ => AddRaw(raw), null);

            var item = _parser.ParseStructured(raw);
            if (item != null)
                _syncContext?.Post(_ => AddStructured(item), null);

            // Ensure UI updates happen on the main thread
            /*Application.Current.Dispatcher.BeginInvoke(() =>
            {
                AddRaw(raw);
                var item = _parser.ParseStructured(raw);
                if (item != null) AddStructured(item);
            });*/
        }

        private void AddRaw(string line)
        {
            RawLines.Add(line);

            if (!IsAutoScrollPaused && MaxLines > 0)
            {
                while (RawLines.Count > MaxLines)
                    RawLines.RemoveAt(0);
            }
        }

        private void AddStructured(ConsoleItem item)
        {
            StructuredLines.Add(item);

            if (item.Type == ConsoleMessageType.Error || item.Type == ConsoleMessageType.Alarm)
            {
                ErrorCount++;
                LastErrorMessage = item.Message;
                ErrorMessages.Add($"{item.Timestamp:HH:mm:ss} - {item.Message}");
            }

            EnforceVisibleLimit();
        }

        private bool FilterItem(ConsoleItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Message) && string.IsNullOrWhiteSpace(item.Response)) 
                return false;

            return item.Type switch
            {
                ConsoleMessageType.Command => ShowCommands,
                ConsoleMessageType.Response => ShowResponses,
                ConsoleMessageType.Status => ShowStatus,
                ConsoleMessageType.Error => ShowErrors,
                ConsoleMessageType.Warning => ShowWarnings,
                ConsoleMessageType.System => ShowSystem,
                ConsoleMessageType.Job => ShowJob,
                ConsoleMessageType.Debug => ShowDebug,
                ConsoleMessageType.Raw => false,
                _ => true
            };
        }
        private void EnforceVisibleLimit()
        {
            if (MaxLines <= 0)
                return;

            var visible = StructuredLines.Where(item => FilterItem(item)).ToList();

            int overflow = visible.Count - MaxLines;
            if (overflow <= 0)
                return;

            for (int i = 0; i < overflow; i++)
            {
                StructuredLines.Remove(visible[i]);
            }
        }
        #endregion
        partial void OnShowCommandsChanged(bool value) => StructuredView.Refresh();
        partial void OnShowResponsesChanged(bool value) => StructuredView.Refresh();
        partial void OnShowStatusChanged(bool value) => StructuredView.Refresh();
        partial void OnShowErrorsChanged(bool value) => StructuredView.Refresh();
        partial void OnShowWarningsChanged(bool value) => StructuredView.Refresh();
        partial void OnShowSystemChanged(bool value) => StructuredView.Refresh();
        partial void OnShowJobChanged(bool value) => StructuredView.Refresh();
        partial void OnShowDebugChanged(bool value) => StructuredView.Refresh();

        #region commands
        [RelayCommand]
        private void ClearConsole()
        {
            LogContextual(_log, "ClearConsole", "User cleared console history");
            RawLines.Clear();
            StructuredLines.Clear();
            ErrorCount = 0;
            LastErrorMessage = "No error";
            IsAutoScrollPaused = false;
        }

        [RelayCommand]
        private void ExportConsole()
        {
            LogContextual(_log, "ExportConsole", "Opening save dialog");
            var dialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
                FileName = $"VDLaser_Console_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (dialog.ShowDialog() != true)
            {
                _log.Debug("[CONSOLE] Export cancelled by user.");
                return;
            }

            try
            {
                var sb = new StringBuilder();

                if (IsRawMode)
                {
                    foreach (var line in RawLines)
                        sb.AppendLine(line);
                }
                else
                {
                    foreach (var item in StructuredLines)
                    {
                        if(item.Type!= ConsoleMessageType.Status)
                        sb.AppendLine(
                            $"[{item.Timestamp:HH:mm:ss}] [{item.Type}] {item.ToExportString()}"
                        );
                    }
                }

                File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);

                _log.Information(
                    "[CONSOLE] Successfully exported {LineCount} lines to {Path}",
                    IsRawMode ? RawLines.Count : StructuredLines.Count,
                    dialog.FileName
                );

                // Ajout d’un message système dans la console structurée
                StructuredLines.Add(new ConsoleItem(
                    $"Console exported to {System.IO.Path.GetFileName(dialog.FileName)}",
                    ConsoleMessageType.System
                ));
            }
            catch (Exception ex)
            {
                _log.Error("[CONSOLE] Failed to export logs to file:", ex);

                StructuredLines.Add(new ConsoleItem(
                    "Failed to export console logs.",
                    ConsoleMessageType.Error
                ));
            }
        }
        #endregion

        #region Helpers & Job Lifecycle
        public void ResetErrorsForJob()
        {
            ErrorCount = 0;
            LastErrorMessage = "No errors";
        }
        public void BeginJob()
        {
            AddStructured(new ConsoleItem("Job started", ConsoleMessageType.Job));
            ResetErrorsForJob();
        }

        public void EndJob(bool success)
        {
            AddStructured(new ConsoleItem(success ? "Job finished: Success" : "Job finished: Failed", ConsoleMessageType.Job));
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) _coreService.DataReceived -= OnDataReceived;
                _disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
