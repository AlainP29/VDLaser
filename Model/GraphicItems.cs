using System.Windows;
using System.Windows.Media;

namespace VDLaser.Model
{
    public class GraphicItems
    {
        public PathGeometry GraphicPathGeometry { get; set; }
        public Brush GraphicFill { get; set; }
        public Brush GraphicStroke { get; set; }
        public double GraphicStrokeThickness { get; set; }
        public bool IsFitCanvas { get; set; }

        public GraphicItems()
        { 
        
        }
    }
}
