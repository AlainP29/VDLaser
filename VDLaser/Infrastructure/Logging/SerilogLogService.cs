using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Interfaces;

namespace VDLaser.Infrastructure.Logging
{
    public sealed class SerilogLogService : ILogService
    {
        private volatile LogProfile _profile = LogProfile.Normal;

        public LogProfile CurrentProfile => _profile;

        public bool IsCncEnabled => _profile >= LogProfile.Cnc;
        public bool IsSupportEnabled => _profile >= LogProfile.Support;

        public void SetProfile(LogProfile profile)
        {
            _profile = profile;
            Log.Information("[SYSTEM][LOG] Profil actif : {Profile}", profile);
        }

        #region Standard logging API
        public void Debug(string message, params object[] args)
        {
            if (_profile >= LogProfile.Cnc)
                Log.Debug(message, args);
        }
        public void Information(string message, params object[] args)
            => Log.Information(message, args);

        public void Warning(string message, params object[] args)
            => Log.Warning(message, args);

        public void Error(string message, params object[] args)
            => Log.Error(message, args);

        public void Fatal(string message, params object[] args)
            => Log.Fatal(message, args);
        #endregion
    }
}
