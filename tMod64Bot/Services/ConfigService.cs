using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace tMod64Bot.Services
{
    public sealed class ConfigService : ServiceBase
    {
        private static readonly string PATH = Utils.SourceFileName("serverConfig.json");

        public ConfigService(IServiceProvider services) : base(services)
        {
            JsonConvert.PopulateObject(File.ReadAllText(PATH), this);

            Guild = _client.GetGuild(GuildId);
            ManagerRole = Guild.GetRole(BotManagerRoleId);
            SupportStaffRole = Guild.GetRole(SupportStaffRoleId);
        }


        public bool IsExempt(SocketGuildUser user) => user.Roles.Contains(ManagerRole) || user.Roles.Contains(supportStaffRole) || user.GuildPermissions.ManageMessages;

        public void SaveToFile()
        {
            File.WriteAllText(PATH, JsonConvert.SerializeObject(this));
        }


        public ulong GuildId { get; set; }

        public ulong AdminChannel { get; set; }

        public ulong AdminRoleId { get; set; }

        public ulong BotManagerRoleId { get; set; }

        public ulong BotOwner { get; set; }

        public string BotPrefix { get; set; }

        public ulong LoggingChannel { get; set; }

        public ulong ModLoggingChannel { get; set; }

        public ulong MutedRoleId { get; set; }

        public ulong SoftbanRoleId { get; set; }

        public ulong SupportStaffRoleId { get; set; }

        public HashSet<ulong> BadWordChannelWhitelist { get; set; }
        
        [JsonIgnore]
        private SocketGuild guild;
        
        [JsonIgnore]
        public SocketGuild Guild
        {
            get => guild;
            set
            {
                guild = value;
                GuildId = value.Id;
            }
        }

        [JsonIgnore]
        private SocketRole managerRole;
        
        [JsonIgnore]
        public SocketRole ManagerRole
        {
            get => managerRole;
            set
            {
                managerRole = value;
                BotManagerRoleId = value.Id;
            }
        }

        [JsonIgnore]
        private SocketRole supportStaffRole;
        
        [JsonIgnore]
        public SocketRole SupportStaffRole
        {
            get => managerRole;
            set
            {
                supportStaffRole = value;
                SupportStaffRoleId = value.Id;
            }
        }
    }
}
