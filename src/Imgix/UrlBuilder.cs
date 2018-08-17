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
        public enum ShardStrategyType
        {
            CRC,
            CYCLE
        }

        public Boolean UseHttps;
        public Boolean IncludeLibraryParam;
        public ShardStrategyType? ShardStrategy;

        private String _signKey;
        public String SignKey { set { _signKey = value; } }

        private String[] Domains;

        private int ShardCycleIndex = 0;

        public UrlBuilder(String[] domains,
                          String signKey = null,
                          ShardStrategyType shardStrategy = ShardStrategyType.CRC,
                          Boolean includeLibraryParam = true,
                          Boolean useHttps = true)
        {
            Domains = (String []) domains.Clone();
            _signKey = signKey;
            ShardStrategy = shardStrategy;
            IncludeLibraryParam = includeLibraryParam;
            UseHttps = useHttps;
        }

        public UrlBuilder(String domain,
                          String signKey = null,
                          Boolean includeLibraryParam = true,
                          Boolean useHttps = true)
            : this(new[] { domain },
                   signKey: signKey,
                   useHttps: useHttps,
                   includeLibraryParam: includeLibraryParam)
        {
        }

        public UrlBuilder(String domain, Boolean useHttps)
            : this(domain, signKey: null, useHttps: useHttps)
        {
        }

        public UrlBuilder(String[] domains, Boolean useHttps)
            : this(domains, signKey: null, useHttps: useHttps)
        {
        }

        public UrlBuilder(String domain, String signKey, Boolean useHttps)
            : this(domain, signKey: signKey, includeLibraryParam: true, useHttps: useHttps)
        {
        }

        public UrlBuilder(String[] domains, String signKey, Boolean useHttps)
            : this(domains, signKey: signKey, includeLibraryParam: true, useHttps: useHttps)
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

            int index = 0;

            if (ShardStrategy == ShardStrategyType.CRC)
            {
                var c = new Crc32();
                index = (int) (c.ComputeCrcHash(path) % Domains.Length);
            }

            else if (ShardStrategy == ShardStrategyType.CYCLE)
            {
                index = (ShardCycleIndex++)%Domains.Length;
            }

            var domain = Domains[index];

            if (IncludeLibraryParam)
            {
                parameters.Add("ixlib", String.Format("csharp-{0}", typeof(UrlBuilder).GetTypeInfo().Assembly.GetName().Version));
            }

            return GenerateUrl(domain, path, parameters);
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
