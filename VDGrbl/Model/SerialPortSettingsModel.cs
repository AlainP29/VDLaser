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
        public string ParityName { get; private set; }
        public Parity ParityValue { get; private set; }
        public StopBits StopBitsValue { get; private set; }
        public string StopBitsName { get; private set; }
        public int DataBitsValue { get; private set; }
        public Handshake HandshakingValue { get; private set; }
        public string HandshakingName { get; private set; }
        public string[] ListPortNames = SerialPort.GetPortNames();
        public int[] ListBaudRates = {1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400 };
        public int[] ListDataBits = { 5, 6, 7, 8, 9 };
        public List<SerialPortSettingsModel> ListParities()
        {
            List<SerialPortSettingsModel> parities = new List<SerialPortSettingsModel>
            {
                new SerialPortSettingsModel() { ParityName = "Even", ParityValue = Parity.Even },
                new SerialPortSettingsModel() { ParityName = "Mark", ParityValue = Parity.Mark },
                new SerialPortSettingsModel() { ParityName = "None", ParityValue = Parity.None },
                new SerialPortSettingsModel() { ParityName = "Odd", ParityValue = Parity.Odd },
                new SerialPortSettingsModel() { ParityName = "Space", ParityValue = Parity.Space }
            };
            return parities;
        }
        public List<SerialPortSettingsModel> ListStopBits()
        {
            List<SerialPortSettingsModel> sp = new List<SerialPortSettingsModel>
            {
                new SerialPortSettingsModel() { StopBitsName = "None", StopBitsValue = StopBits.None },
                new SerialPortSettingsModel() { StopBitsName = "1", StopBitsValue = StopBits.One },
                new SerialPortSettingsModel() { StopBitsName = "1.5", StopBitsValue = StopBits.OnePointFive },
                new SerialPortSettingsModel() { StopBitsName = "2", StopBitsValue = StopBits.Two }
            };

            return sp;
        }
        public List<SerialPortSettingsModel> ListHandshake()
        {
            List<SerialPortSettingsModel> hs = new List<SerialPortSettingsModel>
            {
                new SerialPortSettingsModel() { HandshakingName = "None", HandshakingValue = Handshake.None },
                new SerialPortSettingsModel() { HandshakingName = "RequestToSend", HandshakingValue = Handshake.RequestToSend },
                new SerialPortSettingsModel() { HandshakingName = "RequestToSendXOnXOff", HandshakingValue = Handshake.RequestToSendXOnXOff },
                new SerialPortSettingsModel() { HandshakingName = "XOnXOff", HandshakingValue = Handshake.XOnXOff }
            };
            return hs;
        }
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