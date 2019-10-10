using System;
//using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows;

namespace VDGrbl.Tools
{
    /// <summary>
    /// Convert a list of points into PathGeometry. Use in Xaml.
    /// </summary>
    [ValueConversion(typeof(Point[]), typeof(Geometry))]
    public class PointsToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PointCollection points = (PointCollection)value;
            if (targetType != typeof(PointCollection))
            {
                //throw new InvalidOperationException("The target must be a boolean");
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
