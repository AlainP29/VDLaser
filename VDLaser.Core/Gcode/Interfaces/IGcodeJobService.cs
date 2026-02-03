using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VDLaser.Core.Gcode.Services.GcodeJobService;

namespace VDLaser.Core.Gcode.Interfaces
{
    public interface IGcodeJobService
    {
        bool IsRunning { get; }
        bool IsPaused { get; }
        GcodeErrorHandlingMode ErrorHandlingMode { get; set; }
        event EventHandler? StateChanged;
        event EventHandler<GcodeJobProgress>? ProgressChanged;
        event EventHandler<GcodeJobProgress>? ExecutionProgressChanged;
        Task<bool> PlayAsync(IEnumerable<string> gcodeLines, CancellationToken externalToken);
        void Pause();
        void Resume();
        void Stop();
        void Cleanup();
    }

}
