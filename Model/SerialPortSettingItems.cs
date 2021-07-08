using System.Collections.Generic;

namespace VDLaser.Model
{
    public class SerialPortSettingItems
    {
        public string PortName { get; private set; }
        public List<string> ListPortNames { get; private set; }
        public int BaudRate { get; private set; }
        public List<int> ListBaudRates { get; private set; }
        public SerialPortSettingItems()
        {

        }
    }
}