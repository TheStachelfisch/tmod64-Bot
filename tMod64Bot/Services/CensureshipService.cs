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
    public sealed class CensureshipService : ServiceBase
    {
        private static readonly string BAD_WORDS_PATH = Utils.SourceFileName("badWords.json");


        private readonly ConfigService _config;

        public HashSet<string> Words { get; }

        public CensureshipService(IServiceProvider services) : base(services)
        {
            _config = services.GetRequiredService<ConfigService>();

            _client.MessageReceived += CensureMessages;

            Words = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(BAD_WORDS_PATH));
        }


        private async Task CensureMessages(SocketMessage msg)
        {
            if (_config.BadWordChannelWhitelist.Contains(msg.Channel.Id))
                return;

            var user = msg.Author as SocketGuildUser;

            if (user.IsWebhook || user.IsBot || _config.IsExempt(user))
                return;

            if (msg.Embeds.Count() == 0 && StringContainsWord(msg.Content))
            {
                try
                {
                    await msg.DeleteAsync();
                }
                // TODO: This error is thrown when a message contains multiple banned words
                catch (HttpException)
                {
                }
            }
        }

        public bool StringContainsWord(string text)
        {
            text = text.ToLower();

            foreach (var words in Words)
            {
                if (text.Contains(words.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
