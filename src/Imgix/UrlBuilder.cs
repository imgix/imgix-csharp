using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

namespace Imgix
{
    public class UrlBuilder
    {
        public Boolean UseHttps;
        public Boolean IncludeLibraryParam;

        private String _signKey;
        public String SignKey { set { _signKey = value; } }
        private String Domain;

        private static readonly int[] SrcSetTargetWidths = {
            100, 116, 135, 156, 181, 210, 244, 283,
            328, 380, 441, 512, 594, 689, 799, 927,
            1075, 1247, 1446, 1678, 1946, 2257, 2619,
            3038, 3524, 4087, 4741, 5500, 6380, 7401, 8192 };

        private static readonly int[] DprRatios = { 1, 2, 3, 4, 5 };
        private static readonly int[] DprQualities = { 75, 50, 35, 23, 20 };

        const int MaxWidth = 8192;
        const int MinWidth = 100;
        const double SrcSetWidthTolerance = 0.08;

        public UrlBuilder(String domain,
                          String signKey = null,
                          Boolean includeLibraryParam = true,
                          Boolean useHttps = true)
        {
            Domain = domain;
            _signKey = signKey;
            IncludeLibraryParam = includeLibraryParam;
            UseHttps = useHttps;
        }

        public UrlBuilder(String domain, Boolean useHttps)
            : this(domain, signKey: null, useHttps: useHttps)
        {
        }

        public UrlBuilder(String domain, String signKey, Boolean useHttps)
            : this(domain, signKey: signKey, includeLibraryParam: true, useHttps: useHttps)
        {
        }

        public String BuildUrl(String path)
        {
            return BuildUrl(path, new Dictionary<string, string>());
        }

        /// <summary>
        /// Create a resolvable imgix url.
        /// 
        /// </summary>
        /// <param name="path">path to the image, i.e. "image/file.png"</param>
        /// <param name="parameters">dictionary of query parameters</param>
        /// <returns>resolvable URL</returns>

        /*
        * Encodes the `path` and `paramater` arguments to the UTF8 
        * scheme and returns a formatted string that conforms to the 
        * "{scheme}://{domain}/{path}{params}" format.
        */
        public String BuildUrl(String path, Dictionary<String, String> parameters)
        {
            String scheme = UseHttps ? "https" : "http";
            path = SanitizePath(path);

            var qs = BuildParams(parameters);
            var localParams = new Dictionary<String, String>(parameters);

            if (!String.IsNullOrEmpty(_signKey))
            {
                var hashString = String.Format("{0}/{1}{2}", _signKey, path, localParams.Any() ? "?" + qs : String.Empty);
                localParams.Add("s", HashString(hashString));
            }
            return String.Format("{0}://{1}/{2}{3}", scheme, Domain, path, localParams.Any() ? "?" + BuildParams(localParams) : String.Empty);
        }

        /**
        * SanitizePath strips the leading slash. It `UrlEncodes` proxy paths to ensure *all* 
        * characters are handled. If path is not being used as a proxy it leaves legal 
        * characters, like '/' and '@', alone while encoding the rest using `EncodeURI`.
        */
        private String SanitizePath(String p)
        {
            String path = p;
            // Strip leading slash first (we'll re-add after encoding).
            path = path.TrimEnd('/').TrimStart('/');

            // Check if path is a proxy path and store type of proxy.
            Dictionary<String, Boolean> proxyStatus = CheckProxyStatus(path);
            Object pathIsProxy = proxyStatus["isProxy"];
            Object proxyIsEncoded = proxyStatus["isEncoded"];

            if (pathIsProxy.Equals(true) && proxyIsEncoded.Equals(false))
            {
                // Use WebUtility.UrlEncode to ensure *all* characters are handled,
                // since it's being used as a path.
                return WebUtility.UrlEncode(path);
            }
            else if (pathIsProxy.Equals(true) && proxyIsEncoded.Equals(true))
            {
                // Do nothing to URL being used as path since its encoded.
                return path;
            }
            else
            {
                // The path is not being used as a proxy, so leave
                // legal characters like '/' and '@' alone.
                return EncodeURI(path);
            }
        }

        /*
        * EncodeURI encodes string `p` to the UTF8 format. It splits
        * the string on the `/` character to avoid accidentally
        * encoding them. It then iterates over every character and
        * calls the `UrlEncode` method on them. Finally, it joins
        * the string array on `/`, adding the removed character back.
        */
        private String EncodeURI(String p)
        {
            string[] splitPath = p.Split('/');
            List<String> sanitizedPath = new List<String>();

            foreach (String s in splitPath)
            {
                sanitizedPath.Add(UrlEncode(s));
            }

            return string.Join("/", sanitizedPath);
        }


        /* 
        * CheckProxyStatus checks if the path has one of the four possible
        * acceptable proxy prefixes. First we check if the path has the
        * correct ascii prefix. If it does then we know that it is a proxy,
        * but it's not percent encoded. Second, we check if the path is
        * prefixed by a percent-encoded prefix. If it is, we know that it's
        * a proxy and that it's percent-encoded. Finally, if the path isn't
        * prefixed by any of these four prefixes, it is not a valid proxy.
        * This might be "just enough validation," but if we run into issues
        * we can make this check smarter/more-robust.
        */
        private static Dictionary<String, Boolean> CheckProxyStatus(String p)
        {
            String path = p;
            Dictionary<String, Boolean> status = new Dictionary<String, Boolean>();
            path.Replace("^/", "");

            String asciiHTTP = "http://";
            String asciiHTTPS = "https://";

            String encodedHTTP = "http%3A%2F%2F";
            String encodedHTTPS = "https%3A%2F%2F";

            String encodedHTTPLower = "http%3a%2f%2f";
            String encodedHTTPSLower = "https%3a%ff%2f";

            if (path.StartsWith(asciiHTTP) || path.StartsWith(asciiHTTPS))
            {
                status.Add("isProxy", true);
                status.Add("isEncoded", false);

            }
            else if (path.StartsWith(encodedHTTP) || path.StartsWith(encodedHTTPS))
            {
                status.Add("isProxy", true);
                status.Add("isEncoded", true);

            }
            else if (path.StartsWith(encodedHTTPLower) || path.StartsWith(encodedHTTPSLower))
            {
                status.Add("isProxy", true);
                status.Add("isEncoded", true);

            }
            else
            {
                status.Add("isProxy", false);
                status.Add("isEncoded", false);
            }

            return status;
        }

        /**
        * BuildParams encodes each K,V pair. If key ends with 64, Base64 encode 
        * the value. Also encodes "+$:? " reserved characters, which is standard
        * for imgix libraries. It returns a formatted string which conforms to
        * "{encodedKey}={encodedVal}".
        */
        private String BuildParams(Dictionary<String, String> parameters)
        {
            Boolean hasLibParam = parameters.TryGetValue("ixlib", out String hasLib);

            if (IncludeLibraryParam && !hasLibParam)
            {
                parameters.Add("ixlib", String.Format("csharp-{0}", typeof(UrlBuilder).GetTypeInfo().Assembly.GetName().Version));
            }

            return parameters == null ?
                String.Empty :
                String.Join("&", parameters.Select(p =>
                {
                    String encodedKey = WebUtility.UrlEncode(p.Key);
                    encodedKey = encodedKey.Replace("+", "%20");
                    String encodedVal;

                    if (p.Key.EndsWith("64"))
                    {
                        encodedVal = Base64Encode(p.Value);
                    }
                    else
                    {
                        encodedVal = UrlEncode(p.Value);
                    }
                    return String.Format("{0}={1}", encodedKey, encodedVal);
                }
                ));
        }

        /*
        * Base64Encode encodes a string `value` to the Base64 format. It does 
        * this by calling the `System.Convert.ToBase64String` method on `value`
        * and additionally encoding reserved, `=/+` characters.
        */
        private String Base64Encode(String value)
        {
            String encodedVal;
            Byte[] valBytes = System.Text.Encoding.UTF8.GetBytes(value);
            encodedVal = System.Convert.ToBase64String(valBytes);
            encodedVal = encodedVal.Replace("=", "");
            encodedVal = encodedVal.Replace("/", "_");
            encodedVal = encodedVal.Replace("+", "-");
            return encodedVal;
        }

        /*
        * UrlEncode encodes a string `value` to the UTF8 format. It does 
        * this by calling the `WebUtility.UrlEncode` method on `value` and
        * doing some additional encoding on replacement reserved, `+:?#`,
        * and decoding on delimiter, `~`, characters.
        */
        private String UrlEncode(String value)
        {
            String encodedVal;

            encodedVal = WebUtility.UrlEncode(value);
            encodedVal = encodedVal.Replace("+", "%20");
            encodedVal = encodedVal.Replace(":", "%3A");
            encodedVal = encodedVal.Replace("?", "%3F");
            encodedVal = encodedVal.Replace("#", "%23");
            encodedVal = encodedVal.Replace("%7E", "~");

            return encodedVal;
        }


        public String BuildSrcSet(String path)
        {
            return BuildSrcSet(path, new Dictionary<string, string>());
        }

        public String BuildSrcSet(String path, Dictionary<String, String> parameters)
        {
            return BuildSrcSet(path, parameters, MinWidth, MaxWidth, SrcSetWidthTolerance);
        }

        /// <summary>
        /// Create a srcset attribute by disabling variable output quality.
        ///
        /// Variable output quality for dpr-based srcset attributes
        /// is _on by default_. Setting `disableVariableQuality` to
        /// `true` disables this.
        /// 
        /// </summary>
        /// <param name="path">path to the image, i.e. "image/file.png"</param>
        /// <param name="parameters">dictionary of query parameters</param>
        /// <param name="disableVariableQuality">toggle variable quality output on
        /// (default/false) or off (true).</param>
        /// <returns>srcset attribute string</returns>
        public String BuildSrcSet(
            String path,
            Dictionary<String, String> parameters,
            Boolean disableVariableQuality = false)
        {
            // Pass off to 6-param overload.
            return BuildSrcSet(
                path,
                parameters,
                MinWidth,
                MaxWidth,
                SrcSetWidthTolerance,
                disableVariableQuality);
        }

        /// <summary>
        /// Create a srcset attribute by specifying `tol`erance.
        /// </summary>
        /// <param name="path">path to the image, i.e. "image/file.png"</param>
        /// <param name="parameters">dictionary of query parameters</param>
        /// <param name="tol">tolerable amount of width value variation, 1-100.</param>
        /// <returns>srcset attribute string</returns>
        public String BuildSrcSet(
            String path,
            Dictionary<String, String> parameters,
            double tol = SrcSetWidthTolerance)
        {
            return BuildSrcSet(path, parameters, MinWidth, MaxWidth, tol);
        }

        /// <summary>
        /// Create a srcset attribute by specifying `begin` and `end`.
        ///
        /// This method creates a srcset attribute string whose image width
        /// values range between `begin` and `end`.
        /// 
        /// </summary>
        /// <param name="begin">beginning (min) image width value</param>
        /// <param name="end">ending (max) image width value</param>
        /// <returns>srcset attribute string</returns>
        public String BuildSrcSet(
            String path,
            Dictionary<String, String> parameters,
            int begin = MinWidth,
            int end = MaxWidth)
        {
            return BuildSrcSet(path, parameters, begin, end, SrcSetWidthTolerance);
        }

        /// <summary>
        /// Create a srcset attribute by specifying `begin`, `end`, and `tol`.
        ///
        /// This method creates a srcset attribute string whose image width
        /// values range between `begin` and `end` with the specified amount
        /// `tol`erance between each.
        /// 
        /// </summary>
        /// <param name="begin">beginning (min) image width value</param>
        /// <param name="end">ending (max) image width value</param>
        /// <param name="tol">tolerable amount of width value variation, 1-100.</param>
        /// <returns>srcset attribute string</returns>
        public String BuildSrcSet(
            String path,
            Dictionary<String, String> parameters,
            int begin = MinWidth,
            int end = MaxWidth,
            double tol = SrcSetWidthTolerance)
        {
            return BuildSrcSet(path, parameters, begin, end, tol, false);
        }

        /// <summary>
        /// Create a srcset attribute by specifying a list of target widths.
        /// </summary>
        /// <param name="path">path to the image, i.e. "image/file.png"</param>
        /// <param name="parameters">dictionary of query parameters</param>
        /// <param name="targets">list of target widths</param>
        /// <returns>srcset attribute string</returns>
        public String BuildSrcSet(String path, Dictionary<String, String> parameters, List<int> targets)
        {
            Validator.ValidateWidths(targets);
            return GenerateSrcSetPairs(path, parameters, targets: targets);
        }

        /// <summary>
        /// Create a srcset attribute.
        ///
        /// If the `parameters` indicate the srcset attribute should be
        /// dpr-based, then a dpr-based srcset is created.
        ///
        /// Otherwise, a viewport based srcset attribute is created with
        /// with the target widths that are generated by using `begin`,
        /// `end`, and `tol`.
        /// </summary>
        /// <param name="path">path to the image, i.e. "image/file.png"</param>
        /// <param name="parameters">dictionary of query parameters</param>
        /// <param name="begin">beginning (min) image width value</param>
        /// <param name="end">ending (max) image width value</param>
        /// <param name="tol">tolerable amount of width value variation, 1-100.</param>
        /// <param name="disableVariableQuality">toggle variable quality output on
        /// (default/false) or off (true).</param>
        /// <returns>srcset attribute string</returns>
        public String BuildSrcSet(
            String path,
            Dictionary<String, String> parameters,
            int begin = MinWidth,
            int end = MaxWidth,
            double tol = SrcSetWidthTolerance,
            Boolean disableVariableQuality = false)
        {

            if (IsDpr(parameters))
            {
                return GenerateSrcSetDPR(path, parameters, disableVariableQuality);
            }
            else
            {
                List<int> targets = GenerateTargetWidths(begin: begin, end: end, tol: tol);
                return GenerateSrcSetPairs(path, parameters, targets: targets);
            }
        }

        private String
            GenerateSrcSetDPR(
                String path,
                Dictionary<String, String> parameters,
                Boolean disableVariableQuality)
        {
            String srcset = "";
            var srcSetParams = new Dictionary<String, String>(parameters);

            // If a "q" is present, it will be applied whether or not
            // variable quality has been disabled.
            Boolean hasQuality = parameters.TryGetValue("q", out String q);

            foreach (int ratio in DprRatios)
            {
                if (!disableVariableQuality && !hasQuality)
                {
                    srcSetParams["q"] = DprQualities[ratio - 1].ToString();
                }
                srcSetParams["dpr"] = ratio.ToString();
                srcset += BuildUrl(path, srcSetParams) + " " + ratio.ToString() + "x,\n";
            }

            return srcset.Substring(0, srcset.Length - 2);
        }

        private String GenerateSrcSetPairs(
            String path,
            Dictionary<String, String> parameters,
            List<int> targets = null)
        {
            if (targets == null)
            {
                targets = SrcSetTargetWidths.ToList();
            }

            String srcset = "";

            foreach (int width in targets)
            {
                parameters["w"] = width.ToString();
                srcset += BuildUrl(path, parameters) + " " + width + "w,\n";
            }

            return srcset.Substring(0, srcset.Length - 2);
        }

        /// <summary>
        /// Create a `List` of integer target widths.
        ///
        /// If `begin`, `end`, and `tol` are the default values, then the
        /// targets are not custom, in which case the default widths are
        /// returned.
        ///
        /// A target width list of length one is valid: if `begin` == `end`
        /// then the list begins where it ends.
        /// 
        /// When the `while` loop terminates, `begin` is greater than `end`
        /// (where `end` less than or equal to `MAX_WIDTH`). This means that
        /// the most recently appended value may, or may not, be the `end`
        /// value.
        /// 
        /// To be inclusive of the ending value, we check for this case and the
        /// value is added if necessary.
        /// </summary>
        /// <param name="begin">beginning (min) image width value</param>
        /// <param name="end">ending (max) image width value</param>
        /// <param name="tol">tolerable amount of width value variation, 1-100.</param>
        /// <returns>list of image width values</returns>        
        public static List<int>
            GenerateTargetWidths(
                int begin = MinWidth,
                int end = MaxWidth,
                double tol = SrcSetWidthTolerance)
        {
            Validator.ValidateMinMaxTol(begin, end, tol);
            return ComputeTargetWidths(begin, end, tol);
        }

        public static List<int> GenerateEvenTargetWidths()
        {
            return SrcSetTargetWidths.ToList();
        }

        /**
         * Create an `ArrayList` of integer target widths.
         *
         * This function is the implementation details of `targetWidths`.
         * This function exists to provide a consistent interface for
         * callers of `targetWidths`.
         *
         * This function implements the syntax that fulfills the semantics
         * of `targetWidths`. Meaning, `begin`, `end`, and `tol` are
         * to be whole integers, but computation requires `double`s. This
         * function hides this detail from callers.
         */
        private static List<int> ComputeTargetWidths(double begin, double end, double tol)
        {
            if (NotCustom(begin, end, tol))
            {
                // If not custom, return the default target widths.
                return SrcSetTargetWidths.ToList();
            }

            if (begin == end)
            {
                return new List<int> { (int)Math.Round(begin) };
            }

            List<int> resolutions = new List<int>();
            while (begin < end && begin < MaxWidth)
            {
                resolutions.Add((int)Math.Round(begin));
                begin *= 1 + tol * 2;
            }

            if (resolutions.Last() < end)
            {
                resolutions.Add((int)Math.Round(end));
            }

            return resolutions;

        }

        private static Boolean IsDpr(Dictionary<String, String> parameters)
        {
            Boolean hasWidth = parameters.TryGetValue("w", out String width);
            Boolean hasHeight = parameters.TryGetValue("h", out String height);

            // If `params` have a width param or height parameters then the
            // srcset to be constructed with these params _is dpr based_
            return hasWidth || hasHeight;
        }

        private static Boolean NotCustom(double begin, double end, double tol)
        {
            Boolean defaultBegin = (begin == MinWidth);
            Boolean defaultEnd = (end == MaxWidth);
            Boolean defaultTol = (tol == SrcSetWidthTolerance);

            // A list of target widths is _NOT_ custom if `begin`, `end`,
            // and `tol` are equal to their default values.
            return defaultBegin && defaultEnd && defaultTol;
        }

        private String HashString(String input)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(input.Select(Convert.ToByte).ToArray())).Replace("-", "").ToLower();
        }
    }
}
