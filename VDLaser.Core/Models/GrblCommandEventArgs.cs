namespace VDLaser.Core.Models;

public class GrblCommandEventArgs : EventArgs
{
    public string Command { get; }
    public string? Source { get; }
    public string? Response { get; }
    public int ErrorCode { get; set; } = -1;

    public GrblCommandEventArgs(string command, string? source, string? response = null)
    {
        Command = command;
        Source = source;
        Response = response;
    }
}