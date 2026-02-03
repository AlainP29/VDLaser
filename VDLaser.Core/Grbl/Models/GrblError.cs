namespace VDLaser.Core.Grbl.Models
{
    /// <summary>
    /// Represents a parsed GRBL error with code, message and description
    /// </summary>
    public class GrblError
    {
        /// <summary>
        /// GRBL alarm code (ex: 1, 2, 3…)
        /// </summary>
        public int Code { get; }
        /// Message court.
        /// </summary>
        public string Message { get; } 

        /// <summary>
        /// Description détaillée de l’erreur (issue de ErrorCodes).
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Timestamp de l’erreur pour logs/console.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public GrblError (int code,string message, string description)
        {
            Code = code;
            Message = message;
            Description = description;
        }

        public override string ToString()
        {
            return $"Error {Code}: {Message} {Description}";
        }
    }
}
