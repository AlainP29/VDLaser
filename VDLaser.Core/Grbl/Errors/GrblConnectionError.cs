namespace VDLaser.Core.Grbl.Errors
{
    public enum GrblConnectionError
    {
        PortNotDefined,
        PortNotAvailable,
        PortBusy,
        NoResponse,
        NotAGrblDevice,
        Unknown
    }
}
