using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerWeb.DTO
{
    class SearchEntryDTO
    {
        public string Title { get; set; }
        public string URL { get; set; }
        public int Score { get; set; }
        public string Tags { get; set; }
    }
}
