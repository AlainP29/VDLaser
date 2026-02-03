using System.Diagnostics;
using System.IO.Ports;
using VDLaser.Core.Interfaces;

namespace VDLaser.Core.Models
{
    public partial class SerialPortSettingItems
    {
        public string PortName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 115200;
        public string Parity { get; set; } = string.Empty;
        public int DataBits { get; set; } = 8;
        public int StopBits { get; set; } = 1;
        public string Handshake { get; set; } = string.Empty;
        public int ReadTimeout { get; set; } = 500;
        public int WriteTimeout { get; set; } = 500;
        public List<string> ListPortNames { get; private set; } = new List<string>();
        public List<int> ListBaudRates { get; set; } = new List<int>()
            {
            1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400  // Valeurs courantes pour Arduino/GRBL
            };

        public event EventHandler SettingsChanged;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

    }
}