using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imgix_CSharp
{
    public class URLBuilder
    {
        public const String VERSION = "0.0.1";

        public enum ShardStrategy
        {
            NONE,
            CRC,
            CYCLE
        }

        public String[] domains { get; set; }
        public Boolean useHttps { get; set; }
        public String signKey { get; set; }
        public ShardStrategy shardStrategy { get; set; }
        public Boolean signWithLibraryParameter { get; set; }

        private int shardCycleNextIndex = 0;

        public URLBuilder(String[] domains, Boolean useHttps, String signKey, ShardStrategy shardStrategy,
            Boolean signWithLibraryParameter)
        {
            if (domains == null || domains.Length == 0)
            {
                throw new ArgumentException("At least one domain must be passed into URLBuilder.");
            }

            this.domains = domains;
            this.useHttps = useHttps;
            this.signKey = signKey;
            this.shardStrategy = shardStrategy;
            this.signWithLibraryParameter = signWithLibraryParameter;
        }


        public URLBuilder(String domain) : this(new[] { domain }, false, String.Empty, ShardStrategy.NONE, false)
        {
        }

        public URLBuilder(String[] domain) : this(domain, false, String.Empty, ShardStrategy.NONE, false)
        {
        }

        public URLBuilder(String domain, Boolean useHttps) : this(new[] { domain }, useHttps, String.Empty, ShardStrategy.NONE, false)
        {
        }

        public URLBuilder(String[] domain, Boolean useHttps) : this(domain, useHttps, String.Empty, ShardStrategy.NONE, false)
        {
        }

        public URLBuilder(String domain, Boolean useHttps, String signKey) : this(new[] { domain }, useHttps, signKey, ShardStrategy.CRC, true)
        {
        }

        public URLBuilder(String[] domain, Boolean useHttps, String signKey) : this(domain, useHttps, signKey, ShardStrategy.CRC, true)
        {
        }

        public URLBuilder(String domain, Boolean useHttps, String signKey, ShardStrategy shardStrategy) : this(new[] { domain }, useHttps, signKey, shardStrategy, true)
        {
        }

        public String CreateUrl(String path)
        {
            return CreateUrl(path, new Dictionary<String, String>());
        }

        public String CreateUrl(String path, Dictionary<String, String> parameters)
        {
            String scheme = this.useHttps ? "https" : "http";

            String domain;

            if (shardStrategy == ShardStrategy.CRC)
            {
                var crc = new Crc32();
                var hash = crc.ComputeHash(path.Select(Convert.ToByte).ToArray()).ToString().ToLower();

                var index = (int) (hash%domains.Length);
            }

            else if (shardStrategy == ShardStrategy.CYCLE)
            {

            }

            else
            {
                domain = domains[0];
            }

            if (signWithLibraryParameter)
            {
                //parameters.Add("ixlib", "csharp-" + VERSION);
            }

            return String.Empty;
        }
    }
}
