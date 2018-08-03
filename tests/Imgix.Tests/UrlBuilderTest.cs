using System;
using System.Collections.Generic;
using System.Linq;
using Imgix;
using NUnit.Framework;

namespace Imgix.Tests
{
    [TestFixture]
    public class UrlBuilderTest
    {
        private String SignKey = "aaAAbbBB11223344";

        [Test]
        public void UrlBuilderBuildsBasicUrlHttp()
        {
            var test = new UrlBuilder("domain.imgix.net", signWithLibrary: false);

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain.imgix.net/gaiman.jpg");
        }

        [Test]
        public void UrlBuilderBuildsBasicUrlHttps()
        {
            var test = new UrlBuilder("domain.imgix.net", useHttps: true, signWithLibrary: false);

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain.imgix.net/gaiman.jpg");
        }

        [Test]
        public void UrlBuilderBuildsQueryStringUrlHttp()
        {
            var test = new UrlBuilder("domain.imgix.net", signWithLibrary: false);

            var parameters = new Dictionary<String, String>();
            parameters["w"] = "500";
            parameters["blur"] = "100";

            Assert.AreEqual(test.BuildUrl("gaiman.jpg", parameters), "https://domain.imgix.net/gaiman.jpg?w=500&blur=100");
        }

        [Test]
        public void UrlBuilderBuildsQueryStringUrlHttps()
        {
            var test = new UrlBuilder("domain.imgix.net", useHttps: true, signWithLibrary: false);

            var parameters = new Dictionary<String, String>();
            parameters["w"] = "500";
            parameters["blur"] = "100";

            Assert.AreEqual(test.BuildUrl("gaiman.jpg", parameters), "https://domain.imgix.net/gaiman.jpg?w=500&blur=100");
        }

        [Test]
        public void UrlBuilderUsesCRCShardingBydefault()
        {
            var domains = new [] { "domain1.imgix.net",  "domain2.imgix.net" };
            var test = new UrlBuilder(domains, signWithLibrary: false);

            Assert.True(test.BuildUrl("/users/1.png").Contains(domains[0]));
            Assert.True(test.BuildUrl("/users/1.png").Contains(domains[0]));
            Assert.True(test.BuildUrl("/users/2.png").Contains(domains[0]));
            Assert.True(test.BuildUrl("/users/2.png").Contains(domains[0]));
            Assert.True(test.BuildUrl("/users/a.png").Contains(domains[1]));
            Assert.True(test.BuildUrl("/users/a.png").Contains(domains[1]));
            Assert.True(test.BuildUrl("/users/a.png").Contains(domains[1]));
            Assert.True(test.BuildUrl("/users/a.png").Contains(domains[1]));
        }

        [Test]
        public void UrlBuilderClonesDomainList()
        {
            var domains = new[] { "domain1.imgix.net", "domain2.imgix.net" };
            var test = new UrlBuilder(domains, signWithLibrary: false);
            domains[0] = "domain3.imgix.net";
            domains[1] = "domain4.imgix.net";

            Assert.False(test.BuildUrl("/users/1.png").Contains(domains[0]));
            Assert.False(test.BuildUrl("/users/1.png").Contains(domains[0]));
            Assert.False(test.BuildUrl("/users/2.png").Contains(domains[0]));
            Assert.False(test.BuildUrl("/users/2.png").Contains(domains[0]));
            Assert.False(test.BuildUrl("/users/a.png").Contains(domains[1]));
            Assert.False(test.BuildUrl("/users/a.png").Contains(domains[1]));
            Assert.False(test.BuildUrl("/users/a.png").Contains(domains[1]));
            Assert.False(test.BuildUrl("/users/a.png").Contains(domains[1]));
        }

        [Test]
        public void UrlBuilderSignsParameterlessRequests()
        {
            var test = new UrlBuilder("domain.imgix.net", signKey: SignKey, signWithLibrary: false);

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain.imgix.net/gaiman.jpg?s=db6110637ad768e4b1d503cb96e6439a");
        }

        [Test]
        public void UrlBuilderSignsParameteredRequests()
        {
            var test = new UrlBuilder("domain.imgix.net", signKey: SignKey, signWithLibrary: false);

            var parameters = new Dictionary<String, String>();
            parameters.Add("w", "500");
            parameters.Add("h", "1000");

            Assert.AreEqual(test.BuildUrl("gaiman.jpg", parameters), "https://domain.imgix.net/gaiman.jpg?w=500&h=1000&s=fc4afbc39b6741560717142aeada876c");
        }

        [Test]
        public void UrlBuilderSignsNestedPaths()
        {
            var test = new UrlBuilder("domain.imgix.net", signKey: SignKey, signWithLibrary: false);

            Assert.AreEqual(test.BuildUrl("test/gaiman.jpg"), "https://domain.imgix.net/test/gaiman.jpg?s=51033c27726f19c0f8229a1ed2dc8523");
        }

        [Test]
        public void UrlBuilderWithMultipleDomainsPicksTheFirstWhenShardTypeNull()
        {
            var domains = new[] { "domain.imgix.net", "domain2.imgix.net", "domain3.imgix.net" };

            var test = new UrlBuilder(domains, signWithLibrary: false)
            {
                ShardStrategy = null
            };

            Assert.AreEqual(test.BuildUrl("/users/1.png"), "https://domain.imgix.net/users/1.png");
            Assert.AreEqual(test.BuildUrl("/users/2.png"), "https://domain.imgix.net/users/2.png");
            Assert.AreEqual(test.BuildUrl("/users/a.png"), "https://domain.imgix.net/users/a.png");
        }

        [Test]
        public void UrlBuilderWithMultipleDomainCyclesThroughDomains()
        {
            var domains = new[] {"domain.imgix.net", "domain2.imgix.net", "domain3.imgix.net"};

            var test = new UrlBuilder(domains, shardStrategy: UrlBuilder.ShardStrategyType.CYCLE, signWithLibrary: false);

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain.imgix.net/gaiman.jpg");
            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain2.imgix.net/gaiman.jpg");
            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain3.imgix.net/gaiman.jpg");
            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain.imgix.net/gaiman.jpg");
        }

        [Test]
        public void UrlBuilderWithMultipleDomainsSelectsServerByCRC()
        {
            var domains = new[] { "domain.imgix.net", "domain2.imgix.net", "domain3.imgix.net" };
            var crcs = new [] { "test1.png", "test2.png", "test3.png" }.Select(i => Convert.ToInt32(new Crc32().ComputeCrcHash(i) % domains.Length)).ToArray();

            var test = new UrlBuilder(domains, shardStrategy: UrlBuilder.ShardStrategyType.CRC, signWithLibrary: false);

            Assert.AreEqual(test.BuildUrl("test1.png"), String.Format("https://{0}/test1.png", domains[crcs[0]]));
            Assert.AreEqual(test.BuildUrl("test2.png"), String.Format("https://{0}/test2.png", domains[crcs[1]]));
            Assert.AreEqual(test.BuildUrl("test3.png"), String.Format("https://{0}/test3.png", domains[crcs[2]]));
        }

        [Test]
        public void UrlBuilderEscapesParamKeys()
        {
            var test = new UrlBuilder("demo.imgix.net", signWithLibrary: false);

            var parameters = new Dictionary<String, String>();
            parameters["hello world"] = "interesting";

            Assert.AreEqual("https://demo.imgix.net/demo.png?hello%20world=interesting", test.BuildUrl("demo.png", parameters));
        }

        [Test]
        public void UrlBuilderEscapesParamValues()
        {
            var test = new UrlBuilder("demo.imgix.net", signWithLibrary: false);

            var parameters = new Dictionary<String, String>();
            parameters["hello_world"] = "/foo\"> <script>alert(\"hacked\")</script><";

            Assert.AreEqual("https://demo.imgix.net/demo.png?hello_world=%2Ffoo%22%3E%20%3Cscript%3Ealert(%22hacked%22)%3C%2Fscript%3E%3C", test.BuildUrl("demo.png", parameters));
        }

        [Test]
        public void UrlBuilderBase64EncodesBase64ParamVariants()
        {
            var test = new UrlBuilder("demo.imgix.net", signWithLibrary: false);

            var parameters = new Dictionary<String, String>();
            parameters["txt64"] = "I cannøt belîév∑ it wors! \ud83d\ude31";

            Assert.AreEqual("https://demo.imgix.net/~text?txt64=SSBjYW5uw7h0IGJlbMOuw6l24oiRIGl0IHdvcu-jv3MhIPCfmLE", test.BuildUrl("~text", parameters));
        }
    }
}
