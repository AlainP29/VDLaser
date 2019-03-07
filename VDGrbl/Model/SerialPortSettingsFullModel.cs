using System.Collections.Generic;
using System.IO.Ports;


namespace VDGrbl.Model
{
    public class SerialPortSettingsFullModel
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
        public List<SerialPortSettingsFullModel> ListParities()
        {
            List<SerialPortSettingsFullModel> parities = new List<SerialPortSettingsFullModel>
            {
                new SerialPortSettingsFullModel() { ParityName = "Even", ParityValue = Parity.Even },
                new SerialPortSettingsFullModel() { ParityName = "Mark", ParityValue = Parity.Mark },
                new SerialPortSettingsFullModel() { ParityName = "None", ParityValue = Parity.None },
                new SerialPortSettingsFullModel() { ParityName = "Odd", ParityValue = Parity.Odd },
                new SerialPortSettingsFullModel() { ParityName = "Space", ParityValue = Parity.Space }
            };
            return parities;
        }
        public List<SerialPortSettingsFullModel> ListStopBits()
        {
            List<SerialPortSettingsFullModel> sp = new List<SerialPortSettingsFullModel>
            {
                new SerialPortSettingsFullModel() { StopBitsName = "None", StopBitsValue = StopBits.None },
                new SerialPortSettingsFullModel() { StopBitsName = "1", StopBitsValue = StopBits.One },
                new SerialPortSettingsFullModel() { StopBitsName = "1.5", StopBitsValue = StopBits.OnePointFive },
                new SerialPortSettingsFullModel() { StopBitsName = "2", StopBitsValue = StopBits.Two }
            };

            return sp;
        }
        public List<SerialPortSettingsFullModel> ListHandshake()
        {
            List<SerialPortSettingsFullModel> hs = new List<SerialPortSettingsFullModel>
            {
                new SerialPortSettingsFullModel() { HandshakingName = "None", HandshakingValue = Handshake.None },
                new SerialPortSettingsFullModel() { HandshakingName = "RequestToSend", HandshakingValue = Handshake.RequestToSend },
                new SerialPortSettingsFullModel() { HandshakingName = "RequestToSendXOnXOff", HandshakingValue = Handshake.RequestToSendXOnXOff },
                new SerialPortSettingsFullModel() { HandshakingName = "XOnXOff", HandshakingValue = Handshake.XOnXOff }
            };
            return hs;
        }
        #endregion

        #region Constructors
        public SerialPortSettingsFullModel()
        {
        }

        public SerialPortSettingsFullModel(string portSettingsHeader)
        {
            PortSettingsHeader = portSettingsHeader;
        }
        #endregion
    }
}