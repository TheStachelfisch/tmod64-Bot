using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace tMod64Bot.Services
{
    public sealed class CensorshipService : ServiceBase
    {
        private static readonly string BAD_WORDS_PATH = Utils.SourceFileName("badWords.json");
        
        private readonly ConfigService _config;

        public HashSet<string> Words { get; private set; }

        public CensorshipService(IServiceProvider services) : base(services)
        {
            _config = services.GetRequiredService<ConfigService>();

            Words = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(BAD_WORDS_PATH));
        }
        
        public void Reload() => Words = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(BAD_WORDS_PATH));

        public async Task InitializeAsync() => _client.MessageReceived += CensorMessages;
        
        private async Task CensorMessages(SocketMessage msg)
        {
            if (_config.BadWordChannelWhitelist.Contains(msg.Channel.Id))
                return;

            var user = msg.Author as SocketGuildUser;

            if (user == null)
                return;
            
            if (user.IsWebhook || user.IsBot || _config.IsExempt(user))
                return;

            if (!msg.Embeds.Any() && StringContainsWord(msg.Content))
            {
                try { await msg.DeleteAsync(); }
                // TODO: This error is thrown when a message contains multiple banned words
                catch (HttpException) { }
            }
        }

        public bool StringContainsWord(string text)
        {
            text = text.ToLower();

            foreach (var words in Words)
                if (text.Contains(words.ToLower()))
                    return true;

            return false;
        }
    }
}
