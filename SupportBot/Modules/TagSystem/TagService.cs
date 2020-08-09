using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SupportBot.Modules.TagSystem
{
    public class Tag
    {
        [JsonProperty("name")] 
        public string Name;

        [JsonProperty("content")] 
        public string Content;

        [JsonProperty("owner")] 
        public string OwnerName;

        [JsonProperty("ownerId")]
        public ulong OwnerId;

        [JsonProperty("createdAt")]
        public long CreatedAt;

        [JsonProperty("exclusiveTag")]
        public bool ExclusiveTag;
    }
    
    public class TagService
    {
        private static Tag _tagData;
        
        public static string GetTagContentByName(string TagName)
        {
            string Json = File.ReadAllText(@"tags.json");

            var deserializedObject = JsonConvert.DeserializeObject<List<Tag>>(Json);

            //FirstOrDefault can be used here since that field will only exist once per tag
            return deserializedObject.Where(x => x.Name.Equals(TagName)).Select(i => i.Content).FirstOrDefault();
        }
    }
}