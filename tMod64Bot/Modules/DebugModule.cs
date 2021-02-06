using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Webhook;
using tMod64Bot.Services.Config;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    public class DebugModule : CommandBase
    {
        [Command("wh")]
        public async Task WebhookDebugging()
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            var logChannel = Context.Guild.GetChannel(ConfigService.Config.UserLoggingChannel) as ITextChannel;
            var webhook = logChannel!.GetWebhooksAsync().Result.FirstOrDefault(x => x.Name.Equals("tMod64-Logging"));
            
            using (var client = new DiscordWebhookClient(webhook.GetUrl()))
            {   
                await client.SendMessageAsync("Hello, World!");
            };
            
            sw.Stop();
            await ReplyAsync($"It took {sw.ElapsedMilliseconds}ms to execute");
        }
    }
}