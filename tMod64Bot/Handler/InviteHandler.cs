using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using tMod64Bot.Modules.ConfigSystem;

namespace tMod64Bot.Handler
{
    public class InviteHandler
    {
        private readonly DiscordSocketClient _client;

        public InviteHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += OnMessageReceive;
        }

        //TODO: Check if user has role that allows to send invite
        private async Task OnMessageReceive(SocketMessage arg)
        {
            var botManagerRole = _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString())).GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));
            var supportStaffRole = _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString())).GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.SupportStaffRole)?.ToString()));
            
            var user = arg.Author as SocketGuildUser;
            
            var dmEmbed = new EmbedBuilder();

            if (!arg.Author.IsWebhook && !arg.Author.IsBot)
            {
                //It somehow doesn't work the other way around
                if (user.Roles.Contains(botManagerRole) || user.Roles.Contains(supportStaffRole) || user.GuildPermissions.ManageMessages)
                {
                    return;
                }
                else if (MessageContainsInvite(arg.Content))
                {
                    dmEmbed.WithTitle("Your message was Deleted");
                    dmEmbed.WithDescription($"Your message was deleted because it may have contained a Invite\n\n{arg.Content}");
                    dmEmbed.WithColor(Color.Red);
                    dmEmbed.WithCurrentTimestamp();
                    dmEmbed.WithFooter("This message was sent by a bot");

                    await arg.DeleteAsync();

                    await arg.Author.SendMessageAsync("", false, dmEmbed.Build());
                }
            }
        }

        private static bool MessageContainsInvite(string message)
        {
            if (message.Contains("discord.gg/") || message.Contains("https://discord.gg/") || message.Contains("discord.com/invite/")) return true;

            return false;
        }
    }
}