using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CrawlerCore
{
    class TextProcessor
    {

        private CrawlerConfiguration Config;
        private string[] Splitters;
        public TextProcessor()
        {
            Config = CrawlerConfiguration.GetInstance();
            this.Splitters = this.Config.Splitters().Select(sp => (string)sp).ToArray();
        }

        private int Process(string[] terms, string data) {
            int score = 0;
            for (int i = 0; i < terms.Length; i++)
            {
                score += Occurences(data, terms[i]);
            }
            return score;
        }

        public static int Occurences(string str, string val)
        {
            int occurrences = 0;
            int startingIndex = 0;

            while ((startingIndex = str.IndexOf(val, startingIndex)) >= 0)
            {
                ++occurrences;
                ++startingIndex;
            }

            return occurrences;
        }


        public List<TaggedEntryDTO> Process(string docText)
        {            
            string refinedDocText = " "+String.Join(" ",docText.ToLower().Split(this.Splitters, StringSplitOptions.RemoveEmptyEntries))+" ";
            var rules = this.Config.Rules();
            Dictionary<Int32, TaggedEntryDTO> taggedEntries = new Dictionary<Int32,TaggedEntryDTO>();
            int TagID;
            int Score = 0;
            float max = 0;

            foreach (JObject rule in rules)
            {                
                TagID = Int32.Parse(rule.GetValue("TagID").ToString());
                Score = Process(((JArray)rule.GetValue("Terms")).Select(term => (string)term).ToArray(), refinedDocText);
                if (Score > 0)
                {
                    TaggedEntryDTO dto = taggedEntries.ContainsKey(TagID) ? taggedEntries[TagID] : new TaggedEntryDTO(0);
                    dto.TagID = TagID;
                    dto.Score += Score;
                    taggedEntries[TagID] = dto;
                    if (Score > max)
                    {
                        max = Score;                            
                    } 
                }    
            }
            
            return taggedEntries.Values.ToList();                        
        }
    }
}
