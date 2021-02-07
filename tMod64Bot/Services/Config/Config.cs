using System.Collections.Generic;
using Newtonsoft.Json;

namespace tMod64Bot.Services.Config
{
    public record Config
    {
        /// <summary>
        /// Dictionary of Muted Users. Key value indicates the UserID of the muted Person. Value is the Mute duration.
        /// </summary>
        public Dictionary<ulong, ulong> MutedUsers;
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