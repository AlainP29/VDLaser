using System.Windows;
using System.Windows.Media;

namespace VDGrbl.Model
{
    /// <summary>
    /// Graphic class model : plot 2D G-code file.
    /// 
    /// </summary>
    public class GraphicModel
    {
        public string GraphicHeader { get; private set; }
        public Geometry Cursor { get; set; }
        public PathGeometry GraphicPathGeometry { get; set; }
        public Brush GraphicFill { get; set; }
        public Brush GraphicStroke { get; set; }
        public double GraphicStrokeThickness { get; set; }

        public GraphicModel()
        {
            
        }
        public GraphicModel(string graphicHeader)
        {
            GraphicHeader = graphicHeader;
        }
    }
}
