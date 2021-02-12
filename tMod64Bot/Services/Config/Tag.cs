namespace tMod64Bot.Services.Config
{
    public record Tag
    {
        public string Name;
        public string Content;
        
        public ulong Owner;
        public long CreatedAt;
        public int Uses;
    }
}