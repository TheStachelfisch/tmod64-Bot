using System;
using System.Collections.Generic;

namespace tMod64Bot.Services.Config
{
    public record Config
    {
        public HashSet<Tag> Tags;
        
        public HashSet<MutedUser> MutedUsers;
        public HashSet<TempBannedUser> TempBannedUsers;
        
        public Dictionary<ulong, List<ulong>> StickiedUsers;
        public HashSet<Tuple<ulong, ulong, string>> ReactionRoleMessages;
        public HashSet<ulong> StickiedRoles;
        public HashSet<string> BannedWords;
        public HashSet<ulong> BannedWordsExemptChannel;
        public HashSet<string> ExemptInvites;

        public string BotPrefix;

        [Space]
        
        public ulong UserLoggingChannel;
        public ulong ModerationLoggingChannel;

        [Space]
        
        public ulong MutedRole;
        public ulong SoftbannedRole;
        public ulong BotManagerRole;

        [Space]
        
        public bool MessageOnBan;
        public bool MessageOnMute;
        public bool MessageOnKick;
        public bool MessageWithModerator;

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