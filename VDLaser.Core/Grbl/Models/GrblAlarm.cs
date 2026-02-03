namespace VDLaser.Core.Grbl.Models
{
    /// <summary>
    /// Represents a parsed GRBL alarm with code, message, and severity level.
    /// </summary>
    public class GrblAlarm
    {
        /// <summary>
        /// GRBL alarm code (ex: 1, 2, 3…)
        /// </summary>
        public int Code { get; }
        /// <summary>
        /// Human-readable GRBL alarm message.
        /// </summary>
        public string Message { get; }
        // <summary>
        /// Severity of the alarm (Info, Warning, Critical).
        /// Used for UI coloring and log visibility.
        /// </summary>
        public AlarmSeverity Severity { get; }
        /// <summary>
        /// Optional timestamp to keep track of when this alarm occurred.
        /// Useful for history and logs.
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.Now;
        public GrblAlarm(int code, string message, AlarmSeverity severity = AlarmSeverity.Critical)
        {
            Code = code;
            Message = message;
            Severity = severity;
        }

        public override string ToString() => $"Alarm {Code}: {Message} {Severity}";
    }

    /// <summary>
    /// Defines the severity level of GRBL alarms.
    /// Critical = machine in unsafe or lost state
    /// Warning  = recoverable error
    /// Info     = minor or non-blocking
    /// </summary>
    public enum AlarmSeverity
    {
        Info,
        Warning,
        Critical
    }

}
