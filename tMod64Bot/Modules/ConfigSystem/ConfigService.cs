using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tMod64Bot.Modules.TagSystem;

namespace tMod64Bot.Modules.ConfigSystem
{
    public class Config
    {
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

        public static long GetConfig(ConfigEnum config)
        {
            var deserializedObject = JsonConvert.DeserializeObject<Config>(GetJsonData());

            switch (config)
            {
                //break; isn't needed since return makes everything after that unreachable
                case ConfigEnum.BotManagerRole:
                    return long.Parse(deserializedObject.BotManagerRole.ToString());
                case ConfigEnum.BotOwner:
                    return long.Parse(deserializedObject.BotOwnerId.ToString());
                case ConfigEnum.LoggingChannel:
                    return long.Parse(deserializedObject.LoggingChannelId.ToString());
                case ConfigEnum.ModLogChannel:
                    return long.Parse(deserializedObject.ModLoggingChannelId.ToString());
                case ConfigEnum.AdminChannel:
                    return long.Parse(deserializedObject.AdminChannelId.ToString());
                case ConfigEnum.AdminRole:
                    return long.Parse(deserializedObject.AdminRoleId.ToString());
                case ConfigEnum.MutedRole:
                    return long.Parse(deserializedObject.MutedRole.ToString());
                case ConfigEnum.SoftbanRole:
                    return long.Parse(deserializedObject.SoftbanRole.ToString());
                case ConfigEnum.SupportStaffRole:
                    return long.Parse(deserializedObject.SupportStaffRole.ToString());
                default:
                    return 0;

            }
        }
    }
}