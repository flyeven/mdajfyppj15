using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CrawlerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            string cmd = "";
            if (args == null || args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
            {
                cmd = "analyze";
            }
            else
            {
                cmd = args[0].ToLower();
            }


            CrawlerAPI api = new CrawlerAPI();
            //api.MaxAnalyzed = 20;
            // api.Start();
            if (!cmd.Equals("analyze"))
            {
                var response = api.CrawlAndUpdateSites();
                foreach (var rankings in response)
                {
                    api.Insert(JArray.FromObject(rankings));
                }
            }
            else
            {
                api.CrawlAndAnalyzeSites();
            }
            //Thread.Sleep(3000);
            //CrawlerAPI.Insert(File.Open(@"D:\CrawlerOut.json",FileMode.Open));
            Console.Write("Enter to exit: ");
            var a = Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
