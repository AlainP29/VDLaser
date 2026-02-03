using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Gcode;

namespace VDLaser.Core.Gcode.Services
{
    public class GcodeFileResult
    {
        public IReadOnlyList<string> RawLines { get; init; } = [];
        public IReadOnlyList<GcodeCommand> ParsedCommands { get; init; } = [];
        public GcodeStats Stats { get; init; } = new();
    }

}
