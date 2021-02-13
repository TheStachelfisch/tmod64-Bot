using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging;

namespace tMod64Bot.Services
{
    public class ReactionRolesService : ServiceBase
    {
        private readonly ConfigService _configService;
        private readonly LoggingService _loggingService;
        
        public ReactionRolesService(IServiceProvider services) : base(services)
        {
            _configService = services.GetRequiredService<ConfigService>();
            _loggingService = services.GetRequiredService<LoggingService>();
        }

        public async Task InitializeAsync()
        {
            Client.ReactionAdded += HandleReactionAdded;
            Client.ReactionRemoved += HandleReactionRemoved;
        }

        private async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot || !_configService.Config.ReactionRoleMessages.Select(x => x.Item1).Any(y => y.Equals(reaction.MessageId)))
                return;
            
            var current = _configService.Config.ReactionRoleMessages.First(x => x.Item1.Equals(reaction.MessageId));

            if (Equals(reaction.Emote, new Emoji(current!.Item3)))
            {
                var guildUser = await channel.GetUserAsync(reaction.UserId) as SocketGuildUser;

                if (!guildUser.Roles.Any(x => x.Id.Equals(current.Item2)))
                    return;

                await guildUser!.RemoveRoleAsync(guildUser.Guild.GetRole(current.Item2));
            }
        }

        private async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot || !_configService.Config.ReactionRoleMessages.Select(x => x.Item1).Any(y => y.Equals(reaction.MessageId)))
                return;

            var current = _configService.Config.ReactionRoleMessages.First(x => x.Item1.Equals(reaction.MessageId));

            if (Equals(reaction.Emote, new Emoji(current!.Item3)))
            {
                var guildUser = await channel.GetUserAsync(reaction.UserId) as SocketGuildUser;

                if (guildUser.Roles.Any(x => x.Id.Equals(current.Item2)))
                    return;

                await guildUser!.AddRoleAsync(guildUser.Guild.GetRole(current.Item2));
            }
        }
    }
}