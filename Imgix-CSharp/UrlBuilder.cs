using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Cryptography;

namespace Imgix_CSharp
{
    public class UrlBuilder
    {
        public enum ShardStrategyType
        {
            NONE,
            CRC,
            CYCLE
        }

        public String[] Domains { get; private set; }
        public Boolean UseHttps { get; private set; }

        public Dictionary<String, String> Parameters = new Dictionary<string, string>();

        public String SignKey { get; set; }
        public ShardStrategyType ShardStrategy;
        public Boolean SignWithLibrary { get; set; }

        private int ShardCycleIndex = 0;

        public UrlBuilder(String[] domains, Boolean useHttps = false)
        {
            if (useHttps && ShardStrategy == ShardStrategyType.NONE)
            {
                ShardStrategy = ShardStrategyType.CRC;
            }

            UseHttps = useHttps;
            Domains = domains;
        }

        public UrlBuilder(String domain, Boolean useHttps = false)
        {
            if (useHttps && ShardStrategy == ShardStrategyType.NONE)
            {
                ShardStrategy = ShardStrategyType.CRC;
            }

            UseHttps = useHttps;
            Domains = new[] { domain };
        }

        public String BuildUrl(String path)
        {

            if (path.StartsWith("http"))
            {
                path = WebUtility.UrlEncode(path);
            }

            int index = 0;

            if (ShardStrategy == ShardStrategyType.CRC)
            {
                var c = new Crc32();
                var hash = c.ComputeCrcHash(path);

                index = ((int)hash)%Domains.Length;
            }

            else if (ShardStrategy == ShardStrategyType.CYCLE)
            {
                index = (ShardCycleIndex++)%Domains.Length;
            }

            var domain = Domains.ElementAt(index);

            if (SignWithLibrary)
            {
                Parameters.Add("ixlib", String.Format("csharp-{0}", Assembly.GetExecutingAssembly().GetName().Version));
            }

            return GenerateUrl(path, domain);
        }


        private String GenerateUrl(String path, String domain)
        {
            String scheme = UseHttps ? "https" : "http";
            path = path.TrimEnd('/').TrimStart('/');
            
            var qs = GenerateUrlStringFromDict(Parameters);
            var localParams = new Dictionary<String, String>(Parameters);

            if (!String.IsNullOrEmpty(SignKey))
            {
                var hashString = String.Format("{0}/{1}{2}", SignKey, path, localParams.Any() ? "?" + qs : String.Empty);
                localParams.Add("s", HashString(hashString));
            }

            return String.Format("{0}://{1}/{2}{3}", scheme, domain, path, localParams.Any() ? "?" + GenerateUrlStringFromDict(localParams) : String.Empty);
        }


        private String HashString(String input)
        {
            return new SoapHexBinary(MD5.Create().ComputeHash(input.Select(Convert.ToByte).ToArray())).ToString().ToLower();
        }

        private String GenerateUrlStringFromDict(Dictionary<String, String> queryDictionary)
        {
            return queryDictionary == null ? 
                String.Empty : 
				String.Join("&", queryDictionary.Select(p =>
					{
						String encodedKey = WebUtility.UrlEncode(p.Key);
						String encodedVal;

						if (p.Key.EndsWith("64") {
							Byte[] valBytes = System.Text.Encoding.UTF8.GetBytes(p.Value);
							encodedVal = System.Convert.ToBase64String(valBytes);
							encodedVal = encodedVal.Replace("=", "");
							encodedVal = encodedVal.Replace("/", "_");
							encodedVal = encodedVal.Replace("+", "-");
						} else {
							encodedVal = WebUtility.UrlEncode(p.Value);
						}

						String.Format("{0}={1}", encodedKey, encodedVal)
					}
				));
        }
    }
}
