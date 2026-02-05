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
    public partial class ConsoleViewModel : ViewModelBase, IDisposable
    {
        private readonly ILogService _log;
        private readonly IConsoleParserService _parser;
        private readonly IGrblCoreService _coreService;
        private readonly SynchronizationContext? _syncContext;
        private bool _disposed;

        // ============================================================
        //  COLLECTIONS
        // ============================================================

        public ObservableCollection<string> RawLines { get; } = new();
        public ObservableCollection<ConsoleItem> StructuredLines { get; } = new();
        public ObservableCollection<string> ErrorMessages { get; } = new();
        public ICollectionView RawView { get; }
        public ICollectionView StructuredView { get; }

        // ============================================================
        //  MODES
        // ============================================================

        [ObservableProperty]
        private bool _isRawMode = false;

        public bool IsStructuredMode => !IsRawMode;

        // ============================================================
        //  PARAMÈTRES
        // ============================================================

        [ObservableProperty]
        private int _maxLines = 500;

        [ObservableProperty]
        private bool _isAutoScrollPaused;

        // ============================================================
        //  FILTRES CHECKBOX
        // ============================================================

        [ObservableProperty] private bool _showCommands = true;
        [ObservableProperty] private bool _showResponses = true;
        [ObservableProperty] private bool _showStatus = false;
        [ObservableProperty] private bool _showErrors = true;
        [ObservableProperty] private bool _showWarnings = true;
        [ObservableProperty] private bool _showSystem = true;
        [ObservableProperty] private bool _showJob = true;
        [ObservableProperty] private bool _showDebug = false;

        // ============================================================
        //  ERREURS
        // ============================================================

        [ObservableProperty]
        private int _errorCount;

        [ObservableProperty]
        private string _lastErrorMessage = "Aucune erreur";

        // ============================================================
        //  CONSTRUCTEUR
        // ============================================================

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
        }

        // ============================================================
        //  DISPOSE
        // ============================================================

        public void Dispose()
        {
            if (_disposed)
                return;

            _coreService.DataReceived -= OnDataReceived;
            _disposed = true;
        }

        // ============================================================
        //  RÉCEPTION DES DONNÉES
        // ============================================================

        private void OnDataReceived(object? sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Text))
                return;

            string raw = e.Text.Trim();

            _syncContext?.Post(_ => AddRaw(raw), null);

            var item = _parser.ParseStructured(raw);
            if (item != null)
                _syncContext?.Post(_ => AddStructured(item), null);
        }

        // ============================================================
        //  AJOUT DES LIGNES
        // ============================================================

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

        // ============================================================
        //  FILTRAGE STRUCTURÉ
        // ============================================================

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

            // On récupère les lignes visibles selon le filtre
            var visible = StructuredLines.Where(item => FilterItem(item)).ToList();

            int overflow = visible.Count - MaxLines;
            if (overflow <= 0)
                return;

            // On supprime les plus anciennes visibles
            for (int i = 0; i < overflow; i++)
            {
                StructuredLines.Remove(visible[i]);
            }
        }

        partial void OnShowCommandsChanged(bool value) => StructuredView.Refresh();
        partial void OnShowResponsesChanged(bool value) => StructuredView.Refresh();
        partial void OnShowStatusChanged(bool value) => StructuredView.Refresh();
        partial void OnShowErrorsChanged(bool value) => StructuredView.Refresh();
        partial void OnShowWarningsChanged(bool value) => StructuredView.Refresh();
        partial void OnShowSystemChanged(bool value) => StructuredView.Refresh();
        partial void OnShowJobChanged(bool value) => StructuredView.Refresh();
        partial void OnShowDebugChanged(bool value) => StructuredView.Refresh();

        // ============================================================
        //  COMMANDES
        // ============================================================

        [RelayCommand]
        private void ClearConsole()
        {
            RawLines.Clear();
            StructuredLines.Clear();
            ErrorCount = 0;
            LastErrorMessage = "Aucune erreur";
            IsAutoScrollPaused = false;
        }

        [RelayCommand]
        private void ExportConsole()
        {
            _log.Information("[Console] Exporting logs...");

            var dialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
                FileName = $"VDLaser_Console_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (dialog.ShowDialog() != true)
            {
                _log.Debug("[ConsoleViewModel] Export cancelled by user.");
                return;
            }

            try
            {
                var sb = new StringBuilder();

                if (IsRawMode)
                {
                    // Export RAW
                    foreach (var line in RawLines)
                        sb.AppendLine(line);
                }
                else
                {
                    // Export STRUCTURÉ
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
                    "[ConsoleViewModel] Successfully exported {LineCount} lines to {Path}",
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
                _log.Error("[ConsoleViewModel] Failed to export logs to file:", ex);

                StructuredLines.Add(new ConsoleItem(
                    "Failed to export console logs.",
                    ConsoleMessageType.Error
                ));
            }
        }

        public void ResetErrorsForJob()
        {
            ErrorCount = 0;
            LastErrorMessage = "Aucune erreur";
        }
        public void BeginJob()
        {
            AddStructured(new ConsoleItem("Début du job de gravure", ConsoleMessageType.Job));
            ResetErrorsForJob();
        }

        public void EndJob(bool success)
        {
            AddStructured(new ConsoleItem(success ? "Fin du job : Succès" : "Fin du job : Avec erreurs", ConsoleMessageType.Job));
        }
    }

}
