namespace VDGrbl.Model
{
    public class SerialPortSettingModel
    {
        public string SerialPortSettingHeader { get; private set; }
        public string PortName { get; private set; }
        public int BaudRate { get; private set; }

        public SerialPortSettingModel()
        {

        }
        public SerialPortSettingModel(string serialPortSettingHeader)
        {
            SerialPortSettingHeader = serialPortSettingHeader;
        }
    }
}