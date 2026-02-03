using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Services;
using VDLaser.Core.Interfaces;
using static VDLaser.Core.Grbl.Services.GrblCommandQueueService;

namespace VDLaser.Core.Services
{
    public sealed class LaserStateService : ILaserStateService, IDisposable
    {
        public int LaserPower { get; private set; }
        public bool IsLaserOn { get; private set; }
        public LaserMode LaserMode { get; private set; } = LaserMode.Off;
        private IGrblCommandQueue? _queue;
        private readonly ILogService _log;

        public event Action<int>? LaserPowerChanged;
        public event Action<bool>? LaserStateChanged;
        public event Action<LaserMode>? LaserModeChanged;

        public LaserStateService(IGrblCommandQueue queue, ILogService log)
        {
            if (queue is GrblCommandQueueService q)
            {
                q.LaserPowerCommandSent += OnPower;
                q.LaserStateCommandSent += OnState;
                q.LaserModeCommandSent += OnMode;
            }

            _log = log;
        }

        private void OnPower(int power)
        {
            LaserPower = power;
            IsLaserOn = (power > 0) && (LaserMode != LaserMode.Off);

            LaserPowerChanged?.Invoke(power);
            LaserStateChanged?.Invoke(IsLaserOn);
        }

        private void OnState(bool on)
        {
            IsLaserOn = on;
            if (!on) LaserPower = 0;
            _log.Debug("[LaserStateService Service State Update] New IsOn: {On}, Power reset to: {Power}", on, LaserPower);
            LaserStateChanged?.Invoke(on);
            LaserPowerChanged?.Invoke(LaserPower);
        }

        private void OnMode(LaserMode mode)
        {
            LaserMode = mode;
            _log.Debug("[LaserStateService Service Mode Update] New Mode: {Mode}", mode);
            LaserModeChanged?.Invoke(mode);
            if (mode == LaserMode.Off)
            {
                OnState(false);  
            }
        }

        public void Dispose()
        {
            if (_queue is GrblCommandQueueService q) 
            {
                q.LaserPowerCommandSent -= OnPower;
                q.LaserStateCommandSent -= OnState;
                q.LaserModeCommandSent -= OnMode;
            }
        }
    }

}
