using System.IO.Ports;
using VDLaser.Core.Interfaces;

/// <summary>
/// Pour test unitaire GrblCoreService
/// </summary>
public class SerialPortConnection : ISerialConnection
{
    private readonly SerialPort _port;

    public bool IsOpen => _port.IsOpen;

    public int BytesToRead => _port.BytesToRead;

    int ISerialConnection.BytesToRead { get => BytesToRead; set => throw new NotImplementedException(); }

    public event SerialDataReceivedEventHandler? DataReceived;

    public SerialPortConnection(SerialPort port)
    {
        _port = port;
        _port.DataReceived += (s, e) => DataReceived?.Invoke(s, e);
    }

    public void Open()
    {
        try
        {
            if (!_port.IsOpen)
            {
                _port.Open();
            }
        }
        catch (UnauthorizedAccessException)
        {
            // On ne fait rien ici, l'exception sera gérée par la boucle 
            // de reconnexion dans le ViewModel qui réessaiera plus tard.
            throw;
        }
    }
    public void Close() => _port.Close();
    public void Write(byte[] buffer, int offset, int count)
    {
        try
        {
            if (_port.IsOpen)
            {
                _port.Write(buffer, offset, count);
            }
        }
        catch (Exception)
        {
            if (_port.IsOpen) _port.Close();
            throw;
        }
    }
public void WriteLine(string line)
    {
        try
        {
            if (_port.IsOpen)
            {
                _port.WriteLine(line);
            }
        }
        catch (Exception)
        {
            if (_port.IsOpen) _port.Close();
            throw;
        }
    }
    public string ReadLine() => _port.ReadLine();

    public void Dispose() => _port.Dispose();
}
