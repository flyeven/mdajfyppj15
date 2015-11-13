using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Inserting user");
            using (var db = new CrawlerDataContext()) {
                SiteUser user = new SiteUser() {
                    username = "mudasir.ali",
                    password = "admin1234",
                    email = "mudasir.ale@gmail.com",
                    fullname = "Mudasir Ali" 
                };

                db.SiteUsers.Add(user);
                db.SaveChanges();                
            }
            Console.WriteLine("User inserted");
            Console.ReadLine();
        }
    }
}
