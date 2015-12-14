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
                    response["result"] = JObject.FromObject(result);
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
                response["status"] = true;
                response["message"] = string.Format("{0} Entries added",0);
            }
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = e.Message;
            }

            return response;
        }

        [Route("api/retrievetags")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject RetrieveTags([FromUri(Name = "query", SuppressPrefixCheck = false)] string query = "")
        {
            var response = new JObject();

            try
            {
                List<TagDTO> result;
                if (string.IsNullOrWhiteSpace(query))
                {
                    result = SearchManager.RetrieveTags();
                }
                else
                {
                    result = SearchManager.RetrieveTags(query);
                }
                if (result == null)
                {
                    response["status"] = true;
                    response["message"] = "0 tag(s) fetched";
                    response["tags"] = null;
                } 
                else
                {
                    response["status"] = true;
                    response["message"] = result.Count()+" tag(s) fetched";
                    response["tags"] = JArray.FromObject(result);
                }
            }
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = e.Message;
                response["tags"] = null;
            }
            return response;
        }

    }
}
