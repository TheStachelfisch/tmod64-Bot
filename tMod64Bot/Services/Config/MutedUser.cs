namespace tMod64Bot.Services.Config
{
    public record MutedUser
    {
        public ulong UserId;
        public ulong ServerId;
        public long ExpireTime;
    }
}