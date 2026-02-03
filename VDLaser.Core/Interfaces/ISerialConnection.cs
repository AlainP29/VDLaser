using System.IO.Ports;

namespace VDLaser.Core.Interfaces
{
    /// <summary>
    /// Pour test unitaire GrblCoreService
    /// </summary>
    public interface ISerialConnection : IDisposable
    {
        bool IsOpen { get; }
        int BytesToRead { get; set; }

        void Open();
        void Close();
        string ReadLine();
        void Write(byte[] buffer, int offset, int count);
        void WriteLine(string line);
        event SerialDataReceivedEventHandler DataReceived;
    }
}
