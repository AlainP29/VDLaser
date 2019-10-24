using System.Collections.Generic;

namespace VDGrbl.Model
{
    public class SerialPortSettingItems
    {
        public string SerialPortSettingHeader { get; private set; }
        public string PortName { get; private set; }
        public List<string> ListPortNames { get; private set; }
        public int BaudRate { get; private set; }
        public List<string> ListBaudRates { get; private set; }


        public SerialPortSettingItems()
        {

        }
        public SerialPortSettingItems(string serialPortSettingHeader)
        {
            SerialPortSettingHeader = serialPortSettingHeader;
        }
    }
}