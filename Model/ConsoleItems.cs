
namespace VDLaser.Model
{
    public class ConsoleItems
    {
        public string RXData { get; private set; }
        public string TXData { get; private set; }

        public ConsoleItems()
        {

        }

        public ConsoleItems(string txData, string rxData)
        {
            TXData = txData;
            RXData = rxData;
        }
    }
}
