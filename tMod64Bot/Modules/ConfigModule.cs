using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using tMod64Bot.Modules.Commons;

namespace tMod64Bot.Modules
{
    [Group("config"), Alias("cfg")]
    [BotManagementPerms]
    public class ConfigModule : CommandBase
    {
        [Command("change"), Alias("update")]
        public async Task ChangeValue(string key, string value)
        {
            var embed = new EmbedBuilder();

            try
            {
                var success = ConfigService.ChangeKey(key, value);
                
                if (!success)
                {
                    embed.WithTitle("Error!");
                    embed.WithColor(Color.Red);
                    embed.WithDescription("The key you are trying to access doesn't exist");
                    embed.WithCurrentTimestamp();
                    
                    await ReplyAsync(embed: embed.Build());
                    return;
                }
            }
            catch (Exception e)
            {
                embed.WithTitle("Error!");
                embed.WithColor(Color.Red);
                embed.WithDescription($"{e.Message}\n**Note: You can only change Integer and String values in the config**");
                embed.WithCurrentTimestamp();

                await ReplyAsync(embed: embed.Build());
                return;
            }

            embed.WithTitle("Success!");
            embed.WithColor(Color.Green);
            embed.WithDescription($"Successfully changed the value of key '**{key}**' to '**{value}**'");
            embed.WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }
    }
}