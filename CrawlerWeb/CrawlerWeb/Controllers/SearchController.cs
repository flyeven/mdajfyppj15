using System;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using CrawlerWeb;
using System.Web.Http.Cors;
using CrawlerWeb.DTO;
using System.Collections.Generic;
using CrawlerWeb.Data;

namespace CrawlerWeb.Controllers
{


    

    public class SearchController : ApiController
    {




        [Route("api/addtag")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject AddTag(JObject jTag)
        {
            var response = new JObject();
            try
            {
                Tag _tag = jTag.ToObject<Tag>();
                SearchManager.AddTag(_tag);
                response["status"] = true;
            }
            catch (Exception e)
            {
                response["message"] = e.Message;
                response["status"] = false;
            }
            return response;
        }



        [Route("api/addsite")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject AddSite([FromUri(Name = "url", SuppressPrefixCheck = false)] string url = "")
        {
            var response = new JObject();
            try
            {
                response["status"] = false;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    SearchManager.AddSite(url);
                    response["status"] = true;
                }
            }
            catch (Exception e)
            {
                response["message"] = e.Message;
                response["status"] = false;
            }

            return response;
        }




        [Route("api/deletesite")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject DeleteSiteByURL([FromUri(Name = "url", SuppressPrefixCheck = false)] string url = "")
        {
            var response = new JObject();
            try
            {
                response["status"] = false;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    SearchManager.DeleteSite(url);
                    response["status"] = true;
                }
            }
            catch (Exception e)
            {
                response["message"] = e.Message;
                response["status"] = false;
            }

            return response;
        }

        [Route("api/search")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject search([FromUri(Name = "query", SuppressPrefixCheck = false)] string query = "",
            [FromUri(Name = "from", SuppressPrefixCheck = false)] int from = 0,
            [FromUri(Name = "size", SuppressPrefixCheck = false)] int size = 10)
        {
            var response = new JObject();
            
            try
            {
                var result = SearchManager.Search(query, from, size);

                if (result == null || result.Count() == 0)
                {
                    response["status"] = true;
                    response["message"] = "No results found against terms \"" + query + "\", try web search.";
                    response["result"] = null;
                }
                else
                {
                    response["status"] = true;
                    response["message"] = result.Count() + " record(s) found against terms \"" + query+"\"";
                    response["result"] = JArray.FromObject(result);
                }
            }
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = "An error occured while searching the database ["+e.Message+"]";
                response["result"] = null;
            }
            return response;
        }

        [Route("api/addview")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject AddView()
        {
            var response = new JObject();

            try
            {
                response["status"] = true;
                response["message"] = "View count incremented";
                SearchManager.AddViewCount();
            }
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = e.Message;
            }

            return response;
        }


        [Route("api/getviews")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject GetViews()
        {
            var response = new JObject();

            try
            {
                response["status"] = true;
                response["message"] = "View count fetched";
                int views = SearchManager.Views();
                response["views"] = views;
            }
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = e.Message;
                response["views"] = -1;
            }

            return response;
        }


        [Route("api/addentries")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject AddEntries(JArray jEntries)
        {
            var response = new JObject();

            try
            {
                List<SearchEntry> Entries = null;
                try
                {
                     Entries = jEntries.ToObject<List<SearchEntry>>();
                }
                catch (Exception e)
                {
                    response["status"] = false;
                    response["message"] = "Invalid json body: "+e.Message;
                    return response;
                }

                SearchManager.AddEntries(Entries);
                int total = Entries.Count();
                response["status"] = true;
                response["message"] = string.Format("{0} new Entries added, {1} old Entries updated",Entries.Count(), total - Entries.Count());
            }
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = e.Message;
            }

            return response;
        }

        [Route("api/crawlerconfig")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject GetCrawlerConfig()
        {
            var response = new JObject();

            try
            {
                List<TagDTO> tags = SearchManager.RetrieveTags();
                List<string> sites = SearchManager.GetSites();
                response["status"] = true;
                response["tags"] = JArray.FromObject(tags);
                response["sites"] = JArray.FromObject(sites);              
            }
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = e.Message;
                response["tags"] = null;
            }
            return response;
        }


        [Route("api/usercount")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject UserCount()
        {
            var response = new JObject();

            try
            {
                response["status"] = false;                
                response["count"] = SearchManager.UserCount();
            }
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = e.Message;
                response["count"] = null;
            }
            return response;
        }

        [Route("api/stats")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject Stats()
        {
            var response = new JObject();
            try
            {
                response["status"] = true;
                JObject stats = new JObject();
                stats["usercount"] = SearchManager.UserCount();
                stats["views"] = SearchManager.Views();
                stats["entrycount"] = SearchManager.SearchEntryCount();
                response["stats"] = stats;
            }
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = e.Message;
                response["stats"] = null;
            }
            return response;
        }
    }
}
