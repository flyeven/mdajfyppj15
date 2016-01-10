using Newtonsoft.Json.Linq;
using System;
using System.Net.Mail;

namespace CrawlerCore
{
    class Program
    {



      


        public static void SendEmail(string Subject, string Body, string to)
        {
            MailMessage mail = new MailMessage("mudasir.ale@gmail.com", "mudasir.ali@outlook.com");
            SmtpClient client = new SmtpClient("smtp.gmail.com",587);


            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            
            mail.Subject = "Student Portal Verification";
            mail.Body = "this is my test email body";
            mail.IsBodyHtml = true;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential("mudasir.ale@gmail.com", "Mudas!ralE"); ;
            client.UseDefaultCredentials = false;
            client.Send(mail);
        }




        static void Main(string[] args)
        {

            string a = Crypto.Encrypt("mudasiralii", "KEY332022");
            Console.WriteLine(Uri.EscapeDataString(a));
            Console.WriteLine(Crypto.Decrypt(a,"KEY332022"));
            Console.ReadLine();
            /*
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
             Environment.Exit(0);*/
        }
    }
}
