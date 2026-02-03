using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows.Shapes;
using VDLaser.Core.Codes;
using VDLaser.Core.Console;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;
using VDLaser.ViewModels.Base;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Gère l'affichage des messages série, le filtrage des types de messages GRBL 
    /// et l'exportation des logs de communication.
    /// </summary>
    public partial class ConsoleViewModel : ViewModelBase
    {
        #region Fields & Properties
        private readonly ILogService _log;
        private readonly SynchronizationContext? _syncContext;
        private readonly IGrblCoreService _coreService;
        private readonly IStatusPollingService _polling;
        private readonly IGcodeJobService _gcodeJobService;
        [ObservableProperty]
        private ObservableCollection<ConsoleItem> _lines = new ObservableCollection<ConsoleItem>();
        public ICollectionView FilteredLines { get; }

        [ObservableProperty]
        private bool _onlyShowErrors = false;
        [ObservableProperty]
        private bool _isAutoScrollPaused;

        [ObservableProperty]
        private bool _isVerbose = false;

        [ObservableProperty]
        private int _maxLines = 200;

        [ObservableProperty]
        private bool _hideOkMessages = false;

        [ObservableProperty]
        private bool _hideStatusReports = true;
        [ObservableProperty]
        private string _lastResponse = "Ready";
        [ObservableProperty]
        private bool _hasJobErrors = false;
        //public int LineCount=>Lines.Count;
        [ObservableProperty]
        private int _errorCount=0;
        public bool AreFiltersEnabled => !IsVerbose;
        private readonly ErrorCodes _errorCodes = new();
        private readonly AlarmCodes _alarmCodes = new();
        #endregion

        public ConsoleViewModel(IGrblCoreService coreService, ILogService log, IStatusPollingService polling,IGcodeJobService gcodeJobService)
        {
            _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _polling = polling ?? throw new ArgumentNullException(nameof(polling));
            _gcodeJobService = gcodeJobService ?? throw new ArgumentNullException(nameof(gcodeJobService));
            
            _syncContext = SynchronizationContext.Current;
            _coreService.DataReceived += OnDataReceived;
            _coreService.StatusUpdated += OnMachineStatusUpdated;

            // Dans le constructeur de ConsoleViewModel.cs
            FilteredLines = CollectionViewSource.GetDefaultView(Lines);
            FilteredLines.Filter = obj =>
            {
                if (obj is ConsoleItem item)
                {
                    // On ne retourne "true" que pour les types critiques
                    return item.Type == ConsoleMessageType.Error ||
                           item.Type == ConsoleMessageType.Alarm ||
                           item.Type == ConsoleMessageType.Warning;
                }
                return false;
            };

            _log.Debug("[ConsoleViewModel] Initialized. SyncContext captured: {IsPresent}", _syncContext != null);
            AddSystem("Console initialized");
        }

        #region Logic : Data Parsing & Events
        /// <summary>
        /// Analyse les chaînes brutes reçues du contrôleur GRBL pour les catégoriser.
        /// </summary>
        private void OnDataReceived(object? sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Text)) return;

            var line = e.Text.Trim();

            // --- CAS 1 : ENVOI D'UNE COMMANDE (Signalé par >>) ---
            if (line.StartsWith(">>"))
            {
                var cmd = line.Substring(2).Trim();
                //if (IsVerbose || !cmd.StartsWith("G", StringComparison.OrdinalIgnoreCase))
                    if (IsVerbose || !cmd.StartsWith("?", StringComparison.OrdinalIgnoreCase))
                {
                    Add(new ConsoleItem { Command = cmd, Type = ConsoleMessageType.Info });
                }
                return;
            }

            // --- CAS 2 : RÉPONSE À UNE COMMANDE ---
            // On cherche le dernier item qui a une commande mais pas encore de réponse
            var lastPendingItem = Lines.LastOrDefault(x => !string.IsNullOrEmpty(x.Command)
                                                        && string.IsNullOrEmpty(x.Response));
            if (line.Equals("ok", StringComparison.OrdinalIgnoreCase))
            {
                HandleOkResponse();
            }
            else if (line.StartsWith("error:", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(line.Split(':')[1], out int code))
                    AddGrblError(code);
            }
            else if (line.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(line.Split(':')[1], out int code))
                    AddGrblAlarm(code);
            }
            else if (line.StartsWith("<") && line.EndsWith(">"))
            {
                // On vérifie si l'utilisateur veut voir les rapports d'état (ou si mode Verbose)
                if (ShouldDisplay(line, ConsoleMessageType.System))
                {
                    // On crée une ligne spécifique "Status" qui ne se couple pas
                    // pour ne pas interférer avec le dernier 'ok' attendu
                    Add(new ConsoleItem
                    {
                        Command = "Status",
                        Response = line,
                        Type = ConsoleMessageType.System
                    });
                }
            }

        }
        partial void OnIsVerboseChanged(bool value)
        {
            OnPropertyChanged(nameof(AreFiltersEnabled));
        }
        private void OnMachineStatusUpdated(object? sender, EventArgs e)
        {
            // On vérifie si la machine vient de passer en état Alarm
            if (_coreService.State.MachineState == MachState.Alarm)
            {
                //_log.Fatal("[Console] État ALARME détecté. Annulation des commandes en attente.");

                // IMPORTANT : Exécuter sur le thread UI pour modifier la collection
                _syncContext?.Post(_ =>
                {
                    // 1. On récupère toutes les commandes qui attendent un "ok" (Type Info/Bleu)
                    var pendingCommands = Lines.Where(x => x.Type == ConsoleMessageType.Info && string.IsNullOrEmpty(x.Response)).ToList();

                    foreach (var item in pendingCommands)
                    {
                        item.Response = "STOPPED";
                        item.Description = "Commande annulée suite à une alarme machine.";
                        item.Type = ConsoleMessageType.Warning; // Change la couleur (ex: Orange/Jaune)
                    }

                    // 2. Optionnel : Vider la file d'attente si vous avez accès à _commandQueue ici
                    // Sinon, c'est le GcodeJobService qui doit s'en charger.
                }, null);
            }
        }
        partial void OnOnlyShowErrorsChanged(bool value)
        {
            FilteredLines.Refresh();
        }
        #endregion

        // ---------------------------------------------------------
        // Messages simples
        // ---------------------------------------------------------
        public void AddInfo(string msg) => Add(msg, ConsoleMessageType.Info);
        public void AddWarning(string msg) => Add(msg, ConsoleMessageType.Warning);
        public void AddSuccess(string msg) => Add(msg, ConsoleMessageType.Success);
        public void AddSystem(string msg) => Add(msg, ConsoleMessageType.System);
        public void AddError(string msg) => Add(msg, ConsoleMessageType.Error);
        public void AddAlarm(string msg) => Add(msg, ConsoleMessageType.Alarm);

        // ---------------------------------------------------------
        // GRBL : erreurs / alarmes structurées
        // ---------------------------------------------------------
        public void AddGrblError(int code, string description)
        {
            ErrorCount++;
            Add(new ConsoleItem
            {
                Code = code,
                Message = $"Error {code}: {description}",
                Description = description,
                Type = ConsoleMessageType.Error
            });
        }
        public void AddGrblError(int code)
        {
            ErrorCount++;
            HasJobErrors = true;
            var errorInfo = _errorCodes.GetError(code, isVersion11: true);
            string description = errorInfo?.Description ?? "Unknown GRBL error";
            string message = $"Error {code}: {errorInfo?.Message ?? "Error"}";

            var lastItem = Lines.LastOrDefault(x => !string.IsNullOrEmpty(x.Command)
                                                 && string.IsNullOrEmpty(x.Response));

            if (lastItem != null)
            {
                lastItem.Response = $"error:{code}";
                lastItem.Description = description;
                //lastItem.Type = ConsoleMessageType.Error;
                lastItem.Type = ConsoleMessageType.Warning;
                
            }
            else
            {
                Add(new ConsoleItem(code, message, description, ConsoleMessageType.Warning));
                
            }
            LastResponse = "⚠️ Gravure effectuée avec erreurs";
            _polling.ForcePoll();
        }
        public void AddGrblAlarm(int code, string description)
        {
            Add(new ConsoleItem
            {
                Code = code,
                Message = $"Alarm {code}: {description}",
                Description = description,
                Type = ConsoleMessageType.Alarm
            });
        }
        public void AddGrblAlarm(int code)
        {
            var alarmInfo = _alarmCodes.GetAlarm(code, isVersion11: true);
            string description = alarmInfo?.Message ?? "Unknown Alarm";
            var lastItem = Lines.LastOrDefault(x => !string.IsNullOrEmpty(x.Command)
                                                 && string.IsNullOrEmpty(x.Response));

            if (lastItem != null)
            {
                lastItem.Response = $"ALARM:{code}";
                lastItem.Description = description;
                lastItem.Type = ConsoleMessageType.Alarm;
                LastResponse = description;
            }
            else
            {
                Add(new ConsoleItem(code, $"ALARM:{code}", description, ConsoleMessageType.Alarm));
                LastResponse = description;
            }
            _polling.ForcePoll();
        }
        private void HandleOkResponse()
        {
            var lastItem = Lines.LastOrDefault(x => !string.IsNullOrEmpty(x.Command)
                                                 && string.IsNullOrEmpty(x.Response));

            if (lastItem != null)
            {
                lastItem.Response = "ok";
                lastItem.Type = ConsoleMessageType.Success;

                // Si on doit cacher les OK et qu'on n'est pas en mode verbeux
                if (HideOkMessages && !IsVerbose)
                {
                    _syncContext?.Post(_ => Lines.Remove(lastItem), null);
                }
            }
        }
        /// <summary>
        /// Détermine si un message doit être affiché dans la console en fonction des filtres actifs.
        /// Priorité : IsVerbose > HideOkMessages > HideStatusReports.
        /// </summary>
        private bool ShouldDisplay(string text, ConsoleMessageType type)
        {
            // Si Verbose est actif, on affiche tout (y compris les status et les ok)
            if (IsVerbose) return true;

            // Filtre pour les messages "ok"
            if (HideOkMessages && type == ConsoleMessageType.Success)
                return false;

            // Filtre pour les rapports d'état (Status Reports)
            if (HideStatusReports && type == ConsoleMessageType.System)
                return false;

            return true;
        }
        #region Logic : Buffer Management
        /// <summary>
        /// Ajoute un élément à la collection en s'assurant d'être sur le thread UI.
        /// </summary>
        public void Add(ConsoleItem item)
        {
            if (_syncContext != null)
            {
                _syncContext.Post(_ => AddInternal(item), null);
            }
            else
            {
                _log.Warning("[ConsoleViewModel] No SynchronizationContext. Adding item on current thread.");
                AddInternal(item);
            }
        }
        public void Add(string message, ConsoleMessageType type)
        {
            Add(new ConsoleItem(message, type));
        }



        private void AddInternal(ConsoleItem item)
        {
            try
            {
                Lines.Add(item);

                // Gestion de la mémoire: limite le nombre de lignes
                if (MaxLines <= 0)
                    return;

                if (!IsAutoScrollPaused)
                {
                    while (Lines.Count > MaxLines)
                    {
                        Lines.RemoveAt(0);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("[ConsoleViewModel] Failed to add item to ObservableCollection.");
            }
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void ClearConsole()
        {
            _log.Information("[ConsoleViewModel] Manual clear requested.");
            if (_syncContext != null)
            {
                _syncContext.Post(_ => ClearInternal(), null);
            }
            else
            {
                ClearInternal();
            }
            ErrorCount = 0;
        }
        private void ClearInternal()
        {
            Lines.Clear();
            IsAutoScrollPaused = false;
            AddSystem("Console cleared");
            LastResponse = "Ready";
        }
        [RelayCommand]
        private void ExportConsole()
        {
            _log.Information("[Console] Exporting logs... Including filtered items if present in buffer.");
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

                foreach (var line in Lines)
                {
                     sb.AppendLine($"[{line.Timestamp:HH:mm:ss}] [{line.Type}] {line.ToExportString()}");
                }

                File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                _log.Information("[ConsoleViewModel] Successfully exported {LineCount} lines to {Path}", Lines.Count, dialog.FileName);
                AddSystem($"Console exported to {System.IO.Path.GetFileName(dialog.FileName)}");
            }
            catch (Exception ex)
            {
                _log.Error("[ConsoleViewModel] Failed to export logs to file.");
                AddError("Failed to export console logs.");
            }


        }
        #endregion
        public void ResetJobErrorState()
        {
            HasJobErrors = false;
        }
        public void BeginJob()
        {
            HasJobErrors = false;
            LastResponse = "Gravure en cours...";
        }
        public void EndJob()
        {
            if (HasJobErrors)
            {
                AddWarning("Gravure effectuée avec erreurs");
                LastResponse = "⚠️ Gravure effectuée avec erreurs";
            }
            else
            {
                AddSuccess("Gravure effectuée sans erreur");
                LastResponse = "✅ Gravure effectuée sans erreur";
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _log.Debug("[ConsoleViewModel] Disposing ConsoleViewModel. Unhooking DataReceived.");
                _coreService.DataReceived -= OnDataReceived;
                _coreService.StatusUpdated -= OnMachineStatusUpdated;
            }
            base.Dispose(disposing);
        }
    }
}
