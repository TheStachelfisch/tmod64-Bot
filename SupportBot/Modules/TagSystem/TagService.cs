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
    }
    
    public class TagService
    {
        public static string GetJsonData()
        {
            using (StreamReader r = new StreamReader(TagConstants.FileName))
            {
                return r.ReadToEnd();
            }

            return null;
        }

        public static async Task WriteJsonData(string jsonData)
            => await File.WriteAllTextAsync(TagConstants.FileName, jsonData);

        public static string GetTagContentByName(string tagName)
        {
            string json = File.ReadAllText(TagConstants.FileName);

            var deserializedObject = JsonConvert.DeserializeObject<List<Tag>>(GetJsonData());

            //FirstOrDefault can be used here since that field will only exist once per tag
            return deserializedObject.Where(x => x.Name.Equals(tagName.ToLower())).Select(i => i.Content).FirstOrDefault();
        }

        public static async Task CreateTag(string tagName, string content, string owner, ulong ownerId)
        {
            List<Tag> tags = JsonConvert.DeserializeObject<List<Tag>>(GetJsonData());
            
            Tag tagContent = new Tag
            {
                Name = tagName.ToLower(), Content = content, OwnerName = owner, OwnerId = ownerId,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            
            tags.Add(tagContent);

            string jsonData = JsonConvert.SerializeObject(tags, Formatting.Indented);
            await WriteJsonData(jsonData);
        }
        
        public static async Task DeleteTagByName(string tagName)
        {
            List<Tag> tags = JsonConvert.DeserializeObject<List<Tag>>(GetJsonData());

            tags.RemoveAll(i => i.Name.Equals(tagName.ToLower()));

            string jsonData = JsonConvert.SerializeObject(tags, Formatting.Indented);
            await WriteJsonData(jsonData);
        }

        public static async Task EditTag(string tagName, string newContent)
        {
            List<Tag> tags = JsonConvert.DeserializeObject<List<Tag>>(GetJsonData());

            tags.Where(i => i.Name.Equals(tagName.ToLower())).Select(c => { c.Content = newContent; return c; }).ToList();
            
            string jsonData = JsonConvert.SerializeObject(tags, Formatting.Indented);
            await WriteJsonData(jsonData);
        }
        
        public static List<Tag> GetAllTags()
        {
            List<Tag> tags = JsonConvert.DeserializeObject<List<Tag>>(GetJsonData());
            return tags;
        }
        
        public static bool GetIfTagExists(string tagName)
        {
            var objects = JsonConvert.DeserializeObject<List<Tag>>(GetJsonData());

            return objects.Any(y => y.Name.Equals(tagName.ToLower()));
        }
    }
}