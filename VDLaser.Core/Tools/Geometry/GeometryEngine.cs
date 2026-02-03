using System;
using System.Collections.Generic;
using System.Linq;

namespace VDLaser.Core.Tools.Geometry
{
    public static class GeometryEngine
    {
        /// <summary>Distance entre l'origine (0,0) et un point.</summary>
        public static double Distance(double x, double y) => Math.Sqrt(x * x + y * y);

        /// <summary>Distance entre deux points.</summary>
        public static double Distance(double x0, double y0, double x1, double y1) =>
            Math.Sqrt(Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2));

        public static double Distance(Point2D p0, Point2D p1) => Distance(p0.X, p0.Y, p1.X, p1.Y);

        /// <summary>Rayon d'un cercle donné par un centre et un point sur le cercle.</summary>
        public static double Radius(Point2D point, Point2D center) => Distance(point, center);

        /// <summary>Angle en radians d'une corde par rapport à l'abscisse (de p0 à p1).</summary>
        public static double AngleChordAbscissa(Point2D p0, Point2D p1)
        {
            double dist = Distance(p0, p1);
            return dist > 0 ? Math.Acos((p1.X - p0.X) / dist) : 0;
        }

        /// <summary>Point max (X max, Y max) dans une collection.</summary>
        public static Point2D MaxPoint(IEnumerable<Point2D> points)
        {
            if (points == null || !points.Any()) return new Point2D(0, 0);
            return new Point2D(points.Max(p => p.X), points.Max(p => p.Y));
        }

        /// <summary>Point min (X min, Y min) dans une collection.</summary>
        public static Point2D MinPoint(IEnumerable<Point2D> points)
        {
            if (points == null || !points.Any()) return new Point2D(0, 0);
            return new Point2D(points.Min(p => p.X), points.Min(p => p.Y));
        }

        /// <summary>Intersection ligne-cercle.</summary>
        public static List<Point2D> LineCircleIntersection(Point2D start, Point2D end, Point2D center, double radius)
        {
            var intersections = new List<Point2D>();
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double a = dx * dx + dy * dy;
            double b = 2 * (dx * (start.X - center.X) + dy * (start.Y - center.Y));
            double c = (start.X - center.X) * (start.X - center.X) + (start.Y - center.Y) * (start.Y - center.Y) - radius * radius;

            double disc = b * b - 4 * a * c;
            if (disc < 0) return intersections;

            double sqrtDisc = Math.Sqrt(disc);
            double t1 = (-b + sqrtDisc) / (2 * a);
            double t2 = (-b - sqrtDisc) / (2 * a);

            if (t1 >= 0 && t1 <= 1) intersections.Add(new Point2D(start.X + t1 * dx, start.Y + t1 * dy));
            if (t2 >= 0 && t2 <= 1 && t2 != t1) intersections.Add(new Point2D(start.X + t2 * dx, start.Y + t2 * dy));

            return intersections;
        }

        /// <summary>Intersection cercle-cercle.</summary>
        public static List<Point2D> CircleCircleIntersection(Point2D c1, double r1, Point2D c2, double r2)
        {
            var intersections = new List<Point2D>();
            double d = Distance(c1, c2);
            if (d > r1 + r2 || d < Math.Abs(r1 - r2) || (d == 0 && r1 == r2)) return intersections;

            double a = (r1 * r1 - r2 * r2 + d * d) / (2 * d);
            double h = Math.Sqrt(r1 * r1 - a * a);
            double xm = c1.X + a * (c2.X - c1.X) / d;
            double ym = c1.Y + a * (c2.Y - c1.Y) / d;

            intersections.Add(new Point2D(xm + h * (c2.Y - c1.Y) / d, ym - h * (c2.X - c1.X) / d));
            intersections.Add(new Point2D(xm - h * (c2.Y - c1.Y) / d, ym + h * (c2.X - c1.X) / d));

            return intersections;
        }

        /// <summary>Vérifie si un point est sur un segment.</summary>
        public static bool IsPointOnLineSegment(Point2D p, Point2D start, Point2D end)
        {
            double epsilon = 1e-6;
            double minX = Math.Min(start.X, end.X);
            double maxX = Math.Max(start.X, end.X);
            double minY = Math.Min(start.Y, end.Y);
            double maxY = Math.Max(start.Y, end.Y);

            return p.X >= minX - epsilon && p.X <= maxX + epsilon &&
                   p.Y >= minY - epsilon && p.Y <= maxY + epsilon;
        }

        /// <summary>Vérifie si un point est sur un arc (G2/G3).</summary>
        public static bool IsPointOnArc(Point2D p, Point2D center, Point2D start, Point2D end, bool clockwise)
        {
            double epsilon = 1e-6;
            double radius = Distance(start, center);
            if (Math.Abs(Distance(p, center) - radius) > epsilon) return false;

            double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
            double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);
            double pointAngle = Math.Atan2(p.Y - center.Y, p.X - center.X);

            startAngle = (startAngle + 2 * Math.PI) % (2 * Math.PI);
            endAngle = (endAngle + 2 * Math.PI) % (2 * Math.PI);
            pointAngle = (pointAngle + 2 * Math.PI) % (2 * Math.PI);

            if (clockwise)
            {
                if (startAngle < endAngle) startAngle += 2 * Math.PI;
                return pointAngle <= startAngle && pointAngle >= endAngle;
            }
            else
            {
                if (endAngle < startAngle) endAngle += 2 * Math.PI;
                return pointAngle >= startAngle && pointAngle <= endAngle;
            }
        }

        /// <summary>Intersections ligne-arc.</summary>
        public static List<Point2D> LineArcIntersection(Point2D lineStart, Point2D lineEnd,
            Point2D arcCenter, double arcRadius, Point2D arcStart, Point2D arcEnd, bool clockwise)
        {
            var intersections = new List<Point2D>();
            var circleIntersections = LineCircleIntersection(lineStart, lineEnd, arcCenter, arcRadius);

            foreach (var p in circleIntersections)
            {
                if (IsPointOnArc(p, arcCenter, arcStart, arcEnd, clockwise) &&
                    IsPointOnLineSegment(p, lineStart, lineEnd))
                    intersections.Add(p);
            }
            return intersections;
        }

        /// <summary>Intersections arc-arc.</summary>
        public static List<Point2D> ArcArcIntersection(Point2D c1, double r1, Point2D start1, Point2D end1, bool cw1,
                                                        Point2D c2, double r2, Point2D start2, Point2D end2, bool cw2)
        {
            var intersections = new List<Point2D>();
            var circleIntersections = CircleCircleIntersection(c1, r1, c2, r2);

            foreach (var p in circleIntersections)
            {
                if (IsPointOnArc(p, c1, start1, end1, cw1) &&
                    IsPointOnArc(p, c2, start2, end2, cw2))
                    intersections.Add(p);
            }
            return intersections;
        }

        /// <summary>Approximation polyligne d’un arc (G2/G3) avec n segments.</summary>
        public static IEnumerable<Point2D> InterpolateArc(Point2D start, Point2D end, Point2D center, bool clockwise, int segments = 20)
        {
            var points = new List<Point2D>();
            double radius = Distance(start, center);
            double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
            double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);

            if (clockwise && endAngle >= startAngle) endAngle -= 2 * Math.PI;
            if (!clockwise && endAngle <= startAngle) endAngle += 2 * Math.PI;

            double step = (endAngle - startAngle) / segments;
            double prevX = start.X, prevY = start.Y;

            for (int i = 1; i <= segments; i++)
            {
                double angle = startAngle + i * step;
                double x = center.X + radius * Math.Cos(angle);
                double y = center.Y + radius * Math.Sin(angle);
                points.Add(new Point2D(x, y));
                prevX = x; prevY = y;
            }
            return points;
        }
    }
}
