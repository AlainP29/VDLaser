namespace VDLaser.Core.Gcode
{
    public enum GcodeErrorHandlingMode
    {
        Strict,     // Toute erreur arrête le job
        Tolerant    // Certaines erreurs sont ignorées
    }
}
