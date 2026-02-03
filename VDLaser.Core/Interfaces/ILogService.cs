using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Interfaces
{
    public interface ILogService
    {
        LogProfile CurrentProfile { get; }

        void SetProfile(LogProfile profile);

        bool IsCncEnabled { get; }
        bool IsSupportEnabled { get; }
        void Debug(string message, params object[] args);
        void Information(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Error(string message, params object[] args);
        void Fatal(string message, params object[] args);
    }
    public enum LogProfile
    {
        Normal,
        Cnc,
        Support
    }

}
