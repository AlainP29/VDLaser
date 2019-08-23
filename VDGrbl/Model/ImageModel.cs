using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VDGrbl.Model
{
    public class ImageModel
    {
        public string ImageHeader { get; private set; }
        public string ImagePath { get; private set; }
        public string FileName { get; private set; }

        //public BitmapSource Source {get; private set;}
        public BitmapImage Image { get; private set; }

        public ImageModel(string imageHeader)
        {
            ImageHeader = imageHeader;
        }
    }
}
