using System;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Services.Logging.BotLogging
{
    public class BotLoggingService : ServiceBase
    {
        private readonly ConfigService _config;
        private readonly LoggingService _loggingService;
        private MemoryCache _cache;
        
        public BotLoggingService(IServiceProvider services) : base(services)
        {
            _loggingService = services.GetRequiredService<LoggingService>();
            _config = services.GetRequiredService<ConfigService>();
            _cache = services.GetRequiredService<MemoryCache>();
        }

        public async Task InitializeAsync()
        {
            Client.MessageDeleted += HandleDeletedMessage;
            // Client.MessageUpdated += HandleMessageUpdated;
            // Client.UserJoined += HandleUserJoined;
            // Client.UserLeft += HandleUserLeft;
            // Client.GuildMemberUpdated += HandleUserUpdated;

            //Handle with custom Moderation events
            //Client.UserUnbanned
            //Client.UserUnbanned
        }

        public IWebhook GetOrCreateWebhook(ulong channelId)
        {
            if (_cache.Contains(channelId.ToString()))
            {
                _loggingService.Log(LogSeverity.Verbose, LogSource.Service, "Got Webhook from cache");
                return (_cache.Get(channelId.ToString()) as IWebhook)!;
            }

            CacheItemPolicy policy = new()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
            };
            
            var channel = Client.GetChannel(channelId) as ITextChannel;
            var webhook = channel?.GetWebhooksAsync().Result
                .FirstOrDefault(x => x.Name.Equals("tMod64-Logging"));

            if (webhook == null)
                webhook = channel.CreateWebhookAsync("tMod64-Logging", WebRequest.Create("https://cdn.discordapp.com/icons/574595004064989214/254c07fcd10291f103d691a36c148a99.webp?size=1024").GetResponse().GetResponseStream()).Result;

            _cache.Add(new CacheItem(channelId.ToString(), webhook), policy);

            return webhook;
        }
        
        private Task HandleUserLeft(SocketGuildUser user)
        {
            throw new NotImplementedException();
        }

        private Task HandleUserUpdated(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            throw new NotImplementedException();
        }

        private Task HandleUserJoined(SocketGuildUser user)
        {
            throw new NotImplementedException();
        }

        private Task HandleMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage message, ISocketMessageChannel channel)
        {
            var guildChannel = channel as SocketGuildChannel;

            throw new NotImplementedException();
        }

        private async Task HandleDeletedMessage(Cacheable<IMessage, ulong> messageBefore, ISocketMessageChannel channel)
        {
            if (messageBefore.HasValue && (messageBefore.Value.Author.IsWebhook || messageBefore.Value.Embeds.Any()))
                return;
            
            using var client = new DiscordWebhookClient(GetOrCreateWebhook(_config.Config.UserLoggingChannel));
            {
                // Conditional Operator go brrrrrrr
                EmbedBuilder embed = new()
                {
                    Title = $"Message Deleted from #{channel.Name}",
                    Color = Color.Red,
                    Description = messageBefore.HasValue ? messageBefore.Value.Content : "Message could not be retrieved",
                    Footer = new EmbedFooterBuilder {Text = messageBefore.Id.ToString()},
                    Timestamp = messageBefore.HasValue ? messageBefore.Value.Timestamp : null
                };

                if (messageBefore.HasValue)
                    embed.WithAuthor(messageBefore.Value.Author);
                    
                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }
    }
}