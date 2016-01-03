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
using System.Data.Entity.Validation;
using CrawlerWeb.Data.DTO;

namespace CrawlerWeb.Controllers
{


    

    public class SearchController : ApiController
    {


        [Route("api/crawlerhisto")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject GetCrawlerHistogram()
        {
            var response = new JObject();
            try
            {
                response["data"] = SearchManager.CrawlerHistogram();
                response["status"] = true;
            }
            catch (Exception e)
            {
                response["message"] = e.Message;
                response["status"] = false;
            }
            return response;
        }


        [Route("api/countries")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject Countries()
        {
            var response = new JObject();
            try
            {
                response["data"] = JArray.FromObject(SearchManager.GetCountries());
                response["status"] = true;
            }
            catch (Exception e)
            {
                response["message"] = e.Message;
                response["status"] = false;
            }
            return response;
        }

        [Route("api/addrankings")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject AddRankings(JArray jSites)
        {
            var response = new JObject();
            try
            {
                List<Site> _sites = jSites.ToObject<List<Site>>();
                SearchManager.AddUniversityRankings(_sites);
                response["status"] = true;
            }
            catch (DbEntityValidationException e)
            {
                JArray r = new JArray();

                foreach (var eve in e.EntityValidationErrors)
                {
                    JObject o = new JObject();

                    o["error"] = string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    JArray inner = new JArray();
                    foreach (var ve in eve.ValidationErrors)
                    {
                        inner.Add(string.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                    o["inner"] = inner;
                    r.Add(o);
                }
                response["errors"] = r;
                response["status"] = false;
                response["message"] = e.Message;
            }
            catch (Exception e)
            {
                response["message"] = e.Message;
                response["status"] = false;
            }
            return response;
        }

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



        [Route("api/getsitesbycountry")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject GetSitesByCountry([FromUri(Name = "country", SuppressPrefixCheck = false)] string country = "pakistan")
        {
            var response = new JObject();
            try
            {
                response["status"] = false;
                if (!string.IsNullOrWhiteSpace(country))
                {
                    response["data"] = JArray.FromObject(SearchManager.GetSitesByCountry(country));
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
                List<SiteDTO> sites = SearchManager.GetSites();
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
