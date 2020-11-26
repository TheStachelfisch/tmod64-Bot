using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace tMod64Bot.Services
{
    public sealed class InviteBlockerService : ServiceBase
    {
        private readonly ConfigService _config;

        public InviteBlockerService(IServiceProvider services) : base(services)
        {
            _config = services.GetRequiredService<ConfigService>();

            _client.MessageReceived += BlockInvites;
        }

        private async Task BlockInvites(SocketMessage msg)
        {
            var guild = _client.GetGuild(_config.GuildId);

            var user = msg.Author as SocketGuildUser;

            if (user.IsWebhook || user.IsBot || _config.IsExempt(user))
                return;


            if (MessageContainsInvite(msg.Content))
            {
                var builder = new EmbedBuilder()
                    .WithTitle("Your message was Deleted")
                    .WithDescription($"Your message was deleted because it may have contained a Invite\n\n{msg.Content}")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .WithFooter("This message was sent by a bot");

                await msg.DeleteAsync();

                await msg.Author.SendMessageAsync(embed: builder.Build());
            }
        }

        private static bool MessageContainsInvite(string message)
        {
            if (message.Contains("discord.gg/") || message.Contains("https://discord.gg/") || message.Contains("discord.com/invite/"))
                return true;

            return false;
        }
    }
}