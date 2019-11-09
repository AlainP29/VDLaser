using System.Windows;
using System.Windows.Media;

namespace VDLaser.Tools
{
    /// <summary>
    /// Provide tool to plot the GCode file in a canvas.
    /// </summary>
    public class GraphicTool
    {
        private readonly PointCollection points = new PointCollection();
        public double Xmax { get; private set; }
        public double Ymax { get; private set; }
        public double ScaleFactor { get; private set; }

        /// <summary>
        /// Initialize a new instance of Graphictool class
        /// </summary>
        public GraphicTool()
        {
            
        }
        /// <summary>
        /// Initialize a new instance of graphictool class w/ max x, y G-code value 
        /// </summary>
        /// <param name="xmax"></param>
        /// <param name="ymax"></param>
        public GraphicTool(double xmax, double ymax)
        {
            Xmax = xmax;
            Ymax = ymax;
        }
        public GraphicTool(PointCollection p)
        {
            points = p;
        }
        public PathGeometry Axis(double widthPlane, double heightPlane, int origin)
        {
            switch (origin)
            {
                case 0:
                    points.Add(new Point(0, 0));
                    points.Add(new Point(widthPlane, 0));
                    points.Add(new Point(0, 0));
                    points.Add(new Point(0, heightPlane));
                    break;

                case 1:
                    points.Add(new Point(0, heightPlane / 2));
                    points.Add(new Point(widthPlane, heightPlane / 2));
                    points.Add(new Point(widthPlane/2, 0));
                    points.Add(new Point(widthPlane/2, heightPlane));
                    break;

                default:
                    points.Add(new Point(0, 0));
                    points.Add(new Point(widthPlane, 0));
                    points.Add(new Point(0, heightPlane));
                    break;
            }
            PathFigure axisX = new PathFigure
            {
                StartPoint = points[0]
            };

            LineSegment myLineSegment1 = new LineSegment
            {
                Point = points[1]
            };

            PathFigure axisY = new PathFigure
            {
                StartPoint = points[2]
            };

            LineSegment myLineSegment2 = new LineSegment
            {
                Point = points[3]
            };

            PathSegmentCollection myPathSegmentCollection1 = new PathSegmentCollection
            {
                myLineSegment1
            };
            PathSegmentCollection myPathSegmentCollection2 = new PathSegmentCollection
            {
                myLineSegment2
            };

            axisX.Segments = myPathSegmentCollection1;
            axisY.Segments = myPathSegmentCollection2;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection
            {
                axisX,
                axisY
            };

            PathGeometry myPathGeometry = new PathGeometry
            {
                Figures = myPathFigureCollection
            };

            return myPathGeometry;
        }
        public PathGeometry Plotter()
        {
            PathFigure pathFigure = new PathFigure();
            PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();

            if (points.Count > 0)
            {
                pathFigure.StartPoint = points[0];
                for (int i = 0; i < points.Count; i++)
                {
                    LineSegment lineSegment = new LineSegment
                    {
                        Point = points[i]
                    };
                    pathSegmentCollection.Add(lineSegment);
                }
            }
                pathFigure.Segments = pathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection
            { pathFigure
            };

            PathGeometry myPathGeometry = new PathGeometry
            {
                Figures = myPathFigureCollection
            };

            return myPathGeometry;
        }
        
        private void GetScaleFactor(double canvasWidth, double canvasHeight)
        {
            ScaleFactor=System.Math.Max(canvasWidth / Xmax, canvasHeight / Ymax);
        }
    }
}
