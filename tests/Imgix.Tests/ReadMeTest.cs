using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Imgix.Tests
{
    [TestFixture]
    public class ReadMeTest
    {
        [Test]
        public void TestVariableQualityDefault()
        {
            UrlBuilder ub = new UrlBuilder(
                "demo.imgix.net",
                signKey: null,
                includeLibraryParam: false,
                useHttps: true);

            Dictionary<string, string>
                parameters = new Dictionary<string, string>() { { "w", "100" } };

            String srcset = ub.BuildSrcSet(
                "image.jpg",
                parameters,
                disableVariableQuality: false); // Variable quality enabled.

            String expected =
                "https://demo.imgix.net/image.jpg?w=100&q=75&dpr=1 1x,\n" +
                "https://demo.imgix.net/image.jpg?w=100&q=50&dpr=2 2x,\n" +
                "https://demo.imgix.net/image.jpg?w=100&q=35&dpr=3 3x,\n" +
                "https://demo.imgix.net/image.jpg?w=100&q=23&dpr=4 4x,\n" +
                "https://demo.imgix.net/image.jpg?w=100&q=20&dpr=5 5x";

            Assert.AreEqual(expected, srcset);
        }

        [Test]
        public void TestCustomWidths()
        {
            UrlBuilder ub = new UrlBuilder(
                "demo.imgix.net",
                signKey: null,
                includeLibraryParam: false,
                useHttps: true);

            int[] widths = { 144, 240, 320, 446, 640 };
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            String srcset = ub.BuildSrcSet("image.jpg", parameters, widths.ToList());

            String expected =
                "https://demo.imgix.net/image.jpg?w=144 144w,\n" +
                "https://demo.imgix.net/image.jpg?w=240 240w,\n" +
                "https://demo.imgix.net/image.jpg?w=320 320w,\n" +
                "https://demo.imgix.net/image.jpg?w=446 446w,\n" +
                "https://demo.imgix.net/image.jpg?w=640 640w";

            Assert.AreEqual(expected, srcset);
        }

        [Test]
        public void TestCustomWidthRange()
        {
            UrlBuilder ub = new UrlBuilder(
                "demo.imgix.net",
                signKey: null,
                includeLibraryParam: false,
                useHttps: true);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            String srcset = ub.BuildSrcSet("image.jpg", parameters, 500, 2000);

            String expected =
                "https://demo.imgix.net/image.jpg?w=500 500w,\n" +
                "https://demo.imgix.net/image.jpg?w=580 580w,\n" +
                "https://demo.imgix.net/image.jpg?w=673 673w,\n" +
                "https://demo.imgix.net/image.jpg?w=780 780w,\n" +
                "https://demo.imgix.net/image.jpg?w=905 905w,\n" +
                "https://demo.imgix.net/image.jpg?w=1050 1050w,\n" +
                "https://demo.imgix.net/image.jpg?w=1218 1218w,\n" +
                "https://demo.imgix.net/image.jpg?w=1413 1413w,\n" +
                "https://demo.imgix.net/image.jpg?w=1639 1639w,\n" +
                "https://demo.imgix.net/image.jpg?w=1901 1901w,\n" +
                "https://demo.imgix.net/image.jpg?w=2000 2000w";

            Assert.AreEqual(expected, srcset);
        }

        [Test]
        public void TestCustomWidthTolerance()
        {
            UrlBuilder ub = new UrlBuilder(
                "demo.imgix.net",
                signKey: null,
                includeLibraryParam: false,
                useHttps: true);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            String srcset = ub.BuildSrcSet("image.jpg", parameters, 100, 384, 0.20);

            String expected =
                "https://demo.imgix.net/image.jpg?w=100 100w,\n" +
                "https://demo.imgix.net/image.jpg?w=140 140w,\n" +
                "https://demo.imgix.net/image.jpg?w=196 196w,\n" +
                "https://demo.imgix.net/image.jpg?w=274 274w,\n" +
                "https://demo.imgix.net/image.jpg?w=384 384w";

            Assert.AreEqual(expected, srcset);
        }
    }
}
