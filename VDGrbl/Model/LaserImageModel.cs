using System.Windows.Media.Imaging;

namespace VDGrbl.Model
{
    public class LaserImageModel
    {
        public string LaserImageHeader { get; private set; }
        public string LaserImagePath { get; private set; }
        public string LaserImageName { get; private set; }
        public double LaserImageDpiX { get; }
        public double LaserImageDpiY { get; }
        public double LaserImageHeight { get; }
        public double LaserImageWidth { get; }
        public BitmapImage LaserImage { get; private set; }

        public LaserImageModel()
        {
            
        }
        public LaserImageModel(string laserImageHeader)
        {
            LaserImageHeader = laserImageHeader;
        }
        
    }
}
