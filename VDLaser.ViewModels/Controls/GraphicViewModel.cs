using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VDLaser.ViewModels.Base;


namespace VDLaser.Controls.ViewModel
{
    public class GraphicViewModel : ViewModelBase
    {

        public PathGeometry ParseGCode(string[] lines)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            bool isFirstPoint = true;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("G1")) continue;

                // Simple extraction logic for X and Y
                double x = ExtractCoordinate(line, 'X');
                double y = ExtractCoordinate(line, 'Y');
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
            }
            geometry.Figures.Add(figure);
            return geometry;
        }
        private double ExtractCoordinate(string line, char axis)
        {
            string prefix = axis.ToString();
            int startIndex = line.IndexOf(prefix);
            if (startIndex == -1) return 0;
            startIndex += 1; // Move past the axis character
            int endIndex = line.IndexOf(' ', startIndex);
            if (endIndex == -1) endIndex = line.Length;
            string valueStr = line.Substring(startIndex, endIndex - startIndex);
            if (double.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                return value;
            }
            return 0;
        }
        
    }
}
        
