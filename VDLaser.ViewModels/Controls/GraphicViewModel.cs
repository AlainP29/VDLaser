using System.Globalization;
using System.Windows;
using System.Windows.Media;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;


namespace VDLaser.Controls.ViewModel
{
    /// <summary>
    /// Handles the transformation of G-Code commands into WPF visual geometries.
    /// </summary>
    public class GraphicViewModel : ViewModelBase
    {
        private readonly ILogService _log;

        public GraphicViewModel(ILogService log)
        {
            _log = log;
        }

        #region Geometry Processing

        public PathGeometry ParseGCode(string[] lines)
        {
            LogContextual(_log, "ParseGCode", $"Processing {lines.Length} lines");

            var geometry = new PathGeometry();
            var figure = new PathFigure();
            bool isFirstPoint = true;

            double lastX = 0;
            double lastY = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || (!line.StartsWith("G1") && !line.StartsWith("G0"))) continue;

                double x = ExtractCoordinate(line, 'X', lastX);
                double y = ExtractCoordinate(line, 'Y', lastY);
                Point pt = new Point(x, y);

                if (isFirstPoint)
                {
                    figure.StartPoint = pt;
                    isFirstPoint = false;
                }
                else
                {
                    figure.Segments.Add(new LineSegment(pt, true));
                }
                lastX = x;
                lastY = y;
            }
            geometry.Figures.Add(figure);
            return geometry;
        }

        /// <summary>
        /// Extracts numerical coordinate from G-Code string.
        /// </summary>
        private double ExtractCoordinate(string line, char axis, double defaultValue)
        {
            string prefix = axis.ToString();
            int startIndex = line.IndexOf(prefix);
            if (startIndex == -1) return defaultValue;
            
            startIndex += 1; 
            int endIndex = line.IndexOf(' ', startIndex);
            if (endIndex == -1) endIndex = line.Length;

            string valueStr = line.Substring(startIndex, endIndex - startIndex);
            if (double.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                return value;
            }
            return defaultValue;
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
        
