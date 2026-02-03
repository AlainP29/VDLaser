
using CommunityToolkit.Mvvm.ComponentModel;

namespace VDLaser.Core.Console
{
    public enum ConsoleMessageType
    {
        Info,
        Warning,
        Error,
        Alarm,
        Success,
        System
    }

    /// <summary>
    /// Élément unique affiché dans la console de VDLaser.
    /// </summary>
    public partial class ConsoleItem: ObservableObject
    {
        /// <summary>
        /// Timestamp du message (HH:mm:ss)
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Message affiché dans la console.
        /// </summary>
        [ObservableProperty]
        private string _message = "-";
        [ObservableProperty]
        private string _command = string.Empty;
        /// <summary>
        /// La réponse va changer de "" à "ok", donc elle doit être observable
        /// </summary>
        [ObservableProperty]
        private string _response = string.Empty;
        /// <summary>
        /// Type du message (Info, Error, Alarm, etc.), il peut changer (ex: passer de Info à Error si la réponse est une erreur)
        /// </summary>
        [ObservableProperty]
        private ConsoleMessageType _type = ConsoleMessageType.Info;

        /// <summary>
        /// Code GRBL (error/alarm), optionnel.
        /// </summary>
        public int? Code { get; set; }

        /// <summary>
        /// Description GRBL optionnelle.
        /// </summary>
        public string? Description { get; set; }

        public ConsoleItem() { }

        public ConsoleItem(string message, ConsoleMessageType type)
        {
            Message = message;
            Type = type;
        }

        public ConsoleItem(int code, string message, string? description, ConsoleMessageType type)
        {
            Code = code;
            Message = message;
            Response = message;
            Description = description;
            Type = type;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Message}";
        }
        public string ToExportString()
        {
            if (!string.IsNullOrEmpty(Command) && !string.IsNullOrEmpty(Response))
                return $"{Command} -> {Response}";

            if (!string.IsNullOrEmpty(Command))
                return $">> {Command}";

            if (!string.IsNullOrEmpty(Message))
                return Message;

            return string.Empty;
        }


    }
}
