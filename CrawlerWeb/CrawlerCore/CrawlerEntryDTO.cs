using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerCore
{
    class CrawlerEntryDTO
    {
        public string Title { get; set; }
        public string URL { get; set; }
        public List<TaggedEntryDTO> TaggedEntries { get; set; }
        public string Country { get; set; }
        public DateTime EntryTimestamp { get; set; }
    }
}
