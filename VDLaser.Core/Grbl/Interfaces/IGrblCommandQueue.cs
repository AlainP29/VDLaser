using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Grbl.Commands;
using VDLaser.Core.Models;

namespace VDLaser.Core.Grbl.Interfaces
{
    public interface IGrblCommandQueue
    {
        bool IsBusy { get; }

        Task<GrblCommandResult> EnqueueAsync(
            string command,
            string? source = null,
            bool waitForOk = true,
            CancellationToken ct = default);

        Task SendRealtimeAsync(
            byte realtimeCommand,
            string? source = null);

        void OnDataReceived(object? sender, DataReceivedEventArgs e);
        event EventHandler<GrblCommandEventArgs>? CommandExecuted;
        event EventHandler<int>? RxBufferSizeChanged;
        event Action<int>? LaserPowerCommandSent;
        event Action<bool>? LaserStateCommandSent;

        void Reset();
        void FlushCurrentAsOk(string reason);
    }


}
