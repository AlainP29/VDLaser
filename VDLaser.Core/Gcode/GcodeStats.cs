using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Gcode
{
    public sealed class GcodeStats
    {
        public int LineCount { get; init; }

        public bool UsesLaser { get; init; }
        public bool IsMetric { get; init; }
        public bool IsImperial => !IsMetric;

        public double MinX { get; init; }
        public double MaxX { get; init; }
        public double MinY { get; init; }
        public double MaxY { get; init; }

        public double Width => MaxX - MinX;
        public double Height => MaxY - MinY;

        public TimeSpan EstimatedTime { get; init; }
    }

}
