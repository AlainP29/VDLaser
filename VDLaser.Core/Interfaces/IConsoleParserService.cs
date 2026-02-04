using VDLaser.Core.Console;

namespace VDLaser.Core.Interfaces
{
    public interface IConsoleParserService
    {
        ConsoleItem ParseRaw(string rawLine);
        ConsoleItem ParseStructured(string rawLine);

        ConsoleItem CurrentPendingCommand { get; }

        void BeginCommand(string command);

        

}

}
