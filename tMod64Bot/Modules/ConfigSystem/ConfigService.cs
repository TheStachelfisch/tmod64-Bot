using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace tMod64Bot.Modules.ConfigSystem
{
    public class Config
    {
        public long GuildId;
        
        public long AdminChannel;

        public long AdminRole;

        public long BotManagerRole;

        public long BotOwner;
        
        public string BotPrefix;

        public long LoggingChannel;

        public long ModLoggingChannel;

        public long MutedRole;

        public long SoftbanRole;

        public long SupportStaffRole;
    }

    public class ConfigService
    {
        //TODO: Remove this cancer as soon as we can
        private static readonly string _fullPath = Path.GetFullPath($"{Environment.CurrentDirectory + @"\..\..\..\"}{ConfigConstants.ConfigFileName}");

        public static string GetJsonData()
        {
            using (var r = new StreamReader(_fullPath))
            {
                return r.ReadToEnd();
            }

            return null;
        }

        public static async Task WriteJsonData(string jsonData)
        {
            await File.WriteAllTextAsync(_fullPath, jsonData);
        }

        public static string GetConfig(ConfigEnum config)
        {
            var deserializedObject = JsonConvert.DeserializeObject<Config>(GetJsonData());

            switch (config)
            {
                //break; isn't needed since return makes everything after that unreachable
                case ConfigEnum.BotPrefix:
                    return deserializedObject.BotPrefix;
                case ConfigEnum.GuildId:
                    return deserializedObject.GuildId.ToString();
                case ConfigEnum.BotManagerRole:
                    return deserializedObject.BotManagerRole.ToString();
                case ConfigEnum.BotOwner:
                    return deserializedObject.BotOwner.ToString();
                case ConfigEnum.LoggingChannel:
                    return deserializedObject.LoggingChannel.ToString();
                case ConfigEnum.ModLogChannel:
                    return deserializedObject.ModLoggingChannel.ToString();
                case ConfigEnum.AdminChannel:
                    return deserializedObject.AdminChannel.ToString();
                case ConfigEnum.AdminRole:
                    return deserializedObject.AdminRole.ToString();
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
            var rss = JObject.Parse(GetJsonData());
            rss["BotPrefix"] = newPrefix;

            await WriteJsonData(rss.ToString());
        }

        public static async Task UpdateBotManager(long id)
        {
            var rss = JObject.Parse(GetJsonData());
            rss["BotManagerRole"] = id;

            await WriteJsonData(rss.ToString());
        }

        public static async Task UpdateLoggingChannel(long id)
        {
            var rss = JObject.Parse(GetJsonData());
            rss["LoggingChannel"] = id;

            await WriteJsonData(rss.ToString());
        }

        public static async Task UpdateModLoggingChannel(long id)
        {
            var rss = JObject.Parse(GetJsonData());
            rss["ModLoggingChannel"] = id;

            await WriteJsonData(rss.ToString());
        }

        public static async Task UpdateAdminChannel(long id)
        {
            var rss = JObject.Parse(GetJsonData());
            rss["AdminChannel"] = id;

            await WriteJsonData(rss.ToString());
        }

        public static async Task UpdateAdminRole(long id)
        {
            var rss = JObject.Parse(GetJsonData());
            rss["AdminRole"] = id;

            await WriteJsonData(rss.ToString());
        }

        public static async Task UpdateMutedRole(long id)
        {
            var rss = JObject.Parse(GetJsonData());
            rss["MutedRole"] = id;

            await WriteJsonData(rss.ToString());
        }

        public static async Task UpdateSoftbanRole(long id)
        {
            var rss = JObject.Parse(GetJsonData());
            rss["SoftbanRole"] = id;

            await WriteJsonData(rss.ToString());
        }

        public static async Task UpdateSupportStaffRole(long id)
        {
            var rss = JObject.Parse(GetJsonData());
            rss["SupportStaffRole"] = id;

            await WriteJsonData(rss.ToString());
        }

        public static async Task SetGuildId(long id)
        {
            var rss = JObject.Parse(GetJsonData());
            rss["GuildId"] = id;

            await WriteJsonData(rss.ToString());
        }
    }
}