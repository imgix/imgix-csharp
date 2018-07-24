using NUnit.Framework;
using Imgix;

namespace Imgix.Tests
{
    [TestFixture]
    public class Crc32Test
    {
        [Test]
        public void Crc32()
        {
            var crc = new Crc32();
            var hash = crc.ComputeCrcHash("abc123");

            Assert.AreEqual("cf02bb5c", hash.ToString("X").ToLower());
        }
    }
}
