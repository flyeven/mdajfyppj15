using CrawlerWeb;
using CrawlerWeb.Data;
using CrawlerWeb.DTO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerWeb
{
    class SearchManager
    {

        private static char[] Splitter = new char[] { ' ' };

        private SearchManager() { }

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
                    
                    //ctx.Configuration.LazyLoadingEnabled = false;
                    var result = (from taggedEntry in ctx.TaggedEntries
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
                            dto.Tag = searchEntry.Tag;                            

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
                    var result = (from tag in ctx.Tags
                                  from term in ctx.Terms
                                  where term.TagID == tag.TagID
                                  select new
                                  {
                                      tag = tag.TagText,
                                      TagID = tag.TagID,
                                      terms = tag.Terms
                                  });
                    if (result.Count() > 0)
                    {
                        foreach (var tag in result)
                        {
                            TagDTO dto = new TagDTO();
                            dto.TagID = tag.TagID;
                            dto.TagText = tag.tag;
                            foreach (var term in tag.terms)
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
