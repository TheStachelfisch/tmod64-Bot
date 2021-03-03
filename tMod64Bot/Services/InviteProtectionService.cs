using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Services
{
    public class InviteProtectionService : ServiceBase
    {
        private ConfigService _configService;
        
        public delegate void InviteDeletedEventHandler(SocketGuildUser user, SocketMessage message, Match match);
        public event InviteDeletedEventHandler InviteDeleted;
        
        public InviteProtectionService(IServiceProvider services) : base(services)
        {
            _configService = services.GetRequiredService<ConfigService>();
        }

        public async Task InitializeAsync()
        {
            Client.MessageReceived += HandleMessage;
            Client.MessageUpdated += HandleEdit;
        }

        private async Task HandleEdit(Cacheable<IMessage, ulong> oldMessage, SocketMessage message, ISocketMessageChannel channel)
        {
            var user = message.Author as SocketGuildUser;
            
            try
            {
                if (user.IsWebhook || user.IsBot || user.GuildPermissions.Administrator || String.Join(' ', user.Roles.Select(x => x.Name)).ToLower().ContainsAny("support staff", "moderator"))
                    return;
            }
            catch (Exception e) { /* Ignore - Happens when user has no roles */ }
            
            var match = ContainsInvite(message.Content);
            
            if (match.Success)
            {
                await message.DeleteAsync();
                InviteDeleted?.Invoke(user, message, match);
                
                var embed = new EmbedBuilder
                {
                    Title = "Message Removed",
                    Description = $"Your message was removed from #{message.Channel.Name} because it may have contained a invite\n\n**Message**: {message.Content}\n**Match**: {match.Value}",
                    Color = Color.Orange
                };

                embed.WithCurrentTimestamp();
                
                await user.SendMessageAsync(embed: embed.Build());
            }
        }

        private async Task HandleMessage(SocketMessage message)
        {
            var user = message.Author as SocketGuildUser;

            try
            {
                if (user.IsWebhook || user.IsBot || user.GuildPermissions.Administrator || String.Join(' ', user.Roles.Select(x => x.Name)).ToLower().ContainsAny("support staff", "moderator"))
                    return;
            }
            catch (Exception e) { /* Ignore - Happens when user has no roles */ }

            var match = ContainsInvite(message.Content);
            
            if (match.Success)
            {
                await message.DeleteAsync();
                InviteDeleted?.Invoke(user, message, match);

                var embed = new EmbedBuilder
                {
                    Title = "Message Removed",
                    Description = $"Your message was removed from #{message.Channel.Name} because it may have contained a invite\n\n**Message**: {message.Content}\n**Match**: {match.Value}",
                    Color = Color.Orange
                };

                embed.WithCurrentTimestamp();

                await user.SendMessageAsync(embed: embed.Build());
            }
        }

        public Match ContainsInvite(string message) => Regex.Match(message, @"(?<![\w\d])(discord\.gg\/\w{1,20}|discord\.com\/invite\/\w{1,20})(?![\w\d])");
    }
}