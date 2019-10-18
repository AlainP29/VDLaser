namespace VDGrbl.Model
{
    public class CommandModel
    {
        public string CommandHeader { get; private set; }
        public CommandModel(string commandHeader)
        {
            CommandHeader = commandHeader;
        }
    }
}
