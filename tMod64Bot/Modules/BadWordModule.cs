using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using tMod64Bot.Handler;
using tMod64Bot.Modules.ConfigSystem;

namespace tMod64Bot.Modules
{
    [Group("badword"), Alias("bw", "bannedwords", "badwords", "b")]
    public class BadWordModule : ModuleBase<SocketCommandContext>
    {
        [Command("")]
        public async Task BadWordCommand()
        {
            var messageEmbed = new EmbedBuilder();

            messageEmbed.WithTitle("Documentation for Bad words");
            messageEmbed.WithFooter("Sent at ");
            messageEmbed.WithUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            messageEmbed.WithCurrentTimestamp();

            await ReplyAsync("", false, messageEmbed.Build());
        }
        
        [Command("all"), Alias("get", "list")]
        public async Task BadWords()
        {
            EmbedBuilder wordEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)));
            
            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                wordEmbed.WithTitle("Words:");
                wordEmbed.WithDescription(string.Join(", ", BadWordHandler.GetList().ToArray()));
                wordEmbed.WithColor(Color.Green);
                wordEmbed.WithCurrentTimestamp();
            }
            else
            {
                wordEmbed.WithTitle("Error!");
                wordEmbed.WithDescription("Missing Bot Manager permissions");
                wordEmbed.WithColor(Color.Red);
                wordEmbed.WithCurrentTimestamp();
            }
            
            await ReplyAsync("", false, wordEmbed.Build());
        }
        
        [Command("add"), Alias("a")]
        public async Task AddWord([Remainder]string word)
        {
            EmbedBuilder embed = new EmbedBuilder();
            
            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                if (BadWordHandler.AddBadWord(word).Result)
                {
                    embed.WithTitle("Success!");
                    embed.WithDescription($"Successfully added '**{word.ToLower()}**' to banned words");
                    embed.WithColor(Color.Green);
                    embed.WithCurrentTimestamp();
                }
                else
                {
                    embed.WithTitle("Error!");
                    embed.WithDescription($"'**{word.ToLower()}**' Already exists");
                    embed.WithColor(Color.Red);
                    embed.WithCurrentTimestamp();
                }
            }
            else
            {
                embed.WithTitle("Error!");
                embed.WithDescription("Missing Bot Manager permissions");
                embed.WithColor(Color.Red);
                embed.WithCurrentTimestamp();
            }

            await ReplyAsync("", false, embed.Build());
        }

        [Command("remove"), Alias("r", "delete", "d")]
        public async Task RemoveWord([Remainder]string word)
        {
            EmbedBuilder embed = new EmbedBuilder();
            
            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                if (BadWordHandler.RemoveBadWord(word).Result)
                {
                    embed.WithTitle("Success!");
                    embed.WithDescription($"Successfully removed '**{word.ToLower()}**' from banned words");
                    embed.WithColor(Color.Green);
                    embed.WithCurrentTimestamp();
                }
                else
                {
                    embed.WithTitle("Error!");
                    embed.WithDescription($"'**{word.ToLower()}**' Doesn't exists");
                    embed.WithColor(Color.Red);
                    embed.WithCurrentTimestamp();
                }
            }
            else
            {
                embed.WithTitle("Error!");
                embed.WithDescription("Missing Bot Manager permissions");
                embed.WithColor(Color.Red);
                embed.WithCurrentTimestamp();
            }

            await ReplyAsync("", false, embed.Build());
        }
    }
}