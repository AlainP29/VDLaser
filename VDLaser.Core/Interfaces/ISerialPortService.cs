using System;
using System.Collections.Generic;
using System.IO.Ports;
using VDLaser.Core.Models;

namespace VDLaser.Core.Interfaces
{
    public interface ISerialPortService : IDisposable
    {
        // --- Configuration ---
        string PortName { get; set; }
        int BaudRate { get; set; }
        Parity Parity { get; set; }
        int DataBits { get; set; }
        StopBits StopBits { get; set; }
        Handshake Handshake { get; set; }
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }
        List<string> ListPortNames { get; }
        List<int> ListBaudRates { get; }
        bool IsPortAvailable(string portName);

        // --- Events ---
        event EventHandler SettingsChanged;
        event EventHandler<DataReceivedEventArgs> DataReceived;
        event EventHandler ConnectionLost;

        // --- Port management ---
        void InitializeSerialPort();
        void RefreshPortNames();
        IEnumerable<string> GetAvailablePorts();

        // --- Connection ---
        void Open();
        void Close();

        // --- Write ---
        void WriteLine(string line);
        void Write(byte[] buffer, int offset, int count);
        void ClearBuffer();
    }
}
