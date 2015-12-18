using System;
using System.IO;

namespace CrawlerCore
{
    class Program
    {
        static void Main(string[] args)
        {

            CrawlerAPI api = new CrawlerAPI();
            //api.MaxAnalyzed = 20;
            // api.Start();

            CrawlerAPI.Insert(File.Open(@"D:\CrawlerOut.json",FileMode.Open));
            Console.ReadLine();
        }
    }
}
