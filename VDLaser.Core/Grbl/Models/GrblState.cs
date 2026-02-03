using System;
using System.Collections.Generic;
using System.Windows.Media;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Models;

namespace VDLaser.Core.Grbl.Models
{
    /// <summary>
    /// État complet du firmware GRBL.
    /// Centralise le status, les positions, les erreurs, les alarmes
    /// et maintenant les paramètres ($0 → $132 ; $140 → $142).
    /// </summary>
    public class GrblState
    {
        public enum MachState { Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep, Undefined };

        public MachState MachineState = MachState.Undefined;
        public SolidColorBrush MachineStatusColor { get; set; } = Brushes.DarkGray;

        public bool IsGrbl11 { get; set; } = false;
        public double MachinePosX { get; set; } = 0.0;
        public double MachinePosY { get; set; } = 0.0;
        public double MachinePosZ { get; set; } = 0.0;
        public double WorkPosX { get; set; } = 0.0;
        public double WorkPosY { get; set; } = 0.0;
        public double WorkPosZ { get; set; } = 0.0;
        public string OffsetPosY { get;  set; } = "0";
        public string OffsetPosX { get;  set; } = "0";
        public string OffsetPosZ { get; set; } = "0";
        public string MachineFeed { get;  set; } = "0";
        public string MachineSpeed { get;  set; } = "0";
        public string OverrideMachineFeed { get;  set; } = "0";
        public string OverrideMachineSpeed { get;  set; } = "0";
        public int PlannerBuffer { get; set; } = 0;
        public int RxBuffer { get; set; } = 0;
        public string FeedRate { get; set; } = "0";
        public string SpindleSpeed { get; set; } = "0";
        public string PowerLaser { get; set; } = "0";

        /// <summary>
        /// Dernier message textuel non structuré reçu.
        /// </summary>
        public string LastMessage { get; set; } = string.Empty;

        /// <summary>
        /// True si la machine est en exécution d’un job.
        /// </summary>
        public bool IsRunningJob => MachineState == MachState.Run;

        /// <summary>
        /// True si la machine est en état d’alarme.
        /// </summary>
        public bool IsInAlarm => MachineState == MachState.Alarm;

        /// <summary>
        /// True si la porte est ouverte.
        /// </summary>
        public bool IsDoorOpen => MachineState == MachState.Door;

        /// <summary>
        /// Dernier message busy:xxx si reçu.
        /// </summary>
        public string LastBusyMessage { get; set; } = string.Empty;

        /// <summary>
        /// True si GRBL signale que les commandes sont suspendues/busy.
        /// </summary>
        public bool IsBusy { get; set; }

        /// <summary> Dernière alarme GRBL reçue (alarm:#). </summary>
        public GrblAlarm? Alarm { get; set; }

        /// <summary> Historique de toutes les alarmes reçues. </summary>
        public List<GrblAlarm> AlarmHistory { get; } = new();

        /// <summary> Message d’erreur si un parsing échoue. </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Paramètres GRBL déjà reçus (remplis via $$ ou $x=val).
        /// Clé = ID (ex: 100 pour $100).
        /// Valeur = GrblSetting complet (nom + desc + valeur + unité + commentaire).
        /// </summary>
        public Dictionary<int, GrblSetting> Settings { get; } = new();

        /// <summary>
        /// Dernier paramètre mis à jour par GrblSettingParser.
        /// </summary>
        public GrblSetting? LastParsedSetting { get; set; }

        /// <summary>
        /// Permet de réinitialiser l’état GRBL (mais pas les paramètres ni les erreurs => GCodeState).
        /// </summary>
        public void ResetStatus()
        {
            Alarm = null;
            LastMessage = string.Empty;
            LastBusyMessage = string.Empty;
            IsBusy = false;
        }

        /// <summary>
        /// Réinitialise tout l’état incluant les paramètres GRBL mais pas les erreurs=>GCodeState.
        /// </summary>
        public void ResetAll()
        {
            ResetStatus();
            Settings.Clear();
            LastParsedSetting = null;
        }
    }
}
