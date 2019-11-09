using VDLaser.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace VDLaser.Tools.Tests
{
    [TestClass()]
    public class DotToCommaConverterTests
    {
        [TestMethod()]
        public void ConvertTest()
        {
            DotToCommaConverter converter = new DotToCommaConverter();
            var value = converter.Convert("12.96", typeof(string), null, CultureInfo.CurrentCulture);
            Assert.AreEqual("12,96", value);
        }

        [TestMethod()]
        public void ConvertTest1()
        {
            DotToCommaConverter converter = new DotToCommaConverter();
            var value = converter.Convert("12,96", typeof(string), "parameter", CultureInfo.CurrentCulture);
            Assert.AreEqual("12,96", value);
        }

        [TestMethod()]
        public void ConvertTest2()
        {
            DotToCommaConverter converter = new DotToCommaConverter();
            var value = converter.Convert("12,96", typeof(string), null, CultureInfo.CurrentCulture);
            Assert.AreEqual("12,96", value);
        }
    }
}