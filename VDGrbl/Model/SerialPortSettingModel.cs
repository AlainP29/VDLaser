using System.Collections.Generic;
using System.IO.Ports;
using GalaSoft.MvvmLight;

namespace VDGrbl.Model
{
    /// <summary>
    /// Serial port setting model class.
    /// </summary>
    public class SerialPortSettingModel
    {
        public string SerialPortSettingHeader { get; private set; }
        public string PortName { get; private set; }
        public int BaudRateValue { get; private set; }
        public string BaudRateName { get; private set; }
        public SerialPortSettingModel(string serialPortSettingHeader)
        {
            SerialPortSettingHeader = serialPortSettingHeader;
        }
    }
}