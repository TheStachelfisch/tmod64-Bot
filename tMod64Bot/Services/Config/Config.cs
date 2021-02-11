using System.Collections.Generic;
using Newtonsoft.Json;

namespace tMod64Bot.Services.Config
{
    public record Config
    {
        public Dictionary<ulong, ulong> MutedUsers;
        
        public Dictionary<ulong, List<ulong>> StickiedUsers;
        public HashSet<ulong> StickiedRoles;
        
        public HashSet<string> BannedWords;
        public HashSet<ulong> BannedWordsExemptChannel;

        public string BotPrefix;

        [Space]
        
        public ulong UserLoggingChannel;
        public ulong ModerationLoggingChannel;

        [Space]
        
        public ulong MutedRole;
        public ulong SoftbannedRole;
        public ulong BotManagerRole;

        [Space] 
        
        public bool LogUserLeft;
        public bool LogUserUpdated;
        public bool LogUserJoined;
        public bool LogMessageUpdated;
        public bool LogMessageDeleted;
        public bool ModLogUserBanned;
        public bool ModLogUserKicked;
        public bool ModLogUserMuted;
    }
}