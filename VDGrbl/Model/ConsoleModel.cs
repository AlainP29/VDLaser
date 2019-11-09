
namespace VDLaser.Model
{
    public class ConsoleModel
    {
        public string ConsoleHeader { get; private set; }
        public string RXData { get; private set; }
        public string TXData { get; private set; }

        public ConsoleModel(string consoleHeader)
        {
            ConsoleHeader = consoleHeader;
        }

        public ConsoleModel(string txData, string rxData)
        {
            TXData = txData;
            RXData = rxData;
        }
    }
}
