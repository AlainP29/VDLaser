﻿using VDGrbl.Tools;
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
            var value = converter.Convert(true, typeof(bool), null, CultureInfo.CurrentCulture);
            Assert.AreEqual(new SolidColorBrush(Colors.LightGreen).Color, value);
        }

        [TestMethod()]
        public void ConvertTest1()
        {
            BrushColorConverter converter = new BrushColorConverter();
            var value = converter.Convert(true, typeof(bool), "parameter", CultureInfo.CurrentCulture);
            Assert.AreEqual(new SolidColorBrush(Colors.LightGreen).Color, value);
        }
    }
}