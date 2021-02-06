using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Modules.Commons;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging.BotLogging;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    [Group("debug")]
    [RequireOwner]
    public class DebugModule : CommandBase
    {
        [Command("wh")]
        public async Task WebhookDebugging()
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            var debug = Services.GetRequiredService<BotLoggingService>();

            using (var client = new DiscordWebhookClient(debug.GetOrCreateWebhook(ConfigService.Config.UserLoggingChannel)))
            {
                await client.SendMessageAsync("Hello World!");
            }

            sw.Stop();
            
            await ReplyAsync($"Took {sw.ElapsedMilliseconds}ms to Execute");
        }

        [Command("purge")]
        public async Task PurgeDebug(ulong channelId, ulong amount)
        {
            var channel = Context.Guild.GetChannel(channelId) as ITextChannel;
            
            var messages = await channel.GetMessagesAsync(Convert.ToInt32(amount)).FlattenAsync();
            var filteredMessages = messages.Where(x => x.Timestamp < DateTimeOffset.Now.AddDays(14)).ToList();

            if (!filteredMessages.Any())
                await ReplyAsync("Nothing to delete");
            else
            {
                await channel.DeleteMessagesAsync(filteredMessages);
                await ReplyAsync($"Deleted {filteredMessages.Count()}{(filteredMessages.Count() == 1 ? "message" : "messages")}.");
            }
        }
    }
}