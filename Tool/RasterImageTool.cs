using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VDGrbl.Tool
{
    class RasterImageTool
    {
        #region Properties
        public BitmapSource ImgSource { get; set; }//remove the properties (move to LaserImageModel?) and keep only methods
        public BitmapSource ImgTransform { get; set; }
        public PixelFormat ImgFormat { get; set; }
        public double ImgDpiX { get; }
        public double ImgDpiY { get; }
        public double ImgHeight { get; }
        public double ImgWidth { get; }
        public int ImgPixelHeight { get; }
        public int ImgPixelWidth { get; }
        public PixelFormat ImgPixelFormat { get; private set; }
        #endregion

        #region Constructor
        public RasterImageTool()
        {

        }

        public RasterImageTool(BitmapSource bs)
        {
            if (bs != null)
            {
                if (bs.Format != PixelFormats.Bgra32) //if input is not ARGB format convert to ARGB firstly
                {
                    ImgSource = new FormatConvertedBitmap(bs, PixelFormats.Bgra32, null, 0.0);
                }
                else
                {
                    ImgSource = bs;
                }
                //ImgFormat = PixelFormats.Bgra32;
                ImgDpiX = bs.DpiX;
                ImgDpiY = bs.DpiY;
                ImgHeight = bs.Height;
                ImgWidth = bs.Width;
                ImgPixelHeight = bs.PixelHeight;
                ImgPixelWidth = bs.PixelWidth;
                ImgFormat = bs.Format;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Convert ARGB ImgSource in greyscale format
        /// </summary>
        public void ImgSourceToGrayScale()//0.072*B+0.72*G0.21*R
        {
            if (ImgSource != null)
            {
                FormatConvertedBitmap fcb = new FormatConvertedBitmap();
                fcb.BeginInit();
                fcb.Source = ImgSource;
                if (!ImgSource.Format.Equals(PixelFormats.Gray8))
                {
                    ImgFormat = PixelFormats.Gray8;
                    fcb.DestinationFormat = ImgFormat;
                }
                fcb.EndInit();
                ImgTransform = fcb;
            }
        }

        public BitmapSource ImgSourceToGrayScale(BitmapSource bs)//0.072*B+0.72*G0.21*R
        {
            FormatConvertedBitmap fcb = new FormatConvertedBitmap();
            fcb.BeginInit();
            fcb.Source = bs;
            if (!ImgSource.Format.Equals(PixelFormats.Gray8))
            {
                ImgFormat = PixelFormats.Gray8;
                fcb.DestinationFormat = ImgFormat;
            }
            fcb.EndInit();
            return fcb;
        }

        public void ImgSourceToBlackWhite()
        {
            FormatConvertedBitmap fcb = new FormatConvertedBitmap();
            fcb.BeginInit();
            fcb.Source = ImgSource;
            if (!ImgSource.Format.Equals(PixelFormats.BlackWhite))
            {
                ImgPixelFormat = PixelFormats.BlackWhite;
                fcb.DestinationFormat = ImgFormat;
            }
            fcb.EndInit();
            ImgTransform = fcb;
        }

        public BitmapSource BSToBlackWhite(BitmapSource bs)
        {
            FormatConvertedBitmap fcb = new FormatConvertedBitmap();
            fcb.BeginInit();
            fcb.Source = bs;
            if (!bs.Format.Equals(PixelFormats.BlackWhite))
            {
                ImgPixelFormat = PixelFormats.BlackWhite;
                fcb.DestinationFormat = ImgFormat;
            }
            fcb.EndInit();
            return fcb;
        }
        #endregion
    }
    /*
    private async void SetImage()
   {
      var file = await Windows.Storage.KnownFolders.PicturesLibrary.GetFileAsync("MyImage.png");
      var fileStream = await file.OpenAsync(Window.Storage.FileAccessMode.Read);
      var img = new BitmapImage();
      img.SetSource(fileStream);

      ImgSource = img;
   }*/
}      
