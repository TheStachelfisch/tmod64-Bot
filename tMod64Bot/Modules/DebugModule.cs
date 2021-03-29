using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Commons;
using tMod64Bot.Services.Logging;
using tMod64Bot.Services.Logging.BotLogging;

namespace tMod64Bot.Modules
{
    [Group("debug")]
    [RequireOwner]
    public class DebugModule : CommandBase
    {
        [Command("purge")]
        public async Task PurgeDebug(SocketGuildChannel channel, ulong amount)
        {
            var channelText = Context.Guild.GetChannel(channel.Id) as ITextChannel;
            
            var messages = await channelText!.GetMessagesAsync(Convert.ToInt32(amount)).FlattenAsync();
            var filteredMessages = messages.Where(x => x.Timestamp < DateTimeOffset.Now.AddDays(14)).ToList();

            if (!filteredMessages.Any())
                await ReplyAsync("Nothing to delete");
            else
            {
                await channelText.DeleteMessagesAsync(filteredMessages);
                await ReplyAsync($"Deleted {filteredMessages.Count}{(filteredMessages.Count() == 1 ? "message" : "messages")}.");
            }
        }

        [Command("clearCache")]
        public async Task ClearCache(int percentage = 100)
        {
            var cache = Services.GetRequiredService<MemoryCache>();

            try
            {
                cache.Trim(percentage);
                await ReplyAsync($"Successfully cleared {percentage}% of the cache");
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error while trimming Cache", e);
            }
        }

        [Command("test")]
        public async Task IsAliveAndNew()
        {
            await ReplyAsync("Alive! Number: 1.1");
        }
        
        [Command("leftUser")]
        public async Task LeftUserTest(ulong id)
        {
            var user = await Context.Client.Rest.GetUserAsync(id);

            await ReplyAsync(user.ToString());
        }
    }
}