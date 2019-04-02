using System;

namespace VDGrbl.Tools
{
    public static class MathTool
    {
        /// <summary>
        /// Calculate in mm the distance between the origin and point P(x,y)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double Distance(double x, double y)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        /// <summary>
        /// Calculate in mm the distance between point P0(x0,y0) and point P1(x1,y1)
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static double Distance(double x0, double y0, double x1, double y1)
        {
            return Math.Sqrt(Math.Pow((x1 - x0), 2) + Math.Pow((y1 - y0), 2));
        }

        /// <summary>
        /// Calculate in mm the distance between point P0(x0,y0) and point P1(x1,y1) with step s
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double Distance(double x0, double y0, double x1, double y1, double s)
        {
            return Math.Sqrt(Math.Pow((x1 - x0) * s, 2) + Math.Pow((y1 - y0) * s, 2));
        }

    }
}
