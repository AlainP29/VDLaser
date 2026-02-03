using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Gcode
{
    public sealed record GcodeCommand
    {
        public int? G { get; init; }
        public int? M { get; init; }

        public double? X { get; init; }
        public double? Y { get; init; }
        public double? Z { get; init; }

        public double? I { get; init; }
        public double? J { get; init; }

        public double? F { get; init; }
        public double? S { get; init; }

        public bool IsEmpty =>
            G is null && M is null &&
            X is null && Y is null && Z is null &&
            I is null && J is null &&
            F is null && S is null;
    }

}
