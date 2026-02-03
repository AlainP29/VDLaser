using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Gcode;
using VDLaser.Core.Tools.Geometry;

namespace VDLaser.Core.Gcode.Interfaces
{
    public interface IGcodeGeometryService
    {
        IReadOnlyList<GcodeSegment> BuildGeometry(IEnumerable<string> lines);
    }


}
