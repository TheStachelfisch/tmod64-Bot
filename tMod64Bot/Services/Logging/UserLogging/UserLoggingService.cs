using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Services.Logging.UserLogging
{
    public class UserLoggingService : ServiceBase
    {
        private readonly ConfigService _config;
        private readonly LoggingService _loggingService;
        
        public UserLoggingService(IServiceProvider services) : base(services)
        {
            _loggingService = services.GetRequiredService<LoggingService>();
            _config = services.GetRequiredService<ConfigService>();
        }

        public async Task InitializeAsync()
        {
            // Client.MessageDeleted += HandleDeletedMessage;
            // Client.MessageUpdated += HandleMessageUpdated;
            // Client.UserJoined += HandleUserJoined;
            // Client.UserLeft += HandleUserLeft;
            // Client.GuildMemberUpdated += HandleUserUpdated;

            //Handle with custom Moderation events
            //Client.UserUnbanned
            //Client.UserUnbanned
        }

        private IWebhook GetOrCreateWebhook(ulong channelId)
        {
            var channel = Client.GetChannel(channelId) as ITextChannel;
            
            var webhook = channel?.GetWebhooksAsync().Result
                .FirstOrDefault(x => x.Name.Equals("tMod64-Logging"));

            if (webhook == null)
                webhook = channel?.CreateWebhookAsync("tMod64-Logging").Result;

            return webhook!;
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

        private Task HandleDeletedMessage(Cacheable<IMessage, ulong> messageBefore, ISocketMessageChannel channel)
        {
            throw new NotImplementedException();
        }
    }
}