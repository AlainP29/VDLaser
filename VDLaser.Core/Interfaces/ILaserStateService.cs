using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VDLaser.Core.Grbl.Services.GrblCommandQueueService;

namespace VDLaser.Core.Interfaces
{
    public interface ILaserStateService
    {
        int LaserPower { get; }
        bool IsLaserOn { get; }
        LaserMode LaserMode { get; }

        event Action<int>? LaserPowerChanged;
        event Action<bool>? LaserStateChanged;
        event Action<LaserMode>? LaserModeChanged;
    }


}
