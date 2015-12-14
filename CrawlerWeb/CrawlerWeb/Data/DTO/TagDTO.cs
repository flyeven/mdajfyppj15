using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrawlerWeb.DTO
{
    public class TagDTO
    {
        public int TagID { get; set; }
        public string TagText { get; set; }
        public List<string> Terms { get; set; }


        public TagDTO()
        {
            this.Terms = new List<string>();
        }
    }
}