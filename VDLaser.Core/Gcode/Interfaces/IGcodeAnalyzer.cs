using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VDLaser.Core.Gcode.Interfaces
{
    public interface IGcodeAnalyzer
    {
        GcodeStats Analyze(IEnumerable<string> lines);
    }

}
