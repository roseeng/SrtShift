using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Globalization;

namespace SrtShift.Test
{
    [TestClass]
    public class RateTest
    {
        private SrtParser srt;
        public static CultureInfo MyCulture = CultureInfo.CreateSpecificCulture("fr-FR");

        [TestInitialize]
        public void Setup()
        {
            srt = new SrtParser();
        }

        [TestMethod]
        public void TestCalc1()
        {
            srt.RateCalc1("08", "22,288");
            TimeSpan tsi = TimeSpan.Parse("00:00:22,288", MyCulture);
            TimeSpan tso = TimeSpan.Parse("00:00:08", MyCulture);
            var result = srt.Adjust(tsi);
            Assert.AreEqual((tso - result).TotalMilliseconds, 0);
        }

        [TestMethod]
        public void TestCalc2()
        {
            srt.RateCalc2("08", "22,288", "44:41", "45:06,648");
            TimeSpan ts1i = TimeSpan.Parse("00:00:22,288", MyCulture);
            TimeSpan ts1o = TimeSpan.Parse("00:00:08", MyCulture);
            var result = srt.Adjust(ts1i);
            Assert.AreEqual((ts1o - result).TotalMilliseconds, 0);

            TimeSpan ts2i = TimeSpan.Parse("00:45:06,648", MyCulture);
            TimeSpan ts2o = TimeSpan.Parse("00:44:41", MyCulture);
            result = srt.Adjust(ts2i);
            Assert.AreEqual((ts2o - result).TotalMilliseconds, 0);
        }
    }
}
