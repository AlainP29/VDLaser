using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Tools.Geometry
{
    /// <summary>
    /// Point 2D simple pour la géométrie du G-code.
    /// </summary>
    public readonly record struct Point2D(double X, double Y);
}

