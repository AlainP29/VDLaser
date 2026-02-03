using System;
using System.Collections.Generic;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Tools.Geometry;

namespace VDLaser.Tool
{
    public sealed class GcodeGeometryService : IGcodeGeometryService
    {
        private readonly IGcodeParser _parser;
        private readonly int _arcSegments;       // Nombre de points pour approx arcs
        private readonly double _rapidFeedRate;  // Utilisé si nécessaire pour calculs temps

        public GcodeGeometryService(IGcodeParser parser, int arcSegments = 20, double rapidFeedRate = 3000)
        {
            _parser = parser;
            _arcSegments = Math.Max(4, arcSegments);
            _rapidFeedRate = rapidFeedRate;
        }

        public IReadOnlyList<GcodeSegment> BuildGeometry(IEnumerable<string> lines)
        {
            var segments = new List<GcodeSegment>();

            bool absoluteMode = true;
            bool metric = true;
            bool laserOn = false;

            double currentX = 0;
            double currentY = 0;

            foreach (var line in lines)
            {
                var cmd = _parser.Parse(line);
                if (cmd.IsEmpty) continue;

                // --- Modes ---
                if (cmd.G == 90) absoluteMode = true;
                if (cmd.G == 91) absoluteMode = false;
                if (cmd.G == 21) metric = true;
                if (cmd.G == 20) metric = false;

                if (cmd.M == 3 || cmd.M == 4) laserOn = true;
                if (cmd.M == 5) laserOn = false;

                // --- Motion ---
                if (cmd.G is 0 or 1)
                {
                    double targetX = currentX;
                    double targetY = currentY;

                    if (cmd.X.HasValue)
                        targetX = absoluteMode ? cmd.X.Value : currentX + cmd.X.Value;
                    if (cmd.Y.HasValue)
                        targetY = absoluteMode ? cmd.Y.Value : currentY + cmd.Y.Value;

                    if (!metric)
                    {
                        targetX *= 25.4;
                        targetY *= 25.4;
                    }

                    if (currentX != targetX || currentY != targetY)
                    {
                        segments.Add(new GcodeSegment(
                            new Point2D(currentX, currentY),
                            new Point2D(targetX, targetY),
                            IsRapid: cmd.G == 0,
                            LaserOn: laserOn
                        ));
                    }

                    currentX = targetX;
                    currentY = targetY;
                }
                else if (cmd.G is 2 or 3) // Arcs
                {
                    if (!cmd.X.HasValue || !cmd.Y.HasValue || !cmd.I.HasValue || !cmd.J.HasValue)
                        continue; // skip invalid

                    double endX = absoluteMode ? cmd.X.Value : currentX + cmd.X.Value;
                    double endY = absoluteMode ? cmd.Y.Value : currentY + cmd.Y.Value;
                    double centerX = currentX + cmd.I.Value;
                    double centerY = currentY + cmd.J.Value;

                    if (!metric)
                    {
                        endX *= 25.4;
                        endY *= 25.4;
                        centerX *= 25.4;
                        centerY *= 25.4;
                    }

                    var arcPoints = GeometryEngine.InterpolateArc(
                        new Point2D(currentX, currentY),
                        new Point2D(endX, endY),
                        new Point2D(centerX, centerY),
                        clockwise: cmd.G == 2,
                        segments: _arcSegments);

                    Point2D prev = new Point2D(currentX, currentY);
                    foreach (var pt in arcPoints)
                    {
                        segments.Add(new GcodeSegment(prev, pt, IsRapid: false, LaserOn: laserOn));
                        prev = pt;
                    }

                    currentX = endX;
                    currentY = endY;
                }
            }

            return segments;
        }
    }
}
