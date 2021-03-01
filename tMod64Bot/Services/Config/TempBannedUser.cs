namespace tMod64Bot.Services.Config
{
    public record TempBannedUser
    {
        public ulong UserId;
        public ulong ServerId;
        public long ExpireTime;
        public string Reason;
    }
}