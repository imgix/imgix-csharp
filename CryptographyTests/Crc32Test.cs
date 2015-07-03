using System;
using System.Linq;
using Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CryptographyTests
{
    [TestClass]
    public class Crc32Test
    {
        [TestMethod]
        public void Crc32()
        {
            var crc = new Crc32();
            var hash = crc.ComputeCrcHash("abc123");

            Assert.AreEqual("cf02bb5c", hash.ToString("X").ToLower());
        }
    }
}
