using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    public class InfoModule : CommandBase
    {
        [Command("info")]
        public async Task BotInfo()
        {
            Embed embed = new EmbedBuilder
            {
                Title = "Bot Info",
                Color = Color.Green,
                Description = $"The bot was made by {MentionUtils.MentionUser(442639987180306432)}\n\n" +
                              $"Current Prefix **{ConfigService.Config.BotPrefix}**\n" +
                              $"Current Bot uptime **{tMod64bot.GetUptime.FormatString(true)}min**\n" +
                              $"Amount of tags: {ConfigService.Config.Tags.Count}\n",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            }.Build();

            await ReplyAsync(embed: embed);
        }

        [Command("ping"), Alias("latency")]
        public async Task PingInfo()
        {
            int latency = Context.Client.Latency;
            long milliseconds = 0;

            EmbedBuilder embed = new()
            {
                Title = $"Current bot ping",
                Description = $"**Gateway Ping:** ``{latency}ms``\n" +
                              "**Message Response Time:** *Evaluating*\n" +
                              "**Delta:** *Evaluating*",
            };

            Stopwatch sw = Stopwatch.StartNew();

            var message = await ReplyAsync(embed: embed.Build());

            sw.Stop();
            milliseconds = sw.ElapsedMilliseconds;

            Color color = Color.LightGrey;
            String indication = "";
            switch (milliseconds - latency)
            {
                case >1000:
                    color = Color.DarkRed;
                    indication = "Bad";
                    break;
                case >750:
                    color = Color.Red;
                    indication = "Mediocre";
                    break;
                case >250:
                    color = Color.Orange;
                    indication = "Good";
                    break;
                case <250:
                    color = Color.Green;
                    indication = "Excellent";
                    break;
            }
            
            EmbedBuilder embed2 = new()
            {
                Title = $"Current bot ping: *{indication}*",
                Description = $"**Gateway Ping:** ``{Context.Client.Latency}ms``\n" +
                              $"**Message Response Time:** {(milliseconds == 0 ? "*Evaluating*" : $"`{milliseconds}ms`")}\n" +
                              $"**Delta:** {(milliseconds == 0 ? "*Evaluating*" : $"`{milliseconds - latency}ms`")}",
                Color = color
            };
            
            await message.ModifyAsync(m => m.Embed = embed2.Build());
        }

        [Command("help")]
        public async Task Help()
        {
            var embed = new EmbedBuilder
            {
                Title = "Command Documentation",
                Color = Color.DarkGreen,
                Description = "[Command documentation can be found here](https://github.com/TheStachelfisch/tmod64-Bot/tree/Documentation/Command%20Documentation)"
            }.Build();

            await ReplyAsync(embed:embed);
        }
    }
}