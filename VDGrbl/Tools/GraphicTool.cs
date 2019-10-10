using System.Windows;
using System.Windows.Media;

namespace VDGrbl.Tools
{
    class GraphicTool
    {
        private readonly PointCollection points=new PointCollection();

        public GraphicTool()
        {
            
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
            PathFigure axisX = new PathFigure();
            axisX.StartPoint = points[0];

            LineSegment myLineSegment1 = new LineSegment();
            myLineSegment1.Point = points[1];

            PathFigure axisY = new PathFigure();
            axisY.StartPoint = points[2];

            LineSegment myLineSegment2 = new LineSegment();
            myLineSegment2.Point = points[3];

            PathSegmentCollection myPathSegmentCollection1 = new PathSegmentCollection();
            myPathSegmentCollection1.Add(myLineSegment1);
            PathSegmentCollection myPathSegmentCollection2 = new PathSegmentCollection();
            myPathSegmentCollection2.Add(myLineSegment2);

            axisX.Segments = myPathSegmentCollection1;
            axisY.Segments = myPathSegmentCollection2;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(axisX);
            myPathFigureCollection.Add(axisY);

            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;

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
                    LineSegment lineSegment = new LineSegment();
                    lineSegment.Point = points[i];
                    pathSegmentCollection.Add(lineSegment);
                }
            }
                pathFigure.Segments = pathSegmentCollection;

                PathFigureCollection myPathFigureCollection = new PathFigureCollection();
                myPathFigureCollection.Add(pathFigure);

                PathGeometry myPathGeometry = new PathGeometry();
                myPathGeometry.Figures = myPathFigureCollection;
            
            return myPathGeometry;
        }
    }
}
