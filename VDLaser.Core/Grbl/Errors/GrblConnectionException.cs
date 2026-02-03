namespace VDLaser.Core.Grbl.Errors
{
    public class GrblConnectionException : Exception
    {
        public GrblConnectionError Error { get; }

        public GrblConnectionException(
            GrblConnectionError error,
            string message,
            Exception? inner = null)
            : base(message, inner)
        {
            Error = error;
        }
    }
}
