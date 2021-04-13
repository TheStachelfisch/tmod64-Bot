using System.Linq;
using Discord;
using Discord.WebSocket;

namespace tMod64Bot
{
    public class MemberActivity : IActivity
    {
        private readonly BaseSocketClient _client;

        public MemberActivity(BaseSocketClient client) => _client = client;

        public string Name => $"{_client.Guilds.Sum(x => x.MemberCount)} Members";

        public ActivityType Type => ActivityType.Watching;
        public ActivityProperties Flags => ActivityProperties.None;
        public string Details => "";
    }
}