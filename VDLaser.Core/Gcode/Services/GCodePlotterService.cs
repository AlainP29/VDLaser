using System.Windows;
using System.Windows.Media;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Interfaces;

namespace VDLaser.Core.Gcode.Services
{
    

    public class GCodePlotterService
    {
        private readonly IGcodeParser _parser;
        private readonly ILogService _log;

        public GCodePlotterService(IGcodeParser parser, ILogService log)
        {
            _parser = parser;
            _log = log;
        }
        /// <summary>
        /// Construit deux géométries séparées : une pour les trajets rapides/sans gravure (gris) et une pour la gravure active (bleu).
        /// </summary>
        /// <param name="commands">Liste des commandes G-code.</param>
        /// <param name="offsetX">Offset X (par défaut 0).</param>
        /// <param name="offsetY">Offset Y (par défaut 0).</param>
        /// <returns>Un tuple (RapidPath, EngravePath).</returns>
        public (PathGeometry RapidPath, PathGeometry EngravePath)
            BuildGeometriesFromCommands(
                IEnumerable<GcodeCommand> commands,
                double offsetX = 0,
                double offsetY = 0)
        {
            var rapidGeometry = new PathGeometry();
            var engraveGeometry = new PathGeometry();

            PathFigure? currentRapidFigure = null;
            PathFigure? currentEngraveFigure = null;

            double curX = 0;
            double curY = 0;
            int? lastG = null;
            bool laserOn = false;

            foreach (var cmd in commands)
            {
                if (cmd.IsEmpty)
                    continue;

                // ----- État laser -----
                if (cmd.M == 3||cmd.M==4) laserOn = true;
                if (cmd.M == 5) laserOn = false;
                //if (cmd.S.HasValue) laserOn = cmd.S.Value > 0;

                // ----- Mouvement -----
                int? g = cmd.G ?? lastG;
                if (!g.HasValue)
                    continue;

                double targetX = cmd.X ?? curX;
                double targetY = cmd.Y ?? curY;

                if (targetX == curX && targetY == curY)
                {
                    lastG = g;
                    continue;
                }

                Point startPoint = new Point(curX - offsetX, curY - offsetY);
                Point endPoint = new Point(targetX - offsetX, targetY - offsetY);

                bool isRapid = g == 0 || !laserOn;
                bool isEngrave = g != 0 && laserOn;

                PathGeometry geometry = isEngrave ? engraveGeometry : rapidGeometry;
                ref PathFigure? currentFigure =
                    ref (isEngrave ? ref currentEngraveFigure : ref currentRapidFigure);

                // ----- Nouvelle figure si nécessaire -----
                if (currentFigure == null)
                {
                    currentFigure = new PathFigure
                    {
                        StartPoint = startPoint,
                        IsFilled = false,
                        IsClosed = false
                    };
                    geometry.Figures.Add(currentFigure);
                }

                // ----- Ajout segment -----
                if (g == 0 || g == 1)
                {
                    currentFigure.Segments.Add(
                        new LineSegment(endPoint, true)
                    );
                }
                else if (g == 2 || g == 3)
                {
                    currentFigure.Segments.Add(
                        CreateArc(
                            curX, curY,
                            targetX, targetY,
                            cmd.I ?? 0,
                            cmd.J ?? 0,
                            g == 3,
                            offsetX,
                            offsetY)
                    );
                }

                // ----- Mise à jour position -----
                curX = targetX;
                curY = targetY;
                lastG = g;

                // ----- Changement de type = fin de figure -----
                if (isEngrave)
                    currentRapidFigure = null;
                else
                    currentEngraveFigure = null;
            }

            return (rapidGeometry, engraveGeometry);
        }
        /// <summary>
        /// Construit une géométrie pour l'ensemble de la figure G-code.
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public PathGeometry BuildGeometryFromCommands(IEnumerable<GcodeCommand> commands, double offsetX = 0, double offsetY = 0)
        {
            var geometry = new PathGeometry();
            PathFigure? currentFigure = null;

            double curX = 0, curY = 0;
            int? lastG = null;

            foreach (var cmd in commands)
            {
                if (cmd.IsEmpty) continue;

                double targetX = cmd.X ?? curX;
                double targetY = cmd.Y ?? curY;
                int? currentG = cmd.G ?? lastG;

                Point targetPoint = new Point(targetX - offsetX, targetY - offsetY);

                // CORRECTION : Si c'est G0 ou si on n'a pas encore de figure, on en crée une.
                if (currentG == 0 || currentFigure == null)
                {
                    currentFigure = new PathFigure { StartPoint = targetPoint, IsFilled = false };
                    geometry.Figures.Add(currentFigure);
                }
                else if (currentG == 1) // G1
                {
                    currentFigure.Segments.Add(new LineSegment(targetPoint, true));
                }
                else if (currentG == 2 || currentG == 3) // G2/G3
                {
                    var arc = CreateArc(curX, curY, targetX, targetY, cmd.I ?? 0, cmd.J ?? 0, currentG == 3, offsetX, offsetY);
                    currentFigure.Segments.Add(arc);
                }

                curX = targetX;
                curY = targetY;
                lastG = currentG;
            }
            return geometry;
        }
        private ArcSegment CreateArc(double startX, double startY, double endX, double endY, double i, double j, bool isCounterClockwise, double offsetX, double offsetY)
        {
            double centerX = startX + i - offsetX;
            double centerY = startY + j - offsetY;
            double radius = Math.Sqrt(Math.Pow((startX - offsetX) - centerX, 2) + Math.Pow((startY - offsetY) - centerY, 2));
            return new ArcSegment
            {
                Point = new Point(endX - offsetX, endY - offsetY),
                Size = new Size(radius, radius),
                //SweepDirection = isCounterClockwise ? SweepDirection.Counterclockwise : SweepDirection.Counterclockwise,
                SweepDirection = isCounterClockwise ? SweepDirection.Counterclockwise : SweepDirection.Clockwise,
                IsLargeArc = IsLargeArc(startX, startY, endX, endY, centerX + offsetX, centerY + offsetY, isCounterClockwise)
            };
        }
        private bool IsLargeArc(double startX, double startY, double endX, double endY, double centerX, double centerY, bool isCCW)
        {
            double angleStart = Math.Atan2(startY - centerY, startX - centerX);
            double angleEnd = Math.Atan2(endY - centerY, endX - centerX);
            double diff = angleEnd - angleStart;

            if (isCCW)
            {
                if (diff <= 0) diff += 2 * Math.PI;
            }
            else
            {
                if (diff >= 0) diff -= 2 * Math.PI;
            }
            return Math.Abs(diff) > Math.PI;
        }
    }
}
