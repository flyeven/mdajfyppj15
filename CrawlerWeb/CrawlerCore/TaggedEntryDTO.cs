using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerCore
{
    class TaggedEntryDTO
    {
        private int v;

        public TaggedEntryDTO(int v)
        {
            this.v = v;
        }

        public int TagID { get; set; }
        public int Score { get; set; }
    }
}
