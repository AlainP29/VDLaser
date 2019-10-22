using VDGrbl.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Windows.Media;

namespace VDGrbl.Tools.Tests
{
    [TestClass()]
    public class BrushColorConverterTests
    {
        [TestMethod()]
        public void ConvertTest()
        {
            BrushColorConverter converter = new BrushColorConverter();
            var value = converter.Convert(true, typeof(bool), null, CultureInfo.CurrentCulture).ToString();
            //Assert.AreEqual(new SolidColorBrush(Colors.LightGreen).Color, value.);
            var brush = new SolidColorBrush(Colors.LightGreen).ToString();
            Assert.AreEqual(brush, value);
        }

        [TestMethod()]
        public void ConvertTest1()
        {
            BrushColorConverter converter = new BrushColorConverter();
            var value = converter.Convert(true, typeof(bool), "parameter", CultureInfo.CurrentCulture).ToString();
            Assert.AreEqual(new SolidColorBrush(Colors.LightGreen).ToString(), value);
        }
    }
}