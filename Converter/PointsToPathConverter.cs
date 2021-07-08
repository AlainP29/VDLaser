using System;
//using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows;
using NLog;

namespace VDLaser.Converter
{
    /// <summary>
    /// Points to PathGeometry converter. Use in Xaml.
    /// </summary>
    [ValueConversion(typeof(Point[]), typeof(Geometry))]
    public class PointsToPathConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Convert a list of points into PathGeometry.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PointCollection points = (PointCollection)value;
            if (targetType != typeof(PointCollection))
            {
                logger.Error("StringToIntConverter|The target must be a list of points");
            }
            if (parameter != null&&points.Count>0)
            {
                PathFigure pathFigure = new PathFigure();
                pathFigure.StartPoint = points[0];
                //List<LineSegment> segments = new List<LineSegment>();
                PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();

                for (int i=0; i < points.Count; i++)
                {
                    LineSegment lineSegment = new LineSegment();
                    //segments.Add(lineSegment);
                    lineSegment.Point = points[i];
                    pathSegmentCollection.Add(lineSegment);
                }
                pathFigure.Segments=pathSegmentCollection;

                PathFigureCollection pathFigureCollection = new PathFigureCollection();
                pathFigureCollection.Add(pathFigure);

                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures=pathFigureCollection;
                return pathGeometry;
            }
            else
            return null;
        }
        /// <summary>
        /// Not implemented yet.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
