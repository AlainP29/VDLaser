using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Interfaces;

namespace VDLaser.Tests.UnitTests.Fakes
{
    public class FakeSerialConnection : ISerialConnection
    {
        public bool IsOpen { get; private set; }
        public int BytesToRead { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event SerialDataReceivedEventHandler? DataReceived;

        public void Open() => IsOpen = true;
        public void Close() => IsOpen = false;

        public void Write(byte[] buffer, int offset, int count)
        {
            // noop
        }

        public void WriteLine(string line)
        {
            // noop
        }
        public string ReadLine() => _nextLine ?? string.Empty;

        private string? _nextLine;
        /*
        public void SimulateReceive(string line)
        {
            _nextLine = line;
            DataReceived?.Invoke(this,
                new SerialDataReceivedEventArgs(SerialData.Chars));
        }
        */

        public void Dispose() { }
    }

}
