using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VDGrbl.Tools
{
    class RasterImageTool
    {
        public BitmapSource ImgSource { get; set; }
        public BitmapSource ImgTransform { get; set; }
        public PixelFormat ImgPixelFormat { get; set; }

        public RasterImageTool()
        {

        }

        public RasterImageTool(BitmapSource imgSource)
        {
            if (imgSource != null)
            {
                if (imgSource.Format != PixelFormats.Bgra32) //if input is not ARGB format convert to ARGB firstly
                {
                    ImgSource = new FormatConvertedBitmap(imgSource, PixelFormats.Bgra32, null, 0.0);
                }
                else
                {
                    ImgSource = imgSource;
                }
                ImgPixelFormat = PixelFormats.Bgra32;
            }
        }

        public void ImgSourceToGrayScale()//0.072*B+0.72*G0.21*R
        {
            if (ImgSource != null)
            {
                FormatConvertedBitmap fcb = new FormatConvertedBitmap();
                fcb.BeginInit();
                fcb.Source = ImgSource;
                if (!ImgSource.Format.Equals(PixelFormats.Gray8))
                {
                    ImgPixelFormat = PixelFormats.Gray8;
                    fcb.DestinationFormat = ImgPixelFormat;
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
                ImgPixelFormat = PixelFormats.Gray8;
                fcb.DestinationFormat = ImgPixelFormat;
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
                fcb.DestinationFormat = ImgPixelFormat;
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
                fcb.DestinationFormat = ImgPixelFormat;
            }
            fcb.EndInit();
            return fcb;
        }
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
