﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Logging;
using tMod64Bot.Services.Logging.BotLogging;

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

        [Command("channelCode")]
        public async Task ChannelCode(SocketGuildChannel channel)
        {
            await ReplyAsync($"`{MentionUtils.MentionChannel(channel.Id)}`");
        }
    }
}