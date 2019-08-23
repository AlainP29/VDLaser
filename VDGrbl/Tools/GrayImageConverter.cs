using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace VDGrbl.Tools
{
    /// <summary>
    /// Convert an object to double. Use in Xaml.
    /// </summary>
    [ValueConversion(typeof(object), typeof(double))]
    public class GrayImageConverter: IValueConverter
    {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                RasterImageTool rit = new RasterImageTool((BitmapSource)value);
                rit.ImgSourceToGrayScale();

                if (targetType != typeof(BitmapSource))
                {
                    //throw new InvalidOperationException("The target must be a double");
                }

                if (parameter != null)
                {
                        return rit.ImgTransform;
                }
                return rit.ImgTransform;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
}
