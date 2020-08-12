using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tMod64Bot.Modules.TagSystem;

namespace tMod64Bot.Modules.ConfigSystem
{
    public class Config
    {
        public char BotPrefix;
        
        public long BotManagerRole;

        public long BotOwnerId;

        public long LoggingChannelId;

        public long ModLoggingChannelId;

        public long AdminChannelId;

        public long AdminRoleId;

        public long MutedRole;

        public long SoftbanRole;

        public long SupportStaffRole;
    }
    
    public class ConfigService
    {
        private static string _fullPath = Path.GetFullPath($"{Environment.CurrentDirectory + @"\..\..\..\"}{ConfigConstants.ConfigFileName}");
        
        public static string GetJsonData()
        {
            using (StreamReader r = new StreamReader(_fullPath))
            {
                return r.ReadToEnd();
            }

            return null;
        }
        
        public static async Task WriteJsonData(string jsonData)
            => await File.WriteAllTextAsync(_fullPath, jsonData);

        public static string GetConfig(ConfigEnum config)
        {
            var deserializedObject = JsonConvert.DeserializeObject<Config>(GetJsonData());

            switch (config)
            {
                //break; isn't needed since return makes everything after that unreachable
                case ConfigEnum.BotPrefix:
                    return deserializedObject.BotPrefix.ToString();
                case ConfigEnum.BotManagerRole:
                    return deserializedObject.BotManagerRole.ToString();
                case ConfigEnum.BotOwner:
                    return deserializedObject.BotOwnerId.ToString();
                case ConfigEnum.LoggingChannel:
                    return deserializedObject.LoggingChannelId.ToString();
                case ConfigEnum.ModLogChannel:
                    return deserializedObject.ModLoggingChannelId.ToString();
                case ConfigEnum.AdminChannel:
                    return deserializedObject.AdminChannelId.ToString();
                case ConfigEnum.AdminRole:
                    return deserializedObject.AdminRoleId.ToString();
                case ConfigEnum.MutedRole:
                    return deserializedObject.MutedRole.ToString();
                case ConfigEnum.SoftbanRole:
                    return deserializedObject.SoftbanRole.ToString();
                case ConfigEnum.SupportStaffRole:
                    return deserializedObject.SupportStaffRole.ToString();
                default:
                    return null;
            }
        }

        public static async Task UpdateBotPrefix(string newPrefix)
        {
            JObject rss = JObject.Parse(GetJsonData());
            rss["BotPrefix"] = newPrefix;

            await WriteJsonData(rss.ToString());
        }
        
        public static async Task UpdateBotManager(long id)
        {
            JObject rss = JObject.Parse(GetJsonData());
            rss["BotManagerRole"] = id;

            await WriteJsonData(rss.ToString());
        }
        
        public static async Task UpdateLoggingChannel(long id)
        {
            JObject rss = JObject.Parse(GetJsonData());
            rss["LoggingChannel"] = id;

            await WriteJsonData(rss.ToString());
        }
        
        public static async Task UpdateModLoggingChannel(long id)
        {
            JObject rss = JObject.Parse(GetJsonData());
            rss["ModLoggingChannel"] = id;

            await WriteJsonData(rss.ToString());
        }
        
        public static async Task UpdateAdminChannel(long id)
        {
            JObject rss = JObject.Parse(GetJsonData());
            rss["AdminChannel"] = id;

            await WriteJsonData(rss.ToString());
        }
        
        public static async Task UpdateAdminRole(long id)
        {
            JObject rss = JObject.Parse(GetJsonData());
            rss["AdminRole"] = id;

            await WriteJsonData(rss.ToString());
        }
        
        public static async Task UpdateMutedRole(long id)
        {
            JObject rss = JObject.Parse(GetJsonData());
            rss["MutedRole"] = id;

            await WriteJsonData(rss.ToString());
        }

        public static async Task UpdateSoftbanRole(long id)
        {
            JObject rss = JObject.Parse(GetJsonData());
            rss["SoftbanRole"] = id;

            await WriteJsonData(rss.ToString());
        }
        
        public static async Task UpdateSupportStaffRole(long id)
        {
            JObject rss = JObject.Parse(GetJsonData());
            rss["SupportStaffRole"] = id;

            await WriteJsonData(rss.ToString());
        }
    }
}