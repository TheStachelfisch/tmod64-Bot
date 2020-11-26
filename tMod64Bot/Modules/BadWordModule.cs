using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using tMod64Bot.Services;

namespace tMod64Bot.Modules
{
    [Group("badword"), Alias("bw", "bannedwords", "badwords", "b")]
    public class BadWordModule : ModuleBase<SocketCommandContext>
    {
        public ConfigService Config { get; set; }

        public CensorshipService BadWordsService { get; set; }

        [Group("channel"), Alias("ch", "channels")]
        class Channel : ModuleBase<SocketCommandContext>
        {
            public ConfigService Config { get; set; }

            [Command("")]
            public async Task BadWordCommandChannel()
            {
                var messageEmbed = new EmbedBuilder();

                messageEmbed.WithTitle("Documentation for Bad word channel whitelist");
                messageEmbed.WithFooter("Sent at ");
                messageEmbed.WithUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
                messageEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, messageEmbed.Build());
            }

            [Command("all"), Alias("get", "list")]
            public async Task Channels()
            {
                EmbedBuilder wordEmbed = new EmbedBuilder();

                var user = Context.User as SocketGuildUser;

                if (user.Roles.Contains(Config.ManagerRole) || user.GuildPermissions.Administrator)
                {
                    wordEmbed.WithTitle("Channels:");
                    wordEmbed.WithDescription(string.Join(", ", Config.BadWordChannelWhitelist));
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
            public async Task AddChannel([Remainder] string channelString)
            {
                EmbedBuilder embed = new EmbedBuilder();
                ulong channel;

                try
                { 
                    channel = ulong.Parse(channelString); 
                }

                catch (Exception)
                {
                    embed.WithTitle("Error!");
                    embed.WithDescription($"'**{channelString}**' isn't a unsigned long");
                    embed.WithColor(Color.Red);
                    embed.WithCurrentTimestamp();

                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                var user = Context.User as SocketGuildUser;

                if (user.Roles.Contains(Config.ManagerRole) || user.GuildPermissions.Administrator)
                {
                    if (Config.BadWordChannelWhitelist.Add(channel))
                    {
                        embed.WithTitle("Success!");
                        embed.WithDescription($"Successfully added '**{channel}**' to banned words");
                        embed.WithColor(Color.Green);
                        embed.WithCurrentTimestamp();
                    }
                    else
                    {
                        embed.WithTitle("Error!");
                        embed.WithDescription($"'**{channel}**' Already exists");
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

            [Command("add"), Alias("a")]
            public async Task AddChannel([Remainder] SocketTextChannel channel) => await AddChannel(channel.Id.ToString());

            [Command("remove"), Alias("r", "delete", "d")]
            public async Task RemoveChannel([Remainder] string channelString)
            {
                EmbedBuilder embed = new EmbedBuilder();


                ulong channel = 0;
                try
                { channel = ulong.Parse(channelString); }
                catch (Exception e)
                {
                    embed.WithTitle("Error!");
                    embed.WithDescription($"'**{channelString}**' isn't a unsigned long");
                    embed.WithColor(Color.Red);
                    embed.WithCurrentTimestamp();

                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                var user = Context.User as SocketGuildUser;

                if (user.Roles.Contains(Config.ManagerRole) || user.GuildPermissions.Administrator)
                {
                    if (Config.BadWordChannelWhitelist.Remove(channel))
                    {
                        embed.WithTitle("Success!");
                        embed.WithDescription($"Successfully removed '**{channel}**' from whitelisted channels");
                        embed.WithColor(Color.Green);
                        embed.WithCurrentTimestamp();
                    }
                    else
                    {
                        embed.WithTitle("Error!");
                        embed.WithDescription($"'**{channel}**' Doesn't exists");
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

        [Command("all"), Alias("get", "list")]
        public async Task BadWords()
        {
            EmbedBuilder wordEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            if (user.Roles.Contains(Config.ManagerRole) || user.GuildPermissions.Administrator)
            {
                wordEmbed.WithTitle("Words: ");
                wordEmbed.WithDescription(string.Join(", ", BadWordsService.Words));
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
        public async Task AddWord([Remainder] string word)
        {
            EmbedBuilder embed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            if (user.Roles.Contains(Config.ManagerRole) || user.GuildPermissions.Administrator)
            {
                if (BadWordsService.Words.Add(word))
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
        public async Task RemoveWord([Remainder] string word)
        {
            EmbedBuilder embed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            if (user.Roles.Contains(Config.ManagerRole) || user.GuildPermissions.Administrator)
            {
                if (BadWordsService.Words.Remove(word))
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
    }
}