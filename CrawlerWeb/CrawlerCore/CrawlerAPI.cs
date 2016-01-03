using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerCore
{
    class CrawlerAPI
    {
        public Queue<string> Links { get; private set; }
        public List<string> AnalyzedLinks { get; private set; }
        public int MaxAnalyzed { get; private set; }
        public int TimeOut { get; private set; }
        public string[] Elements { get; private set; }
        private CrawlerConfiguration Config;


        private TextProcessor processor;

        public CrawlerAPI() {
            this.Links = new Queue<string>();
            this.AnalyzedLinks = new List<string>();
            this.processor = new TextProcessor();
            this.Config = CrawlerConfiguration.GetInstance();
            this.MaxAnalyzed = Int32.Parse(this.Config.Get("max_links").ToString());
            this.TimeOut = Int32.Parse(this.Config.Get("request_timeout").ToString());
            this.Elements = this.Config.Elements().Select(e => e.ToString()).ToArray();
            if (this.Elements.Length < 1) {
                this.Elements = new string[] { "p" }; 
            }
        }

        string university_ranking_source = "http://www.webometrics.info/en/";
        public List<List<UniversitySiteDTO>> CrawlAndUpdateSites()
        {
            List<List<UniversitySiteDTO>> listResult = new List<List<UniversitySiteDTO>>();
            JArray regions = (JArray) this.Config.Get("regions");
            foreach (var r in regions)
            {
                HtmlDocument doc = RetrieveHtmlDoc(university_ranking_source + r.ToString().Replace(" ","%20"));
                if (doc != null) {
                    Console.WriteLine("Getting ranking of universities for "+r);
                    string country = r.ToString().Split('/')[1];
                    var tables = doc.DocumentNode.Descendants("table").Where(d =>
                                    d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("sticky-enabled")
                                );
                    if (tables.Count() == 1) {
                        HtmlNode table = tables.First();
                        var rows = table.Element("tbody").Descendants("tr");
                        int rowno = 1;
                        List<UniversitySiteDTO> ranking = new List<UniversitySiteDTO>();
                        foreach (var row in rows)
                        {
                            UniversitySiteDTO dto = new UniversitySiteDTO();
                            var columns = row.Descendants("td");

                            dto.rank = Int32.Parse(columns.ElementAt(1).FirstChild.InnerText);
                            HtmlNode nameAndURL = columns.ElementAt(2).FirstChild;
                            dto.name = nameAndURL.InnerText;
                            dto.url = nameAndURL.GetAttributeValue("href","#");
                            dto.country = country;
                            ranking.Add(dto);
                            if (rowno > 24)
                                break;

                            rowno++;
                        }
                        listResult.Add(ranking);
                    }
                }
            }

            return listResult;
        }

        public void Insert(JArray content)
        {
            StreamWriter Writer = new StreamWriter(@"C:\universities.json");
            Writer.WriteLine(content.ToString());
            Writer.Close();

            Insert(File.Open(@"C:\universities.json", FileMode.Open), "http://localhost:5066/api/addrankings");
        } 

        public void CrawlAndAnalyzeSites()
        {
            JArray sites = this.Config.Sites();
            List<CrawlerEntryDTO> range = new List<CrawlerEntryDTO>();
            foreach (JObject site in sites)
            {
                Console.WriteLine("Analyzing "+site);
                string country = site.GetValue("country").ToString();
                range = (this.AnalyzeSite(site.GetValue("url").ToString(), country));
                WriteToFile(range);
                //Console.WriteLine("Output: "+"@CrawlerOut.json");
                Insert(File.Open(@"C:\CrawlerOut.json", FileMode.Open), "http://localhost:5066/api/addentries");
                this.AnalyzedLinks.Clear();
                this.Links.Clear();
            }
        }

        public static Boolean WriteToFile(List<CrawlerEntryDTO> ResultList)
        {
            if (ResultList == null)
            {
                return false;
            }
            try
            {        
                StreamWriter Writer = new StreamWriter(@"C:\CrawlerOut.json");
                Writer.WriteLine(JArray.FromObject(ResultList).ToString());           
                Writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
            return true;
        }


        public static void Insert(FileStream stream, string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 60 * 5 * 1000;
                request.Proxy = null;
                request.ServicePoint.ConnectionLeaseTimeout = 60 * 5 * 1000;
                request.ServicePoint.MaxIdleTime = 60 * 5 * 1000;
                StreamReader reader = new StreamReader(stream);
                string line = "";
                StreamWriter writer = new StreamWriter(request.GetRequestStream());
                while ((line = reader.ReadLine()) != null)
                {
                    writer.WriteLine(line);
                }
                reader.Close();
                writer.Close();
                WebResponse response = request.GetResponse();
                string serverResponse = "";
                line = "";
                reader = new StreamReader(response.GetResponseStream());
                while ((line = reader.ReadLine()) != null)
                {
                    serverResponse += line;
                }
                Console.WriteLine(serverResponse);
                reader.Close();
                response.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public List<CrawlerEntryDTO> AnalyzeSite(string rootUrl, string country)
        {
            
            DateTime then = DateTime.Now;
            List<CrawlerEntryDTO> result = new List<CrawlerEntryDTO>();
            CrawlerEntryDTO rootEntry = AnalyzeHtml(rootUrl);
            rootEntry.Country = country;
            rootEntry.EntryTimestamp = DateTime.Now;
            if (rootEntry == null) {
                return null;
            }
            result.Add(rootEntry);
            //this.Links.Enqueue(rootUrl);
            Console.WriteLine();
            while (this.Links.Count() > 0 && this.AnalyzedLinks.Count() < this.MaxAnalyzed)
            {
                CrawlerEntryDTO subLinkResult = AnalyzeHtml(this.Links.Dequeue());
                
                if (subLinkResult != null) {
                    subLinkResult.Country = country;
                    subLinkResult.EntryTimestamp = DateTime.Now;
                    result.Add(subLinkResult);                
                }
                Console.WriteLine();
                Console.WriteLine("\nProgress: ["+this.AnalyzedLinks.Count()+"/"+this.MaxAnalyzed+"]\n");
            }
            Console.WriteLine("Took: "+ DateTime.Now.Subtract(then).Seconds+" seconds");
            return result;
        }


        private string GetContentType(string URL)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.Method = "HEAD";
                request.Timeout = this.TimeOut * 1000;
                request.Proxy = null;
                request.ServicePoint.ConnectionLeaseTimeout = this.TimeOut * 1000;
                request.ServicePoint.MaxIdleTime = this.TimeOut * 1000;

                WebResponse response = request.GetResponse();
                string Type = response.ContentType;
                response.Close();
                return Type.ToLower();
            }
            catch (Exception)
            {
                return null;
            }          
        }


        private string GetContent(string URL)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.Method = "GET";
                request.Timeout = this.TimeOut * 1000;
                request.Proxy = null;
                request.ServicePoint.ConnectionLeaseTimeout = this.TimeOut * 1000;
                request.ServicePoint.MaxIdleTime = this.TimeOut * 1000;

                WebResponse response = request.GetResponse();
                string html = "";
                string line = "";
                StreamReader reader = new StreamReader(response.GetResponseStream());
                while ((line = reader.ReadLine()) != null)
                {
                    html += line;
                }

                reader.Close();
                response.Close();
                return html;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private HtmlDocument RetrieveHtmlDoc(string URL)
        {
            
            HtmlDocument doc = new HtmlDocument();
            try
            {
                string ContentType = GetContentType(URL);
                if (ContentType != null && ContentType.Contains("html"))
                {
                    doc.LoadHtml(GetContent(URL));
                }
                else
                {
                    return null;       
                }               
            }
            catch (Exception  e)
            {
                Console.WriteLine("     > WARN: ["+e.Message+"]");
                return null;
            }
            return doc;
        }

        private Boolean LinkFilter(string Link)
        {


            Link = Link.ToLower();

            if (String.IsNullOrWhiteSpace(Link))
                return false;



            var filters = (JArray) Config.Get("ignore_links_with");
            if (filters != null && filters.Count() > 0)
            {
                foreach (JObject filter in filters)
                {
                    string type = filter.GetValue("type").ToString();
                    var terms = (JArray)filter.GetValue("terms");
                    switch (type)
                    {
                        case "contains":
                            foreach (string term in terms)
                            {
                                if (Link.Contains(term))
                                    return false;                                
                            }
                            break;
                        case "ends_with":
                            foreach (string term in terms)
                            {
                                if (Link.EndsWith(term))
                                    return false;                                
                            }
                            break;
                        case "starts_with":
                            foreach (string term in terms)
                            {
                                if (Link.StartsWith(term))                                
                                    return false;                                
                            }
                            break;
                        default:
                            break;
                    }
                }
            }


            return true;            
                                 
        }

        private string RefineLink(string Url, string Link)
        {
            if (Link.Contains("#"))
            {
                Link = Link.Split('#')[0];
            }
            if (!Url.EndsWith("/"))
            {
                Url += "/";
            }
            if (!Link.ToLower().StartsWith("http"))
            {
                if (Link.StartsWith("/"))
                {
                    Link = Link.Substring(1);
                }
                Link = Url + Link;
            }
            if (!Link.EndsWith("/"))
            {
                Link += "/";
            }
            return Link;
        }


        public static string SaveEntries(JArray jEntries)
        {
            HttpWebRequest Request = WebRequest.Create("http://localhost:5066/api/addentries") as HttpWebRequest;
            Request.Method = "POST";
            Request.ContentType = "application/json; charset=UTF-8";
            Request.Accept = "application/json";
            Request.KeepAlive = false;
            Request.ProtocolVersion = HttpVersion.Version10;
            Stream RequestStream = Request.GetRequestStream();
            byte[] JsonBytes = Encoding.UTF8.GetBytes(jEntries.ToString());
            
            RequestStream.Write(JsonBytes, 0, JsonBytes.Length);
            RequestStream.Close();

            HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
            var ResponseJSON = (JObject)new JsonSerializer().Deserialize(new JsonTextReader(new StreamReader(Response.GetResponseStream())));
            Response.Close();
            return ResponseJSON.ToString();
        }


        public CrawlerEntryDTO AnalyzeHtml(string url) {
            CrawlerEntryDTO crawlerResponse = new CrawlerEntryDTO();
            //long SecondsTook = 0;
            //DateTime time = DateTime.Now;
            //Console.WriteLine("URL: ["+url+"] "+" DateTime: ["+time.ToString()+"]");
           

            try
            {
                if (this.AnalyzedLinks.Contains(url))
                {
                    //Console.WriteLine("     > Already analyzed!");
                    return null;
                }
                //Console.WriteLine("     > Retrieving HTML...");
                HtmlDocument doc = this.RetrieveHtmlDoc(url);                
                if (doc == null)
                {
                    Console.WriteLine("     > WARN: Could not retrieve html document.");
                    return null;
                }
                else
                {    
                                  
                    /*if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0)
                    {
                        Console.WriteLine(string.Format("     > WARN: HTML Has ({0}) errors.", doc.ParseErrors.Count()));
                    }*/
                    //Console.WriteLine("     > Analyzing..");
                    IEnumerable<HtmlNode> title = doc.DocumentNode.Descendants("title");
                    string titleText = "";
                    if (title.Count() > 0)
                    {
                        titleText = title.ElementAt(0).InnerText;
                       
                    }                
                    IEnumerable<HtmlNode> anchors = doc.DocumentNode.Descendants("a");
                    foreach (var node in anchors)
                    {
                        string href = node.GetAttributeValue("href", "");
                        if (LinkFilter(href))
                        {
                            href = RefineLink(url, href);
                            if (!this.AnalyzedLinks.Contains(href) && !this.Links.Contains(href))
                            {
                                this.Links.Enqueue(href);
                            }
                        }
                    }

                    crawlerResponse.URL = url;
                    crawlerResponse.Title = titleText;
                    string text = titleText + " ";
                    for (int i = 0; i < this.Elements.Length; i++)
                    {
                        var htmlNodes = doc.DocumentNode.Descendants(this.Elements[i]);
                        foreach (var node in htmlNodes)
                        {
                            text += (node.InnerText + " ");
                        }
                    }

                    crawlerResponse.TaggedEntries = processor.Process(text);
                    //Console.WriteLine("     > Analyzed successfully!");
                    this.AnalyzedLinks.Add(url); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("     > WARN: ["+e.Message+"]");
                return null;
            }
            //SecondsTook = DateTime.Now.Subtract(time).Seconds;
            //Console.WriteLine("     > Took "+SecondsTook+" s");
            return crawlerResponse;
        }
    }
}
