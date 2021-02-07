using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace tMod64Bot.Modules
{
    public class InfoModule : CommandBase
    {
        [Command("info")]
        public async Task BotInfo()
        {
            decimal minuteUptime = Math.Round((decimal) tMod64bot.GetUptime / 60, 2);
            
            Embed embed = new EmbedBuilder
            {
                Title = "Bot Info",
                Color = Color.Green,
                Description = $"Current Prefix **{ConfigService.Config.BotPrefix}**\n" +
                              $"Current Bot uptime **{minuteUptime}min**\n" +
                              $"Amount of tags: \n",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            }.Build();

            await ReplyAsync(embed: embed);
        }
    }
}