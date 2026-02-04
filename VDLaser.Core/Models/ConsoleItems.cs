
using CommunityToolkit.Mvvm.ComponentModel;
using VDLaser.Core.Grbl.Models;

namespace VDLaser.Core.Console
{
    /// <summary>
    /// Console message types.
    /// </summary>
    public enum ConsoleMessageType
    {
        Command,
        Response,
        Status,
        Job,
        Info,
        Warning,
        Error,
        Alarm,
        Success,
        System,
        Debug,
        Raw
    }
    public enum ConsoleSource
    { 
        Manual, 
        Job, 
        System, 
        Internal 
    }
    /// <summary>
    /// A single item is displayed in the VDLaser console.
    /// </summary>
    public partial class ConsoleItem: ObservableObject
    {
        #region Properties
        /// <summary>
        /// Message Timestamp (HH:mm:ss)
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public ConsoleSource Source { get; set; } = ConsoleSource.Manual; // valeur par défaut

        /// <summary>
        /// Gets or sets the message associated with this instance.
        /// </summary>
        [ObservableProperty]
        private string _message = "-";
        /// <summary>
        /// Gets or sets the command associated with this instance.
        /// </summary>
        [ObservableProperty]
        private string _command = string.Empty;
        /// <summary>
        /// Gets or sets the response associated with this instance.
        /// </summary>
        [ObservableProperty]
        private string _response = string.Empty;
        /// <summary>
        /// Type of message (Info, Error, Alarm, etc.)
        /// </summary>
        [ObservableProperty]
        private ConsoleMessageType _type = ConsoleMessageType.Info;
        /// <summary>
        /// Raw line as received from the serial port (RAW mode).
        /// </summary>
        [ObservableProperty]
        private string _rawText = string.Empty;
        public bool IsRaw => Type == ConsoleMessageType.Raw;

        public AlarmSeverity? Severity { get; set; }

        /// <summary>
        /// GRBL code (error/alarm).
        /// </summary>
        public int? Code { get; set; }

        /// <summary>
        /// GRBL description.
        /// </summary>
        public string? Description { get; set; }
        #endregion

        #region Identifiers
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? ParentId { get; set; }
        #endregion

        #region Constructors
        public ConsoleItem() { }
        public ConsoleItem(string command)
        { 
            Command = command; 
            Type = ConsoleMessageType.Command; 
        }

        public ConsoleItem(string rawLine, bool isRaw)
        {
            RawText = rawLine;
            Message = rawLine;
            Type = ConsoleMessageType.Raw;
        }

        public ConsoleItem(string message, ConsoleMessageType type)
        {
            Message = message;
            Type = type;
        }

        public ConsoleItem(string command, string response)
        { 
            Command = command; 
            Response = response; 
            Type = ConsoleMessageType.Response; 
        }

        public ConsoleItem(int code, string message, string? description, ConsoleMessageType type)
        {
            Code = code;
            Response = message;
            Description = description;
            Type = type;
        }
        #endregion

        #region Factory Methods
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

            return Response;
        }
        #endregion
    }
}
