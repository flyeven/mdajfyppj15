using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerCore
{
    class CrawlerConfiguration
    {

        public static string CONFIG_PATH = @"Config\config.json";
        public static string REMOTE_CONFIG_URL = "http://localhost:5066/api/crawlerconfig";
        private static CrawlerConfiguration instance;
        private JObject Config;
        public static string ROOT="";
        
        private CrawlerConfiguration()
        {
            ROOT = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            StreamReader Reader;
            Reader = new StreamReader(
                         Path.Combine(ROOT, CONFIG_PATH)
                        );
            string json = "";
            string line = "";

            while ((line = Reader.ReadLine()) != null)
            {
                json += line;
            }

            Reader.Close();
            this.Config = JObject.Parse(json);
        
            ReloadConfiguration();

        }



        public void ReloadConfiguration()
        {
            HttpWebRequest Request = (HttpWebRequest)HttpWebRequest.Create(REMOTE_CONFIG_URL);           
            Request.Method = "GET";
            Request.Proxy = null;

            WebResponse Response = Request.GetResponse();
            var ResponseJSON = (JObject) new JsonSerializer().Deserialize(new JsonTextReader(new StreamReader(Response.GetResponseStream())));
            this.Config["rules"] = ResponseJSON["tags"];
            this.Config["sites"] = ResponseJSON["sites"];
            StreamWriter Writer = new StreamWriter(new FileStream(Path.Combine(ROOT, CONFIG_PATH),FileMode.Create));
            Writer.Write(this.Config.ToString());
            Writer.Close();
            Response.Close();
        }


        public JEnumerable<JToken> Rules()
        {
            return this.Config.GetValue("rules").Children();
        }

        public JArray Splitters()
        {
            return (JArray)this.Config.GetValue("splitters");
        }

        public JArray Sites()
        {
            return (JArray)this.Config.GetValue("sites");
        }

        public JArray Elements()
        {
            return (JArray)this.Config.GetValue("elements");
        }

        public JToken Get(string property)
        {
            return this.Config.GetValue(property);
        }

        public static CrawlerConfiguration GetInstance()
        {
            if (instance == null)
            {
                instance = new CrawlerConfiguration();
            }
            return instance;
        }
    }
}
