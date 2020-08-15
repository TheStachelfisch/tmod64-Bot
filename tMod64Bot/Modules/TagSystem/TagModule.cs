using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using tMod64Bot.Modules.ConfigSystem;

namespace tMod64Bot.Modules.TagSystem
{
    [Group("tag")]
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        [Command("")]
        public async Task TagCommand()
        {
            var messageEmbed = new EmbedBuilder();

            messageEmbed.WithTitle("Documentation for Tags");
            messageEmbed.WithFooter("Sent at ");
            messageEmbed.WithUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            messageEmbed.WithCurrentTimestamp();

            await ReplyAsync("", false, messageEmbed.Build());
        }

        [Command]
        [Alias("t", "g", "get")]
        public async Task TagGetCommand(string tagName)
        {
            var embed = new EmbedBuilder();

            if (TagService.GetIfTagExists(tagName))
            {
                await TagService.IncreaseUsesForTag(tagName);
                embed.WithFooter($"Requested by {Context.User.Username}");
                embed.WithCurrentTimestamp();
                embed.WithColor(Color.DarkBlue);
                await ReplyAsync(TagService.GetTagContentByName(tagName), false, embed.Build());
                await Context.Message.DeleteAsync();
            }
            else
            {
                embed.WithTitle("Error!");
                embed.WithDescription($"Tag '**{tagName}**' doesn't exist");
                embed.WithColor(Color.Red);
                embed.WithFooter("Requested by " + Context.User.Username);
                await ReplyAsync("", false, embed.Build());
                await Context.Message.DeleteAsync();
            }
        }

        [Command("add")]
        [Alias("Create")]
        public async Task TagCreateCommand(string tagName, [Remainder] string tagContent)
        {
            var embed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                if (!TagService.GetIfTagExists(tagName))
                {
                    await TagService.CreateTag(tagName, tagContent, Context.User.Username, Context.User.Id);
                    embed.WithTitle("Success!");
                    embed.WithDescription($"Tag '**{tagName}**' was successfully created");
                    embed.WithColor(Color.DarkGreen);
                }
                else
                {
                    embed.WithTitle("Error!");
                    embed.WithDescription($"Tag '**{tagName}**' already exists");
                    embed.WithColor(Color.Red);
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

        [Command("delete")]
        [Alias("d", "remove", "r")]
        public async Task TagDeleteCommand(string tagName)
        {
            var embed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                if (TagService.GetIfTagExists(tagName))
                {
                    await TagService.DeleteTagByName(tagName);
                    embed.WithTitle("Success!");
                    embed.WithDescription($"Tag '**{tagName}**' was successfully deleted");
                    embed.WithColor(Color.DarkGreen);
                    embed.WithCurrentTimestamp();
                }
                else
                {
                    embed.WithTitle("Error!");
                    embed.WithDescription($"Tag '**{tagName}**' doesn't exists");
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

        [Command("edit")]
        [Alias("e", "change")]
        public async Task TagEditCommand(string tagName, [Remainder] string tagNewContent)
        {
            var embed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                if (TagService.GetIfTagExists(tagName))
                {
                    await TagService.EditTag(tagName, tagNewContent);
                    embed.WithTitle("Success!");
                    embed.WithDescription($"Tag '**{tagName}**' was successfully edited");
                    embed.WithColor(Color.DarkGreen);
                }
                else
                {
                    embed.WithTitle("Error!");
                    embed.WithDescription($"Tag '**{tagName}**' doesn't exists");
                    embed.WithColor(Color.Red);
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

        [Command("info")]
        public async Task TagInfoCommand(string tagName)
        {
            var errorEmbed = new EmbedBuilder();
            var embedBuilder = new EmbedBuilder();

            if (TagService.GetIfTagExists(tagName))
            {
                foreach (var tag in TagService.GetTag(tagName))
                {
                    embedBuilder.WithTitle(tag.Name);
                    embedBuilder.WithDescription(
                        $"**Created By**\n {Context.Client.GetUser(tag.OwnerId).Mention}\n\n **Uses** \n{TagService.GetUsesForTag(tagName)}");
                    embedBuilder.WithColor(Color.DarkGreen);
                    embedBuilder.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}",
                        Context.Client.GetUser(tag.OwnerId).GetAvatarUrl());
                    embedBuilder.WithFooter("Tag created at ");
                    embedBuilder.WithTimestamp(DateTimeOffset.FromUnixTimeSeconds(tag.CreatedAt));
                }

                await ReplyAsync("", false, embedBuilder.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription($"Tag '**{tagName}**' doesn't exists");
                errorEmbed.WithColor(Color.Red);
                await ReplyAsync("", false, errorEmbed.Build());
            }
        }
    }

    [Group("tags")]
    public class GetTagModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task TagGetAll()
        {
            var embedBuilder = new EmbedBuilder();

            foreach (var tags in TagService.GetAllTags()) embedBuilder.AddField(tags.Name, $"Owner: {Context.Client.GetUser(tags.OwnerId).Mention}");

            await ReplyAsync("", false, embedBuilder.Build());
        }
    }
}