using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

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

        /// <summary>
        /// Calculate the radius of a circle with center coordinates (i,j) and with point coordinate (x,y) on the circle.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static double RayonGCode(double x, double y, double i, double j)
        {
            return Math.Sqrt(Math.Pow(x - i, 2) + Math.Pow(y - j, 2));
        }

        /// <summary>
        /// Calculate the angle between the chord endswith (x0,y0) and (x1,y1) of the circle with center coordinate (i,j) and abscissa axis.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static double AngleChordAbscissa(double x0, double y0, double x1, double y1, double i, double j)
        {
                return Math.Acos((x1 - x0) / Math.Sqrt(Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2)));
        }

        /// <summary>
        /// Get a couple (max X and max Y) of a pointcollection
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point MaxPointCollection(PointCollection points)
        {
            List<double> listX = new List<double>();
            List<double> listY = new List<double>();
            double xmax=0,ymax=0;

            if (points != null)
            {
                foreach (Point p in points)
                {
                    listX.Add(Convert.ToDouble(p.X));
                    listY.Add(Convert.ToDouble(p.Y));
                }
                xmax = listX.Max();
                ymax = listY.Max();
            }
            return new Point(xmax,ymax);
        }

        /// <summary>
        /// Get a couple (min X and min Y) of a pointcollection
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point MinPointCollection(PointCollection points)
        {
            List<double> listX = new List<double>();
            List<double> listY = new List<double>();
            double xmin = 0, ymin = 0;

            if (points != null)
            {
                foreach (Point p in points)
                {
                    listX.Add(Convert.ToDouble(p.X));
                    listY.Add(Convert.ToDouble(p.Y));
                }
                xmin = listX.Min();
                ymin = listY.Min();
            }
            return new Point(xmin, ymin);
        }
    }
}
