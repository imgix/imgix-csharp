using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imgix_CSharp;

namespace ClientDriver.cs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new URLBuilder("bensterrett.imgix.net", false);

            var url = builder.CreateUrl("gaimain.jpeg");

            Console.WriteLine(url);
        }
    }
}
