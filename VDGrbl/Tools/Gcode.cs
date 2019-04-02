using System;
using Microsoft.Win32;

namespace VDGrbl.Tools
{
    /// <summary>
    /// G-code class: usefull tool to format, check or parse G-code file.
    /// </summary>
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
            string fLine = string.Format("g9{0}g{1}x{2}y{3}z{4}f{5}", D, G, X, Y, Z, F);
            if (fLine.Contains(","))
            {
                return fLine.Replace(',', '.');
            }
            else
            {
                return fLine;
            }
        }

        /// <summary>
        /// Change/Remove characters in a G-code line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string TrimGcode(string line)
        {
            char[] trimArray = new char[] { '\r', '\n'};
            return line.ToLower().Replace(" ", string.Empty).TrimEnd(trimArray);
    }

        /// <summary>
        /// Convert second in time. Use converter
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SecondToTime(double s)
        {
            TimeSpan ts = TimeSpan.FromSeconds(s);
            return ts.ToString(@"hh\:mm\:ss\:fff");
        }

    }
}
