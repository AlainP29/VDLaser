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
        #region public Properties
        /// <summary>
        /// Title of the groupbox serial port setting
        /// </summary>
        public string SerialPortSettingHeader { get; private set; }

        /// <summary>
        /// The name of the serial port
        /// </summary>
        public string PortName { get; private set; }

        /// <summary>
        /// The integer value of the baud rate
        /// </summary>
        public int BaudRateValue { get; private set; }

        /// <summary>
        /// The baud rate displayed name
        /// </summary>
        public string BaudRateName { get; private set; }
        #endregion

        #region Constructors
        public SerialPortSettingModel(string serialPortSettingHeader)
        {
            SerialPortSettingHeader = serialPortSettingHeader;
        }
        #endregion
    }
}