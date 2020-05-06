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

        private static readonly List<int> SRCSET_TARGET_WIDTHS = GenerateTargetWidths();

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

        public String BuildUrl(String path, Dictionary<String, String> parameters)
        {

            if (path.StartsWith("http"))
            {
                path = WebUtility.UrlEncode(path);
            }

            Boolean hasLibParam = parameters.TryGetValue("ixlib", out String hasLib);
            if (IncludeLibraryParam && !hasLibParam)
            {
                parameters.Add("ixlib", String.Format("csharp-{0}", typeof(UrlBuilder).GetTypeInfo().Assembly.GetName().Version));
            }

            return GenerateUrl(Domain, path, parameters);
        }

        public String BuildSrcSet(String path)
        {
            return BuildSrcSet(path, new Dictionary<string, string>());
        }

        public String BuildSrcSet(String path, Dictionary<String, String> parameters)
        {
            String srcset;
            parameters.TryGetValue("w", out String width);
            parameters.TryGetValue("h", out String height);
            parameters.TryGetValue("ar", out String aspectRatio);

            if ((width != null) || (height != null && aspectRatio != null))
            {
                srcset = GenerateSrcSetDPR(Domain, path, parameters);
            }
            else
            {
                srcset = GenerateSrcSetPairs(Domain, path, parameters);
            }

            return srcset;
        }

        private String GenerateUrl(String domain, String path, Dictionary<String, String> parameters)
        {
            String scheme = UseHttps ? "https" : "http";
            path = path.TrimEnd('/').TrimStart('/');

            var qs = GenerateUrlStringFromDict(parameters);
            var localParams = new Dictionary<String, String>(parameters);

            if (!String.IsNullOrEmpty(_signKey))
            {
                var hashString = String.Format("{0}/{1}{2}", _signKey, path, localParams.Any() ? "?" + qs : String.Empty);
                localParams.Add("s", HashString(hashString));
            }

            return String.Format("{0}://{1}/{2}{3}", scheme, domain, path, localParams.Any() ? "?" + GenerateUrlStringFromDict(localParams) : String.Empty);
        }

        private String GenerateSrcSetDPR(String domain, String path, Dictionary<String, String> parameters)
        {
            String srcset = "";
            int[] targetRatios = { 1, 2, 3, 4, 5 };

            foreach(int ratio in targetRatios)
            {
                parameters["dpr"] = ratio.ToString();
                srcset += BuildUrl(path, parameters) + " " + ratio.ToString()+ "x,\n";
            }

            return srcset.Substring(0, srcset.Length - 2);
        }

        private String GenerateSrcSetPairs(String domain, String path, Dictionary<String, String> parameters)
        {
            String srcset = "";

            foreach(int width in SRCSET_TARGET_WIDTHS)
            {
                parameters["w"] = width.ToString();
                srcset += BuildUrl(path, parameters) + " " + width + "w,\n";
            }

            return srcset.Substring(0, srcset.Length - 2);
        }

        public static List<int>
            GenerateTargetWidths(
                double start = 100, double stop = 8192, double tol = 8)
        {
            List<int> resolutions = new List<int>();
            int MAX_SIZE = 8192;

            while (start < stop && start < MAX_SIZE)
            {
                resolutions.Add(MakeEven(start));
                start *= 1 + (tol / 100) * 2;
            }

            if (resolutions.Last() < stop)
            {
                resolutions.Add(MakeEven(stop));
            }

            return resolutions;
        }

        private static int MakeEven(double value)
        {
            return (int)(2 * Math.Round(value / 2));
        }

        private String HashString(String input)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(input.Select(Convert.ToByte).ToArray())).Replace("-", "").ToLower();
        }

        private String GenerateUrlStringFromDict(Dictionary<String, String> queryDictionary)
        {
            return queryDictionary == null ?
                String.Empty :
                String.Join("&", queryDictionary.Select(p =>
                    {
                        String encodedKey = WebUtility.UrlEncode(p.Key);
                        encodedKey = encodedKey.Replace("+", "%20");
                        String encodedVal;

                        if (p.Key.EndsWith("64")) {
                            Byte[] valBytes = System.Text.Encoding.UTF8.GetBytes(p.Value);
                            encodedVal = System.Convert.ToBase64String(valBytes);
                            encodedVal = encodedVal.Replace("=", "");
                            encodedVal = encodedVal.Replace("/", "_");
                            encodedVal = encodedVal.Replace("+", "-");
                        } else {
                            encodedVal = WebUtility.UrlEncode(p.Value);
                            encodedVal = encodedVal.Replace("+", "%20");
                        }

                        return String.Format("{0}={1}", encodedKey, encodedVal);
                    }
                ));
        }
    }
}
