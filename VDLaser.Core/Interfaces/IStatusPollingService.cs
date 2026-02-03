using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Interfaces
{
    public interface IStatusPollingService
    {
        void Start();
        void Stop();
        void ForcePoll();

        bool IsRunning { get; }
    }
}
