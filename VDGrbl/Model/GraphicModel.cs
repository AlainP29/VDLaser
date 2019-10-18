using System.Windows;
using System.Windows.Media;

namespace VDGrbl.Model
{
    public class GraphicModel
    {
        public string GraphicHeader { get; private set; }
        public PathGeometry GraphicPathGeometry { get; set; }
        public Brush GraphicFill { get; set; }
        public Brush GraphicStroke { get; set; }
        public double GraphicStrokeThickness { get; set; }
        public bool IsFitCanvas { get; set; }

        public GraphicModel()
        {  }
        public GraphicModel(string graphicHeader)
        {
            GraphicHeader = graphicHeader;
        }
    }
}
