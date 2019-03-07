using System.Collections.Generic;
using System.IO.Ports;


namespace VDGrbl.Model
{
    public class SerialPortSettingsModel
    {
        #region public Properties
        public string PortSettingsHeader { get; private set; }
        public string PortName { get; private set; }
        public int BaudRateValue { get; private set; }
        public string BaudRateName { get; private set; }
        public string[] ListPortNames = SerialPort.GetPortNames();
        public int[] ListBaudRates = {1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400 };
        #endregion

        #region Constructors
        public SerialPortSettingsModel()
        {
        }

        public SerialPortSettingsModel(string portSettingsHeader)
        {
            PortSettingsHeader = portSettingsHeader;
        }
        #endregion
    }
}