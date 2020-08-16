using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using tMod64Bot.Modules.ConfigSystem;

namespace tMod64Bot.Handler
{
    public class LoggingHandler
    {
        private DiscordSocketClient _client;
        
        public LoggingHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageDeleted += OnMessageDelete;
        }

        private Task OnMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)) == 0)
                return Task.CompletedTask;

            EmbedBuilder embed = new EmbedBuilder();
            
            if (message.HasValue)
            {
                embed.WithAuthor(message.Value.Author);
                embed.WithTitle($"Message deleted in #{channel.Name}");
                embed.WithDescription(message.Value.Content);
                embed.WithColor(Color.Red);
                embed.WithFooter($"Id: {message.Value.Id}");
                embed.WithCurrentTimestamp();
            }
            else
            {
                embed.WithTitle($"Message deleted in #{channel.Name}");
                embed.WithDescription("Message Value could not be retrieved");
                embed.WithColor(Color.Red);
                embed.WithCurrentTimestamp();
            }


            _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)))
                .GetTextChannel(ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)))
                .SendMessageAsync("", false, embed.Build());
            return Task.CompletedTask;
        }
    }
}