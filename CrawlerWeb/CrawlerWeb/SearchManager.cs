using CrawlerWeb.Data;
using CrawlerWeb.DTO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace CrawlerWeb
{
    class SearchManager
    {

        private static char[] Splitter = new char[] { ' ', '+', '-', ',', '.', ':', ';', '/', '\\' };

        private SearchManager() { }

        public static void AddUniversityRankings(List<Site> _sites)
        {
            using (var ctx = new CrawlerDataContext())
            {
                var urls = _sites.Select(s => s.url);
                var duplicates = from site in ctx.Sites where urls.Contains(site.url) select site;
                if(duplicates.Count() > 0)
                {
                    ctx.Sites.RemoveRange(duplicates.ToList());
                    ctx.SaveChanges();                  
                }

                ctx.Sites.AddRange(_sites);
                ctx.SaveChanges();                
            }
        }

        public static JObject CrawlerHistogram() {
            JObject o = new JObject();
            JArray cat = new JArray();
            JArray data = new JArray();
            using (var ctx = new CrawlerDataContext())
            {
                var result = from entry in ctx.SearchEntries
                             where entry.EntryTimestamp.HasValue
                             group entry by DbFunctions.TruncateTime(entry.EntryTimestamp) into entries
                             select new { Key = entries.Key, Count = entries.Count() };

                foreach (var stat in result)
                {
                    
                    cat.Add(Shorten(((DateTime) stat.Key).ToLongDateString()));
                    data.Add(stat.Count);
                    
                }
            }
            o["categories"] = cat;
            o["data"] = data;
            return o;
        }

        private static String Shorten(String LongDateString)
        {
            String[] DateStringParts = LongDateString.Split(',');
            String[] MonthAndDay = DateStringParts[1].Trim().Split(' ');
            return MonthAndDay[0].Substring(0, 3) + " " + MonthAndDay[1] + ", " + DateStringParts[2].Substring(3);
        }

        public static void AddTag(Tag tag) {
            try
            {
               
                using (var ctx = new CrawlerDataContext())
                {
                    ctx.Tags.Add(tag);
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {

                throw new CrawlerDataError(e.Message);
            }           
        }

        public static void DeleteSite(string url) {
            try
            {
                using (var ctx = new CrawlerDataContext())
                {
                    var r = ctx.Sites.Where(s => s.url.Equals(url));
                    if (r.Count() > 0)
                    {
                        Site s = r.First();
                        ctx.Sites.Remove(s);
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {

                throw new CrawlerDataError(e.Message);
            }
        }

        public static void AddSite(string site) {
            try
            {
                using (var ctx = new CrawlerDataContext())
                {
                    Site s = new Site();
                    s.url = site;
                    ctx.Sites.Add(s);

                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new CrawlerDataError(e.Message);
            }
        }


        public static List<string> GetCountries()
        {
            using (var ctx = new CrawlerDataContext())
            {
                var result = (from site in ctx.Sites select site.country).Distinct();
                return result.ToList();
            }
        }

        public static List<Site> GetSitesByCountry(string country)
        {
            using (var ctx = new CrawlerDataContext())
            {
                var result = (from site in ctx.Sites where country.ToLower().Equals(site.country.ToLower()) select site).OrderBy(s => s.rank);
                return result.ToList();
            }
        }

        public static List<string> GetSites()
        {
            try
            {
                using (var ctx = new CrawlerDataContext())
                {
                    return ctx.Sites.Select(s => s.url).ToList();
                }
            }
            catch (Exception e)
            {
                throw new CrawlerDataError(e.Message);
            }
        }

        public static List<SearchEntryDTO> Search(string Query, int from, int size)
        {
            if (size < 10)
            {
                size = 10;
            }
            List<SearchEntryDTO> resultList = new List<SearchEntryDTO>();
            if (String.IsNullOrWhiteSpace(Query))
            {
                return resultList;
            }

            string[] RefinedTerms = Query.Split(Splitter);

            try
            {
                using (var ctx = new CrawlerDataContext())
                {
                    
                    var result = ctx.TaggedEntries.Where(t => t.Tag.Terms.Any(term => RefinedTerms.Any(rft => term.TermText.Contains(rft)))).GroupBy(g=>g.EntryID).Select(s=> new { URL=s.FirstOrDefault().SearchEntry.URL, Title=s.FirstOrDefault().SearchEntry.Title, Score = s.Sum(st=>st.Score), ID=s.FirstOrDefault().EntryID}).OrderByDescending(a=>a.Score).Skip(from).Take(size);

                    //ctx.Configuration.LazyLoadingEnabled = false;
                   /* var result = (from taggedEntry in ctx.TaggedEntries
                                  where taggedEntry.Tag.Terms.Any(term =>
                                        RefinedTerms.Any(rft =>
                                            term.TermText.Contains(rft)
                                        )                                      
                                  )
                                  select new
                                  {
                                      URL = taggedEntry.SearchEntry.URL,
                                      Title = taggedEntry.SearchEntry.Title,
                                      Score = taggedEntry.Score,
                                      Tag = taggedEntry.Tag.TagText
                                  }).OrderByDescending(s => s.Score).Skip(from).Take(size);

                    */
                    //var result = (from se in ctx.SearchEntries select se);

                    if (result != null && result.Count() > 0)
                    {
                        foreach (var searchEntry in result)
                        {

                            SearchEntryDTO dto = new SearchEntryDTO();

                            //dto.Tag = taggedEntry.Tag.TagText;
                            dto.URL = searchEntry.URL;
                            dto.Title = searchEntry.Title;
                            dto.Score = (int)searchEntry.Score;
                            dto.Tag = searchEntry.ID+","+dto.Score;                            
                            
                            resultList.Add(dto);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new CrawlerDataError(e.Message);
            }

            return resultList;


        }


        public static List<TagDTO> RetrieveTags()
        {
            List<TagDTO> resultList = new List<TagDTO>();
            try
            {
                using (var ctx = new CrawlerDataContext())
                {
                    var result = ctx.Tags.ToList();
                    if (result.Count() > 0)
                    {
                        foreach (var tag in result)
                        {
                            TagDTO dto = new TagDTO();
                            dto.TagID = tag.TagID;
                            dto.TagText = tag.TagText;
                            foreach (var term in tag.Terms)
                            {
                                dto.Terms.Add(term.TermText);
                            }

                            resultList.Add(dto);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                throw new CrawlerDataError(e.Message);
            }

            return resultList;
        }


        public static List<TagDTO> RetrieveTags(string Query)
        {
            List<TagDTO> Suggestions = new List<TagDTO>();

            if (string.IsNullOrWhiteSpace(Query))
                return null;

            if (Query.Length < 3)
                return null;

            string[] terms = Query.Split(Splitter);

            using (var ctx = new CrawlerDataContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                var result = (from tag in ctx.Tags
                              from term in ctx.Terms
                              where tag.TagID == term.TagID
                              where terms.Any(rft => term.TermText.Contains(rft))
                              select new {
                                  tag = tag.TagText,
                                  TagID = tag.TagID,
                                  terms = tag.Terms
                              });
                if (result.Count() > 0)
                {
                    foreach (var tag in result)
                    {
                        
                        TagDTO _tag = new TagDTO();
                        _tag.TagText = tag.tag;
                        _tag.TagID = tag.TagID;
                        foreach (var term in tag.terms)
                        {
                            _tag.Terms.Add(term.TermText);
                        }
                        Suggestions.Add(_tag);
                    }
                }
            }

            return Suggestions;
        }

        public static int Views()
        {
            try
            {
                using (var ctx = new CrawlerDataContext())
                {
                    AppSite App = null;
                    var result = (from app in ctx.AppSites where app.SiteID == 0 select app);
                    if (result.Count() > 0)
                    {
                        App = result.First();
                        return (int)App.Views;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static void AddEntries(List<SearchEntry> Entries)
        {
            try
            {


                var Comparer = new TagEqualityComparer();
                var URLs = Entries.Select(e => e.URL);
                using (var ctx = new CrawlerDataContext())
                {
                    var entriesInDB = (from entryDB in ctx.SearchEntries
                                       where URLs.Contains(entryDB.URL)
                                       select entryDB);

                    if (entriesInDB.Count() > 0)
                    {
                        foreach (var entryInDB in entriesInDB)
                        {
                            SearchEntry duplicate = Entries.Single(e => e.URL.Equals(entryInDB.URL));
                            Entries.Remove(duplicate);
                            entryInDB.TaggedEntries = entryInDB.TaggedEntries.Union(duplicate.TaggedEntries,Comparer).ToList();
                        }
                    }
                    ctx.SearchEntries.AddRange(Entries);

                    ctx.SaveChanges();                                          
                }
            }
            catch (Exception e)
            {
                throw new CrawlerDataError("Could not add Entries: "+e.Message);
            }
        }

        public static int SearchEntryCount() {
            try
            {
                using (var ctx = new CrawlerDataContext())
                {
                    return ctx.SearchEntries.Count();
                }
            }
            catch (Exception e)
            {
                throw new CrawlerDataError(e.Message);
            }
        }

        public static int UserCount() {
            try
            {
                using (var ctx = new CrawlerDataContext())
                {
                    return ctx.SiteUsers.Count();
                }
            }
            catch (Exception e)
            {

                throw new CrawlerDataError(e.Message);
            }
        }
        public static void AddViewCount()
        {
            try
            {
                using (var ctx = new CrawlerDataContext())
                {
                    AppSite App = null;
                    var result = (from app in ctx.AppSites where app.SiteID == 0 select app);
                    if (result.Count() > 0)
                    {
                        App = result.First();
                        App.Views = App.Views + 1;
                    }
                    else
                    {
                        App = new AppSite();
                        App.SiteID = 0;
                        App.Views = 1;
                        ctx.AppSites.Add(App);
                    }
                    ctx.SaveChanges();
                }
            }
            catch (Exception)
            {

                throw new CrawlerWeb.CrawlerDataError("Could not update views");
            }
        }
    }
}
