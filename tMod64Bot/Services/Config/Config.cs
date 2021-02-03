using System.Collections.Generic;

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
        
        public ulong LoggingChannel;
        public ulong ModerationLoggingChannel;
        
        public ulong MutedRole;
        public ulong SoftbannedRole;
        public ulong BotManagerRole;
    }
}