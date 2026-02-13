using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Models;

namespace VDLaser.Core.Grbl.Interfaces
{
    public interface IGrblStream
    {
        void SendLine(string command);
        Task SendRealtimeCommandAsync(byte command);
        event EventHandler<DataReceivedEventArgs> DataReceived;
    }
}
