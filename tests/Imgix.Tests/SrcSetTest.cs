using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;

namespace Imgix.Tests
{
    [TestFixture]
    public class SrcSetTest
    {
        private static Dictionary<String, String> parameters;
        private static String[] srcsetSplit;
        private static String[] srcsetWidthSplit;
        private static String[] srcsetHeightSplit;
        private static String[] srcsetAspectRatioSplit;
        private static String[] srcsetWidthAndHeightSplit;
        private static String[] srcsetWidthAndAspectRatioSplit;
        private static String[] srcsetHeightAndAspectRatioSplit;

        [TestFixtureSetUp]
        public void Init()
        {
            String srcset, srcsetWidth, srcsetHeight, srcsetAspectRatio, srcsetWidthAndHeight, srcsetWidthAndAspectRatio, srcsetHeightAndAspectRatio;

            UrlBuilder ub = new UrlBuilder("test.imgix.net", "MYT0KEN", false, true);
            parameters = new Dictionary<String, String>();

            srcset = ub.BuildSrcSet("image.jpg");
            srcsetSplit = srcset.Split(',');

            parameters.Add("w", "300");
            srcsetWidth = ub.BuildSrcSet("image.jpg", parameters);
            srcsetWidthSplit = srcsetWidth.Split(',');
            parameters.Clear();

            parameters.Add("h", "300");
            srcsetHeight = ub.BuildSrcSet("image.jpg", parameters);
            srcsetHeightSplit = srcsetHeight.Split(',');
            parameters.Clear();

            parameters.Add("ar", "3:2");
            srcsetAspectRatio = ub.BuildSrcSet("image.jpg", parameters);
            srcsetAspectRatioSplit = srcsetAspectRatio.Split(',');
            parameters.Clear();

            parameters.Add("w", "300");
            parameters.Add("h", "400");
            srcsetWidthAndHeight = ub.BuildSrcSet("image.jpg", parameters);
            srcsetWidthAndHeightSplit = srcsetWidthAndHeight.Split(',');
            parameters.Clear();

            parameters.Add("w", "300");
            parameters.Add("ar", "3:2");
            srcsetWidthAndAspectRatio = ub.BuildSrcSet("image.jpg", parameters);
            srcsetWidthAndAspectRatioSplit = srcsetWidthAndAspectRatio.Split(',');
            parameters.Clear();

            parameters.Add("h", "300");
            parameters.Add("ar", "3:2");
            srcsetHeightAndAspectRatio = ub.BuildSrcSet("image.jpg", parameters);
            srcsetHeightAndAspectRatioSplit = srcsetHeightAndAspectRatio.Split(',');
            parameters.Clear();
        }

        [Test]
        public void NoParametersGeneratesCorrectWidths()
        {
            int[] targetWidths = {
                100, 116, 135, 156, 181, 210, 244, 283,
                328, 380, 441, 512, 594, 689, 799, 927,
                1075, 1247, 1446, 1678, 1946, 2257, 2619,
                3038, 3524, 4087, 4741, 5500, 6380, 7401, 8192};

            String generatedWidth;
            int index = 0;
            int widthInt;

            foreach (String src in srcsetSplit)
            {
                generatedWidth = src.Split(' ')[1];
                widthInt = int.Parse(generatedWidth.Substring(0, generatedWidth.Length - 1));
                Assert.AreEqual(targetWidths[index], widthInt);
                index++;
            }
        }

        [Test]
        public void NoParametersReturnsExpectedNumberOfPairs()
        {
            int expectedPairs = 31;
            Assert.AreEqual(expectedPairs, srcsetSplit.Length);
        }

        [Test]
        public void NoParametersDoesNotExceedBounds()
        {
            String minWidth = srcsetSplit[0].Split(' ')[1];
            String maxWidth = srcsetSplit[srcsetSplit.Length - 1].Split(' ')[1];

            int minWidthInt = int.Parse(minWidth.Substring(0, minWidth.Length - 1));
            int maxWidthInt = int.Parse(maxWidth.Substring(0, maxWidth.Length - 1));

            Assert.True(minWidthInt >= 100);
            Assert.True(maxWidthInt <= 8192);
        }

        // a 17% testing threshold is used to account for rounding
        [Test]
        public void NoParametersDoesNotIncreaseMoreThan17Percent()
        {
            const double INCREMENT_ALLOWED = .17;
            String width;
            int widthInt, prev;

            // convert and store first width (typically: 100)
            width = srcsetSplit[0].Split(' ')[1];
            prev = int.Parse(width.Substring(0, width.Length - 1));

            foreach (String src in srcsetSplit)
            {
                width = src.Split(' ')[1];
                widthInt = int.Parse(width.Substring(0, width.Length - 1));

                Assert.True((widthInt / prev) < (1 + INCREMENT_ALLOWED));
                prev = widthInt;
            }
        }

        [Test]
        public void NoParametersSignsUrls()
        {
            String src, parameters, generatedSignature, signatureBase, expectedSignature;

            foreach (String srcLine in srcsetSplit)
            {

                src = srcLine.Split(' ')[0];
                Assert.True(src.Contains("s="));
                generatedSignature = src.Substring(src.IndexOf("s=") + 2);

                // calculate the number of chars between ? and s=
                var parameterAll = src.Substring(src.IndexOf("?")).Length;
                var parameterSignKey = src.Substring(src.IndexOf("s=")).Length;

                parameters = src.Substring(src.IndexOf("?"), parameterAll - parameterSignKey - 1);
                signatureBase = "MYT0KEN" + "/image.jpg" + parameters;
                var hashString = String.Format("{0}/{1}{2}", "MYT0KEN", "image.jpg", parameters);
                expectedSignature = BitConverter.ToString(MD5.Create().ComputeHash(signatureBase.Select(Convert.ToByte).ToArray())).Replace("-", "").ToLower();

                Assert.AreEqual(expectedSignature, generatedSignature);
            }
        }

        [Test]
        public void WidthInDPRForm()
        {
            String generatedRatio;
            int expectedRatio = 1;
            Assert.True(srcsetWidthSplit.Length == 5);

            foreach (String src in srcsetWidthSplit)
            {
                generatedRatio = src.Split(' ')[1];
                Assert.AreEqual(expectedRatio + "x", generatedRatio);
                expectedRatio++;
            }
        }

        [Test]
        public void WidthSignsUrls()
        {
            String src, parameters, generatedSignature, signatureBase, expectedSignature;

            foreach (String srcLine in srcsetWidthSplit)
            {

                src = srcLine.Split(' ')[0];
                Assert.True(src.Contains("s="));
                generatedSignature = src.Substring(src.IndexOf("s=") + 2);

                // calculate the number of chars between ? and s=
                var parameterAll = src.Substring(src.IndexOf("?")).Length;
                var parameterSignKey = src.Substring(src.IndexOf("s=")).Length;

                parameters = src.Substring(src.IndexOf("?"), parameterAll - parameterSignKey - 1);
                signatureBase = "MYT0KEN" + "/image.jpg" + parameters;
                var hashString = String.Format("{0}/{1}{2}", "MYT0KEN", "image.jpg", parameters);
                expectedSignature = BitConverter.ToString(MD5.Create().ComputeHash(signatureBase.Select(Convert.ToByte).ToArray())).Replace("-", "").ToLower();

                Assert.AreEqual(expectedSignature, generatedSignature);
            }
        }

        [Test]
        public void WidthIncludesDPRParam()
        {
            String src;

            for (int i = 0; i < srcsetWidthSplit.Length; i++)
            {
                src = srcsetWidthSplit[i].Split(' ')[0];
                Assert.True(src.Contains(String.Format("dpr={0}", i + 1)));
            }
        }

        // [Test]
        // TODO: remove this test, deprecated
        // public void HeightGeneratesCorrectWidths()
        // {
        //     int[] targetWidths = {
        //         100, 116, 135, 156, 181, 210, 244, 283,
        //         328, 380, 441, 512, 594, 689, 799, 927,
        //         1075, 1247, 1446, 1678, 1946, 2257, 2619,
        //         3038, 3524, 4087, 4741, 5500, 6380, 7401, 8192};

        //     String generatedWidth;
        //     int index = 0;
        //     int widthInt;

        //     foreach (String src in srcsetHeightSplit)
        //     {
        //         generatedWidth = src.Split(' ')[1];
        //         widthInt = int.Parse(generatedWidth.Substring(0, generatedWidth.Length - 1));
        //         Assert.AreEqual(targetWidths[index], widthInt);
        //         index++;
        //     }
        // }

        [Test]
        public void HeightContainsHeightParameter()
        {
            String url;

            foreach (String src in srcsetHeightSplit)
            {
                url = src.Split(' ')[0];
                Assert.True(url.Contains("h="));
            }
        }

        [Test]
        public void HeightReturnsExpectedNumberOfPairs()
        {
            int expectedPairs = 5;
            Assert.AreEqual(expectedPairs, srcsetHeightSplit.Length);
        }

        // TODO: remove this test, deprecated
        // [Test]
        // public void HeightDoesNotExceedBounds()
        // {
        //     String minWidth = srcsetHeightSplit[0].Split(' ')[1];
        //     String maxWidth = srcsetHeightSplit[srcsetHeightSplit.Length - 1].Split(' ')[1];

        //     int minWidthInt = int.Parse(minWidth.Substring(0, minWidth.Length - 1));
        //     int maxWidthInt = int.Parse(maxWidth.Substring(0, maxWidth.Length - 1));

        //     Assert.True(minWidthInt >= 100);
        //     Assert.True(maxWidthInt <= 8192);
        // }

        // TODO: remove this test, deprecated
        // a 17% testing threshold is used to account for rounding
        // [Test]
        // public void testHeightDoesNotIncreaseMoreThan17Percent()
        // {
        //     const double INCREMENT_ALLOWED = .17;
        //     String width;
        //     int widthInt, prev;

        //     // convert and store first width (typically: 100)
        //     width = srcsetHeightSplit[0].Split(' ')[1];
        //     prev = int.Parse(width.Substring(0, width.Length - 1));

        //     foreach (String src in srcsetHeightSplit)
        //     {
        //         width = src.Split(' ')[1];
        //         widthInt = int.Parse(width.Substring(0, width.Length - 1));

        //         Assert.True((widthInt / prev) < (1 + INCREMENT_ALLOWED));
        //         prev = widthInt;
        //     }
        // }

        [Test]
        public void testHeightSignsUrls()
        {
            String src, parameters, generatedSignature, signatureBase, expectedSignature;

            foreach (String srcLine in srcsetHeightSplit)
            {

                src = srcLine.Split(' ')[0];
                Assert.True(src.Contains("s="));
                generatedSignature = src.Substring(src.IndexOf("s=") + 2);

                // calculate the number of chars between ? and s=
                var parameterAll = src.Substring(src.IndexOf("?")).Length;
                var parameterSignKey = src.Substring(src.IndexOf("s=")).Length;

                parameters = src.Substring(src.IndexOf("?"), parameterAll - parameterSignKey - 1);
                signatureBase = "MYT0KEN" + "/image.jpg" + parameters;
                var hashString = String.Format("{0}/{1}{2}", "MYT0KEN", "image.jpg", parameters);
                expectedSignature = BitConverter.ToString(MD5.Create().ComputeHash(signatureBase.Select(Convert.ToByte).ToArray())).Replace("-", "").ToLower();

                Assert.AreEqual(expectedSignature, generatedSignature);
            }
        }

        [Test]
        public void WidthAndHeightInDPRForm()
        {
            String generatedRatio;
            int expectedRatio = 1;
            Assert.True(srcsetWidthAndHeightSplit.Length == 5);

            foreach (String src in srcsetWidthAndHeightSplit)
            {
                generatedRatio = src.Split(' ')[1];
                Assert.AreEqual(expectedRatio + "x", generatedRatio);
                expectedRatio++;
            }
        }

        [Test]
        public void WidthAndHeightSignsUrls()
        {
            String src, parameters, generatedSignature, signatureBase, expectedSignature;

            foreach (String srcLine in srcsetWidthAndHeightSplit)
            {

                src = srcLine.Split(' ')[0];
                Assert.True(src.Contains("s="));
                generatedSignature = src.Substring(src.IndexOf("s=") + 2);

                // calculate the number of chars between ? and s=
                var parameterAll = src.Substring(src.IndexOf("?")).Length;
                var parameterSignKey = src.Substring(src.IndexOf("s=")).Length;

                parameters = src.Substring(src.IndexOf("?"), parameterAll - parameterSignKey - 1);
                signatureBase = "MYT0KEN" + "/image.jpg" + parameters;
                var hashString = String.Format("{0}/{1}{2}", "MYT0KEN", "image.jpg", parameters);
                expectedSignature = BitConverter.ToString(MD5.Create().ComputeHash(signatureBase.Select(Convert.ToByte).ToArray())).Replace("-", "").ToLower();

                Assert.AreEqual(expectedSignature, generatedSignature);
            }
        }

        [Test]
        public void WidthAndHeightIncludesDPRParam()
        {
            String src;

            for (int i = 0; i < srcsetWidthAndHeightSplit.Length; i++)
            {
                src = srcsetWidthAndHeightSplit[i].Split(' ')[0];
                Assert.True(src.Contains(String.Format("dpr={0}", i + 1)));
            }
        }

        [Test]
        public void AspectRatioGeneratesCorrectWidths()
        {
            int[] targetWidths = {
                100, 116, 135, 156, 181, 210, 244, 283,
                328, 380, 441, 512, 594, 689, 799, 927,
                1075, 1247, 1446, 1678, 1946, 2257, 2619,
                3038, 3524, 4087, 4741, 5500, 6380, 7401, 8192};

            String generatedWidth;
            int index = 0;
            int widthInt;

            foreach (String src in srcsetAspectRatioSplit)
            {
                generatedWidth = src.Split(' ')[1];
                widthInt = int.Parse(generatedWidth.Substring(0, generatedWidth.Length - 1));
                Assert.AreEqual(targetWidths[index], widthInt);
                index++;
            }
        }

        [Test]
        public void AspectRatioContainsARParameter()
        {
            String url;

            foreach (String src in srcsetAspectRatioSplit)
            {
                url = src.Split(' ')[0];
                Assert.True(url.Contains("ar="));
            }
        }

        [Test]
        public void AspectRatioReturnsExpectedNumberOfPairs()
        {
            int expectedPairs = 31;
            Assert.AreEqual(expectedPairs, srcsetAspectRatioSplit.Length);
        }

        [Test]
        public void AspectRatioDoesNotExceedBounds()
        {
            String minWidth = srcsetAspectRatioSplit[0].Split(' ')[1];
            String maxWidth = srcsetAspectRatioSplit[srcsetAspectRatioSplit.Length - 1].Split(' ')[1];

            int minWidthInt = int.Parse(minWidth.Substring(0, minWidth.Length - 1));
            int maxWidthInt = int.Parse(maxWidth.Substring(0, maxWidth.Length - 1));

            Assert.True(minWidthInt >= 100);
            Assert.True(maxWidthInt <= 8192);
        }

        // a 17% testing threshold is used to account for rounding
        [Test]
        public void AspectRatioDoesNotIncreaseMoreThan17Percent()
        {
            const double INCREMENT_ALLOWED = .17;
            String width;
            int widthInt, prev;

            // convert and store first width (typically: 100)
            width = srcsetAspectRatioSplit[0].Split(' ')[1];
            prev = int.Parse(width.Substring(0, width.Length - 1));

            foreach (String src in srcsetAspectRatioSplit)
            {
                width = src.Split(' ')[1];
                widthInt = int.Parse(width.Substring(0, width.Length - 1));

                Assert.True((widthInt / prev) < (1 + INCREMENT_ALLOWED));
                prev = widthInt;
            }
        }

        [Test]
        public void AspectRatioSignsUrls()
        {
            String src, parameters, generatedSignature, signatureBase, expectedSignature;

            foreach (String srcLine in srcsetAspectRatioSplit)
            {

                src = srcLine.Split(' ')[0];
                Assert.True(src.Contains("s="));
                generatedSignature = src.Substring(src.IndexOf("s=") + 2);

                // calculate the number of chars between ? and s=
                var parameterAll = src.Substring(src.IndexOf("?")).Length;
                var parameterSignKey = src.Substring(src.IndexOf("s=")).Length;

                parameters = src.Substring(src.IndexOf("?"), parameterAll - parameterSignKey - 1);
                signatureBase = "MYT0KEN" + "/image.jpg" + parameters;
                var hashString = String.Format("{0}/{1}{2}", "MYT0KEN", "image.jpg", parameters);
                expectedSignature = BitConverter.ToString(MD5.Create().ComputeHash(signatureBase.Select(Convert.ToByte).ToArray())).Replace("-", "").ToLower();

                Assert.AreEqual(expectedSignature, generatedSignature);
            }
        }

        [Test]
        public void WidthAndAspectRatioInDPRForm()
        {
            String generatedRatio;
            int expectedRatio = 1;
            Assert.True(srcsetWidthAndAspectRatioSplit.Length == 5);

            foreach (String src in srcsetWidthAndAspectRatioSplit)
            {
                generatedRatio = src.Split(' ')[1];
                Assert.AreEqual(expectedRatio + "x", generatedRatio);
                expectedRatio++;
            }
        }

        [Test]
        public void WidthAndAspectRatioSignsUrls()
        {
            String src, parameters, generatedSignature, signatureBase, expectedSignature;

            foreach (String srcLine in srcsetWidthAndAspectRatioSplit)
            {

                src = srcLine.Split(' ')[0];
                Assert.True(src.Contains("s="));
                generatedSignature = src.Substring(src.IndexOf("s=") + 2);

                // calculate the number of chars between ? and s=
                var parameterAll = src.Substring(src.IndexOf("?")).Length;
                var parameterSignKey = src.Substring(src.IndexOf("s=")).Length;

                parameters = src.Substring(src.IndexOf("?"), parameterAll - parameterSignKey - 1);
                signatureBase = "MYT0KEN" + "/image.jpg" + parameters;
                var hashString = String.Format("{0}/{1}{2}", "MYT0KEN", "image.jpg", parameters);
                expectedSignature = BitConverter.ToString(MD5.Create().ComputeHash(signatureBase.Select(Convert.ToByte).ToArray())).Replace("-", "").ToLower();

                Assert.AreEqual(expectedSignature, generatedSignature);
            }
        }

        [Test]
        public void WidthAndAspectRatioIncludesDPRParam()
        {
            String src;

            for (int i = 0; i < srcsetWidthAndAspectRatioSplit.Length; i++)
            {
                src = srcsetWidthAndAspectRatioSplit[i].Split(' ')[0];
                Assert.True(src.Contains(String.Format("dpr={0}", i + 1)));
            }
        }

        [Test]
        public void HeightAndAspectRatioInDPRForm()
        {
            String generatedRatio;
            int expectedRatio = 1;
            Assert.True(srcsetHeightAndAspectRatioSplit.Length == 5);

            foreach (String src in srcsetHeightAndAspectRatioSplit)
            {
                generatedRatio = src.Split(' ')[1];
                Assert.AreEqual(String.Format("{0}", expectedRatio + "x"), generatedRatio);
                expectedRatio++;
            }
        }

        [Test]
        public void HeightAndAspectRatioSignsUrls()
        {
            String src, parameters, generatedSignature, signatureBase, expectedSignature;

            foreach (String srcLine in srcsetHeightAndAspectRatioSplit)
            {

                src = srcLine.Split(' ')[0];
                Assert.True(src.Contains("s="));
                generatedSignature = src.Substring(src.IndexOf("s=") + 2);

                // calculate the number of chars between ? and s=
                var parameterAll = src.Substring(src.IndexOf("?")).Length;
                var parameterSignKey = src.Substring(src.IndexOf("s=")).Length;

                parameters = src.Substring(src.IndexOf("?"), parameterAll - parameterSignKey - 1);
                signatureBase = "MYT0KEN" + "/image.jpg" + parameters;
                var hashString = String.Format("{0}/{1}{2}", "MYT0KEN", "image.jpg", parameters);
                expectedSignature = BitConverter.ToString(MD5.Create().ComputeHash(signatureBase.Select(Convert.ToByte).ToArray())).Replace("-", "").ToLower();

                Assert.AreEqual(expectedSignature, generatedSignature);
            }
        }

        [Test]
        public void HeightAndAspectRatioIncludesDPRParam()
        {
            String src;

            for (int i = 0; i < srcsetHeightAndAspectRatioSplit.Length; i++)
            {
                src = srcsetHeightAndAspectRatioSplit[i].Split(' ')[0];
                Assert.True(src.Contains(String.Format("dpr={0}", i + 1)));
            }
        }

        [Test]
        public void TargetWidths100to7400()
        {
            List<int> actual = UrlBuilder.GenerateTargetWidths(begin: 100, end: 7401);
            int[] expected = {
                100, 116, 135, 156, 181, 210, 244, 283,
                328, 380, 441, 512, 594, 689, 799, 927,
                1075, 1247, 1446, 1678, 1946, 2257, 2619,
                3038, 3524, 4087, 4741, 5500, 6380, 7401};
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TargetWidths328to4088()
        {
            List<int> actual = UrlBuilder.GenerateTargetWidths(begin: 328, end: 4088);
            int[] expected = {
                328, 380, 441, 512, 594, 689, 799, 927,
                1075, 1247, 1447, 1678, 1947, 2259, 2620,
                3039, 3525, 4088};

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void TargetWidths100to8192MaxTolerance()
        {
            List<int> actual = UrlBuilder.GenerateTargetWidths(tol: 100000000);
            int[] expected = { 100, 8192 };

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void TestCustomSrcSetPairs()
        {
            UrlBuilder ub = new UrlBuilder(
                "test.imgix.net",
                signKey: null,
                includeLibraryParam: false,
                useHttps: true);

            // Begin == End
            String srcset = ub.BuildSrcSet(
                "image.jpg",
                new Dictionary<string, string>(),
                begin: 328, end: 328);

            Assert.AreEqual("https://test.imgix.net/image.jpg?w=328 328w", srcset);

            String srcset640to720 = ub.BuildSrcSet(
                "image.jpg",
                new Dictionary<string, string>(),
                begin: 640, end: 720);

            String expect640to720 =
                "https://test.imgix.net/image.jpg?w=640 640w,\n" +
                "https://test.imgix.net/image.jpg?w=720 720w";

            Assert.AreEqual(expect640to720, srcset640to720);

            String expect100to108 =
                "https://test.imgix.net/image.jpg?w=100 100w,\n" +
                "https://test.imgix.net/image.jpg?w=102 102w,\n" +
                "https://test.imgix.net/image.jpg?w=104 104w,\n" +
                "https://test.imgix.net/image.jpg?w=106 106w,\n" +
                "https://test.imgix.net/image.jpg?w=108 108w";

            String srcset100to108 = ub.BuildSrcSet(
                "image.jpg",
                new Dictionary<string, string>(),
                begin: 100,
                end: 108,
                tol: 0.01);

            Assert.AreEqual(expect100to108, srcset100to108);
        }

        [Test]
        public void TestSrcSetPairsWithCustomTargets()
        {
            UrlBuilder ub = new UrlBuilder(
               "test.imgix.net",
               signKey: null,
               includeLibraryParam: false,
               useHttps: true);

            String expect100to513 =
                "https://test.imgix.net/image.jpg?w=100 100w,\n" +
                "https://test.imgix.net/image.jpg?w=200 200w,\n" +
                "https://test.imgix.net/image.jpg?w=300 300w,\n" +
                "https://test.imgix.net/image.jpg?w=400 400w,\n" +
                "https://test.imgix.net/image.jpg?w=513 513w";

            int[] targetWidths = {100, 200, 300, 400, 513};

            String srcset100to513 = ub.BuildSrcSet(
                "image.jpg",
                new Dictionary<string, string>(),
                targetWidths.ToList());

            Assert.AreEqual(expect100to513, srcset100to513);
        }

        [Test]
        public void TestDisableVariableQualityOffByDefault()
        {
            UrlBuilder ub = new UrlBuilder(
                "test.imgix.net",
                signKey: null,
                includeLibraryParam: false,
                useHttps: true);

            var parameters = new Dictionary<string, string>() { { "w", "100" } };
            String srcset = ub.BuildSrcSet("image.jpg", parameters);

            String expected =
            "https://test.imgix.net/image.jpg?w=100&q=75&dpr=1 1x,\n" +
            "https://test.imgix.net/image.jpg?w=100&q=50&dpr=2 2x,\n" +
            "https://test.imgix.net/image.jpg?w=100&q=35&dpr=3 3x,\n" +
            "https://test.imgix.net/image.jpg?w=100&q=23&dpr=4 4x,\n" +
            "https://test.imgix.net/image.jpg?w=100&q=20&dpr=5 5x";

            Assert.AreEqual(expected, srcset);
        }

        [Test]
        public void TestDisableVariableQuality()
        {
            UrlBuilder ub = new UrlBuilder(
                "test.imgix.net",
                signKey: null,
                includeLibraryParam: false,
                useHttps: true);

            var parameters = new Dictionary<string, string>() { { "w", "100" } };

            String srcset = ub.BuildSrcSet(
                "image.jpg",
                parameters,
                disableVariableQuality: true); // disable variable quality

            String expected =
            "https://test.imgix.net/image.jpg?w=100&dpr=1 1x,\n" +
            "https://test.imgix.net/image.jpg?w=100&dpr=2 2x,\n" +
            "https://test.imgix.net/image.jpg?w=100&dpr=3 3x,\n" +
            "https://test.imgix.net/image.jpg?w=100&dpr=4 4x,\n" +
            "https://test.imgix.net/image.jpg?w=100&dpr=5 5x";

            Assert.AreEqual(expected, srcset);
        }

        [Test]
        public void TestDisableVariableQualityWithQuality()
        {
            UrlBuilder ub = new UrlBuilder(
                "test.imgix.net",
                signKey: null,
                includeLibraryParam: false,
                useHttps: false);

            var parameters = new Dictionary<string, string>() { { "w", "100" }, { "q", "99" } };

            String srcset = ub.BuildSrcSet(
                "image.png",
                parameters,
                disableVariableQuality: true); // disable variable quality

            String expected =
                "http://test.imgix.net/image.png?w=100&q=99&dpr=1 1x,\n" +
                "http://test.imgix.net/image.png?w=100&q=99&dpr=2 2x,\n" +
                "http://test.imgix.net/image.png?w=100&q=99&dpr=3 3x,\n" +
                "http://test.imgix.net/image.png?w=100&q=99&dpr=4 4x,\n" +
                "http://test.imgix.net/image.png?w=100&q=99&dpr=5 5x";

            Assert.AreEqual(expected, srcset);
        }

        [Test]
        public void TestVariableQualityEnabledWithQ()
        {
            UrlBuilder ub = new UrlBuilder(
                "test.imgix.net",
                signKey: null,
                includeLibraryParam: false,
                useHttps: true);

            // Pass "q" with variable quality enabled.
            var parameters = new Dictionary<string, string>() { { "ar", "2:3"}, { "h", "100" }, { "q", "99" } };

            String srcset = ub.BuildSrcSet(
                "image.png",
                parameters,
                disableVariableQuality: false); // Variable quality enabled.

            String expected =
                "https://test.imgix.net/image.png?ar=2%3A3&h=100&q=99&dpr=1 1x,\n" +
                "https://test.imgix.net/image.png?ar=2%3A3&h=100&q=99&dpr=2 2x,\n" +
                "https://test.imgix.net/image.png?ar=2%3A3&h=100&q=99&dpr=3 3x,\n" +
                "https://test.imgix.net/image.png?ar=2%3A3&h=100&q=99&dpr=4 4x,\n" +
                "https://test.imgix.net/image.png?ar=2%3A3&h=100&q=99&dpr=5 5x";

            Assert.AreEqual(expected, srcset);
        }
    }
}
