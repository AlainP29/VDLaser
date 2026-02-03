namespace VDLaser.ViewModels
{
    public enum MachineUiState
    {
        Disconnected,
        Connecting,
        Reconnecting,
        Connected,
        HomingRequired,
        Homing,
        Ready,
        EmergencyStop,
        Alarm
    }
}
