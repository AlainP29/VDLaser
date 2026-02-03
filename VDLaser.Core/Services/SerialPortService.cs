using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows.Shapes;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;

namespace VDLaser.Core.Services
{
    public class SerialPortService : ISerialPortService
    {
        private readonly ILogService _log;
        private SerialPort? _serialPort;

        public string PortName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 115200;
        public Parity Parity { get; set; } = Parity.None;
        public int DataBits { get; set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Handshake Handshake { get; set; } = Handshake.None;
        public int ReadTimeout { get; set; } = 5000;
        public int WriteTimeout { get; set; } = 2000;

        public List<string> ListPortNames { get; private set; } = new();
        public List<int> ListBaudRates { get; private set; } = new()
        {
            1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400
        };

        public event EventHandler SettingsChanged;
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler? ConnectionLost;

        public SerialPortService(ILogService log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            RefreshPortNames();
        }

        public void InitializeSerialPort()
        {
            _serialPort = new SerialPort
            {
                PortName = PortName,
                BaudRate = BaudRate,
                Parity = Parity,
                DataBits = DataBits,
                StopBits = StopBits,
                Handshake = Handshake,
                ReadTimeout = ReadTimeout,
                WriteTimeout = WriteTimeout,
                Encoding = System.Text.Encoding.UTF8
            };

            _serialPort.DataReceived += OnDataReceived;

            _log.Information("[SerialPortService] Serial port initialized");
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort?.IsOpen != true)
                return;
            try
            {


                while (_serialPort.BytesToRead > 0)
                {
                    string line = _serialPort.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        DataReceived?.Invoke(this, new DataReceivedEventArgs(line));
                    }
                }
            }
            catch (TimeoutException)
            {
                // On ignore silencieusement les timeouts de lecture simples 
                // pour ne pas polluer les logs, GRBL finira par répondre.
            }
            catch (Exception ex)
            {
                _log.Error("[SerialPortService] Error reading data : {Message}", ex.Message);
                if (ex is System.IO.IOException || ex is UnauthorizedAccessException)
                {
                    HandleCriticalFailure();
                }
            }
        }
        public bool IsPortAvailable(string portName)
        {
            return SerialPort.GetPortNames().Contains(portName);
        }
        public void Open()
        {
            try
            {
                if (_serialPort == null)
                    InitializeSerialPort();

                if (_serialPort!.IsOpen)
                    return;

                _serialPort.Open();
                _log.Information("[SerialPortService] Serial port opened on {Port}", PortName);
            }
            catch (Exception ex)
            {
                _log.Error("[SerialPortService] Cannot open serial port : {Message}", ex.Message);
                throw;
            }
        }

        public void Close()
        {
            try
            {
                if (_serialPort?.IsOpen != true)
                    return;

                _serialPort.DataReceived -= OnDataReceived;
                _serialPort.Close();

                _log.Information("[SerialPortService] Serial port closed");
            }
            catch (Exception ex)
            {
                _log.Error("[SerialPortService] Error closing serial port : {Message}", ex.Message);
            }
        }

        public IEnumerable<string> GetAvailablePorts() => SerialPort.GetPortNames();

        public void RefreshPortNames()
        {
            ListPortNames = SerialPort.GetPortNames().ToList();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            _log.Information("[SerialPortService] ports refreshed");

        }

        public void WriteLine(string line)
        {
            if (_serialPort?.IsOpen != true) return;
            try
            {
                _log.Information("[SerialPortService] Send text command : {Cmd}", line);
                _serialPort.WriteLine(line);
            }
            catch (Exception ex)
            {
                _log.Fatal("[SerialPortService] Perte de connexion physique : {Message}", ex.Message);
                HandleCriticalFailure();
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (_serialPort?.IsOpen != true)
                return;

            _log.Information("[SerialPortService] Send byte command");
            _serialPort.Write(buffer, offset, count);
        }
        private void HandleCriticalFailure()
        {
            _log.Warning("[SerialPortService] Signalement de perte de connexion.");
            if (_serialPort == null || !_serialPort.IsOpen) return;
            try
            {
                _log.Warning("[SerialPortService] Fermeture du port suite à une erreur matérielle.");
                if (_serialPort != null)
                {
                    _serialPort.DataReceived -= OnDataReceived;
                    _serialPort.Close();
                    _serialPort.Dispose();
                }

                Close();
            }
            catch { /* On ignore les erreurs lors de la fermeture forcée */ }
            finally
            {
                ConnectionLost?.Invoke(this, EventArgs.Empty);
            }
        }
        public void ClearBuffer()
        {
            if (_serialPort?.IsOpen == true)
            {
                try
                {
                    _serialPort.DiscardInBuffer();  // Vide ce que l'Arduino a envoyé
                    _serialPort.DiscardOutBuffer(); // Vide ce que le PC s'apprête à envoyer
                    _log.Information("[SerialPortService] Communication buffers cleared.");
                }
                catch (Exception ex)
                {
                    _log.Warning("[SerialPortService] Could not clear buffers: {Msg}", ex.Message);
                }
            }
        }
        public void Dispose()
        {
            try
            {
                _serialPort?.Dispose();
                _log.Information("[SerialPortService] Serial port disposed");
            }
            catch { }
        }
    }
}
