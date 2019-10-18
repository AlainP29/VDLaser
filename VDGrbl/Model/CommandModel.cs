namespace VDGrbl.Model
{
    public class CommandModel
    {
        public string CommandHeader { get; private set; } = "Command";
        public CommandModel(string commandHeader)
        {
            CommandHeader += commandHeader;
        }
    }
}
