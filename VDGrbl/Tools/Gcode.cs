using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDGrbl.Tools
{
    public static class Gcode
    {
        /// <summary>
        /// Static method to format a G-code line using distance mode G9, motion mode G, positions X, Y, Z, FeedRate and Step.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="fl"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FormatGcode(int d,int g, double x, double y, double z, double f, double s)
        {
            int D = d;
            int G = g;
            double X = x * s; 
            double Y = y * s;
            double Z = z * s;
            double F = f;
            string fLine = string.Format("G9{0} G{1} X{2} Y{3} Z{4} F{5}", D, G, X, Y, Z, F);
            if (fLine.Contains(","))
            {
                return fLine.Replace(',', '.');
            }
            else
            {
                return fLine;
            }
        }
    }
}
