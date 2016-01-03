using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrawlerWeb.Data.DTO
{
    public class SiteDTO
    {
        public string url { get; set; }
        public string country { get; set; }

        public SiteDTO()
        {

        }

        public SiteDTO(string c, string u)
        {
            this.country = c;
            this.url = u;
        }
    }
}