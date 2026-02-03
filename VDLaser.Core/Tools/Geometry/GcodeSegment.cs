using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Tools.Geometry
{
    /// <summary>
    /// Représente un segment de mouvement du laser.
    /// </summary>
    public sealed record GcodeSegment(
        Point2D Start,
        Point2D End,
        bool IsRapid,
        bool LaserOn);
}

