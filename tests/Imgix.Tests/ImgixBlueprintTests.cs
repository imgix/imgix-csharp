using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imgix;
using NUnit.Framework;

namespace Imgix.Tests
{
    /* This fixture exists to show that the library conforms to all requirements as defined in the
     * imgix client blueprint repo here: https://github.com/imgix/imgix-blueprint
     */

    [TestFixture]
    public class ImgixBlueprintTests
    {
        [Test]
        public void UrlBuilderHandlesBasicPathsProperly()
        {
            var test = new UrlBuilder("my-social-network.imgix.net", useHttps: true, includeLibraryParam: false);

            Assert.AreEqual("https://my-social-network.imgix.net/users/1.png", test.BuildUrl("/users/1.png"));
        }

        [Test]
        public void UrlBuilderDoesnotMerelyAppend()
        {
            var test = new UrlBuilder("my-social-network.imgix.net", useHttps: true, includeLibraryParam: false);

            Assert.AreNotEqual("https://my-social-network.imgix.net/http://avatars.com/john-smith.png", test.BuildUrl("http://avatars.com/john-smith.png"));
        }

        [Test]
        public void UrlBuilderProperlyEncodesAbsolutePaths()
        {
            var test = new UrlBuilder("my-social-network.imgix.net", useHttps: true, includeLibraryParam: false);

            Assert.AreEqual("https://my-social-network.imgix.net/http%3A%2F%2Favatars.com%2Fjohn-smith.png", test.BuildUrl("http://avatars.com/john-smith.png"));
        }

        [Test]
        public void UrlBuilderAppendsQueryStringParameters()
        {
            var test = new UrlBuilder("my-social-network.imgix.net", useHttps: true, includeLibraryParam: false);

            var parameters = new Dictionary<String, String>();
            parameters.Add("w", "400");
            parameters.Add("h", "300");

            Assert.AreEqual("https://my-social-network.imgix.net/users/1.png?w=400&h=300", test.BuildUrl("/users/1.png", parameters));
        }

        [Test]
        public void UrlBuilderProperlySignsSimpleRequests()
        {
            var test = new UrlBuilder("my-social-network.imgix.net", useHttps: true, signKey: "FOO123bar", includeLibraryParam: false);

            Assert.AreEqual("https://my-social-network.imgix.net/users/1.png?s=6797c24146142d5b40bde3141fd3600c", test.BuildUrl("/users/1.png"));
        }

        [Test]
        public void UrlBuilderProperlySignsFullyQualifiedUrls()
        {
            var test = new UrlBuilder("my-social-network.imgix.net", useHttps: true, signKey:  "FOO123bar", includeLibraryParam: false);

            Assert.AreEqual("https://my-social-network.imgix.net/http%3A%2F%2Favatars.com%2Fjohn-smith.png?s=493a52f008c91416351f8b33d4883135", test.BuildUrl("/http%3A%2F%2Favatars.com%2Fjohn-smith.png"));
        }

        [Test]
        public void UrlBuilderProperlySignsSimplePathsWithParameters()
        {
            var test = new UrlBuilder("my-social-network.imgix.net", useHttps: true, signKey:  "FOO123bar", includeLibraryParam: false);

            var parameters = new Dictionary<String, String>();
            parameters.Add("w", "400");
            parameters.Add("h", "300");

            Assert.AreEqual("https://my-social-network.imgix.net/users/1.png?w=400&h=300&s=c7b86f666a832434dd38577e38cf86d1",
                            test.BuildUrl("/users/1.png", parameters));
        }

        [Test]
        public void UrlBuilderProperlySignsFullyQualifiedUrlsWithParameters()
        {
            var test = new UrlBuilder("my-social-network.imgix.net", useHttps: true, signKey: "FOO123bar", includeLibraryParam: false);

            var parameters = new Dictionary<String, String>();
            parameters.Add("w", "400");
            parameters.Add("h", "300");

            Assert.AreEqual("https://my-social-network.imgix.net/http%3A%2F%2Favatars.com%2Fjohn-smith.png?w=400&h=300&s=61ea1cc7add87653bb0695fe25f2b534",
                            test.BuildUrl("/http%3A%2F%2Favatars.com%2Fjohn-smith.png", parameters));
        }
    }
}
