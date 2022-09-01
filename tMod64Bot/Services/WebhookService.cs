using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Logging;

namespace tMod64Bot.Services
{
    public class WebhookService : ServiceBase
    {
        private readonly LoggingService _loggingService;
        private readonly MemoryCache _cache;
        
        public WebhookService(IServiceProvider services) : base(services)
        {
            _loggingService = services.GetRequiredService<LoggingService>();
            _cache = services.GetRequiredService<MemoryCache>();
        }
        
        public IWebhook GetOrCreateWebhook(ulong channelId)
        {
            if (_cache.Contains(channelId.ToString()))
                return (_cache.Get(channelId.ToString()) as IWebhook)!;

            CacheItemPolicy policy = new()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
            };

            var channel = Client.GetChannel(channelId) as ITextChannel;
            var webhook = channel?.GetWebhooksAsync().Result
                .FirstOrDefault(x => x.Name.Equals("tMod64-Logging"));

            if (webhook == null)
            {
                webhook = channel.CreateWebhookAsync("tMod64-Logging", new HttpClient().GetStreamAsync("https://cdn.discordapp.com/attachments/630417470112399360/1012493237367345182/unknown.png").GetAwaiter().GetResult()).Result;
            }

            _cache.Add(new CacheItem(channelId.ToString(), webhook), policy);

            return webhook;
        }
    }
}