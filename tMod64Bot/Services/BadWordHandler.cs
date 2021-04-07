using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Commons;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Services
{
    public class BadWordHandler : ServiceBase, IInitializeable
    {
        private readonly ConfigService _configService;
        
        public BadWordHandler(IServiceProvider services) : base(services)
        {
            _configService = services.GetRequiredService<ConfigService>();
        }

        public async Task Initialize()
        {
            Client.MessageReceived += HandleBadWordMessage;
            Client.MessageUpdated += HandleBadWordMessageEdit;
        }

        private async Task HandleBadWordMessageEdit(Cacheable<IMessage, ulong> oldMessage, SocketMessage message, ISocketMessageChannel channel)
        {
            var user = message.Author as SocketGuildUser;
            
            if (message.Author.IsBot || message.Author.IsWebhook || (message.Author as SocketGuildUser)!.GuildPermissions.Administrator || String.Join(' ', user.Roles.Select(x => x.Name)).ToLower().ContainsAny("support staff", "moderator"))
                return;

            if (ContainsBadWord(message.Content.ToLower(), out string word))
            {
                await message.DeleteAsync();

                var embed = new EmbedBuilder
                {
                    Title = "Message Removed",
                    Description = $"Your message was removed from #{message.Channel.Name}, because it contained a banned word\n\n**Message**: {message.Content}\n**Banned Word**: {word}",
                    Color = Color.Orange
                };

                embed.WithCurrentTimestamp();
                
                await message.Author.SendMessageAsync(embed: embed.Build());
            }
        }

        private async Task HandleBadWordMessage(SocketMessage message)
        {
            var user = message.Author as SocketGuildUser;
            
            if (message.Author.IsBot || message.Author.IsWebhook || (message.Author as SocketGuildUser)!.GuildPermissions.Administrator || String.Join(' ', user.Roles.Select(x => x.Name)).ToLower().ContainsAny("support staff", "moderator"))
                return;

            if (ContainsBadWord(message.Content.ToLower(), out string word))
            {
                await message.DeleteAsync();

                var embed = new EmbedBuilder
                {
                    Title = "Message Removed",
                    Description = $"Your message was removed from #{message.Channel.Name}, because it contained a banned word\n\n**Message**: {message.Content}\n**Banned Word**: {word}",
                    Color = Color.Orange
                };

                embed.WithCurrentTimestamp();
                
                await message.Author.SendMessageAsync(embed: embed.Build());
            }
        }

        private bool ContainsBadWord(string message, out string badWord) => message.ContainsAny(out badWord, _configService.Config.BannedWords.ToArray());
    }
}