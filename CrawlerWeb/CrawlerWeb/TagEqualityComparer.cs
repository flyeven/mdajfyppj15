using CrawlerWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrawlerWeb
{
    public class TagEqualityComparer : IEqualityComparer<TaggedEntry>
    {
        public bool Equals(TaggedEntry x, TaggedEntry y)
        {
            return x.TagID == y.TagID;
        }

        public int GetHashCode(TaggedEntry obj)
        {
            return base.GetHashCode();
        }
    }
}