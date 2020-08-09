using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace SupportBot.Modules.TagSystem
{
    [Group("tag")]
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        [Command("")]
        public async Task TagCommand()
        {
            EmbedBuilder messageEmbed = new EmbedBuilder();

            messageEmbed.WithTitle("Documentation for Tags");
            messageEmbed.WithFooter("Sent at ");
            messageEmbed.WithUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            messageEmbed.WithCurrentTimestamp();

            await ReplyAsync("", false, messageEmbed.Build());
        }
        
        [Command, Alias("t", "g", "get")]
        public async Task TagGetCommand(string tagName)
        {
            var context = Context;
            var tagUser = context.User;
            
            EmbedBuilder tagEmbed = new EmbedBuilder();
            EmbedBuilder errorEmbed = new EmbedBuilder();
            
            if (TagService.GetIfTagExists(tagName))
            {
                foreach (var role in ((SocketGuildUser)Context.Message.Author).Roles)
                {
                    
                }
                
                await TagService.IncreaseUsesForTag(tagName);
                tagEmbed.WithFooter($"Requested by {Context.User.Username}");
                tagEmbed.WithCurrentTimestamp();
                tagEmbed.WithColor(Color.DarkBlue);
                await ReplyAsync(TagService.GetTagContentByName(tagName), false, tagEmbed.Build());
                await Context.Message.DeleteAsync();
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription($"Tag '**{tagName}**' doesn't exist");
                errorEmbed.WithColor(Color.Red);
                errorEmbed.WithFooter("Requested by " + Context.User.Username);
                await ReplyAsync("", false, errorEmbed.Build());
                await Context.Message.DeleteAsync();
            }
        }
        
        [Command("add"), Alias("Create")]
        public async Task TagCreateCommand(string tagName, [Remainder] string tagContent)
        {
            EmbedBuilder errorEmbed = new EmbedBuilder();
            EmbedBuilder successEmbed = new EmbedBuilder();
            
            if (!TagService.GetIfTagExists(tagName))
            {
                await TagService.CreateTag(tagName, tagContent, Context.User.Username, Context.User.Id);
                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription($"Tag '**{tagName}**' was successfully created");
                successEmbed.WithColor(Color.DarkGreen);
                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription($"Tag '**{tagName}**' already exists");
                errorEmbed.WithColor(Color.Red);
                await ReplyAsync("", false, errorEmbed.Build());
            }
        }

        [Command("delete"), Alias("d", "remove", "r")]
        public async Task TagDeleteCommand(string tagName)
        {
            EmbedBuilder errorEmbed = new EmbedBuilder();
            EmbedBuilder successEmbed = new EmbedBuilder();
            
            if (TagService.GetIfTagExists(tagName))
            {
                await TagService.DeleteTagByName(tagName);
                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription($"Tag '**{tagName}**' was successfully deleted");
                successEmbed.WithColor(Color.DarkGreen);
                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription($"Tag '**{tagName}**' doesn't exists");
                errorEmbed.WithColor(Color.Red);
                await ReplyAsync("", false, errorEmbed.Build());
            }
        }

        [Command("edit"), Alias("e", "change")]
        public async Task TagEditCommand(string tagName, [Remainder]string tagNewContent)
        {
            EmbedBuilder errorEmbed = new EmbedBuilder();
            EmbedBuilder successEmbed = new EmbedBuilder();
            
            if (TagService.GetIfTagExists(tagName))
            {
                await TagService.EditTag(tagName, tagNewContent);
                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription($"Tag '**{tagName}**' was successfully edited");
                successEmbed.WithColor(Color.DarkGreen);
                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription($"Tag '**{tagName}**' doesn't exists");
                errorEmbed.WithColor(Color.Red);
                await ReplyAsync("", false, errorEmbed.Build());
            }
        }

        [Command("info")]
        public async Task TagInfoCommand(string tagName)
        {               
            EmbedBuilder errorEmbed = new EmbedBuilder();
            EmbedBuilder embedBuilder = new EmbedBuilder();

            if (TagService.GetIfTagExists(tagName))
            {
                foreach (var tag in TagService.GetTag(tagName))
                {
                    embedBuilder.WithTitle(tag.Name);
                    embedBuilder.WithDescription(
                        $"**Created By**\n {Context.Client.GetUser(tag.OwnerId).Mention}\n\n **Uses** \n{TagService.GetUsesForTag(tagName)}");
                    embedBuilder.WithColor(Color.DarkGreen);
                    embedBuilder.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.Client.GetUser(tag.OwnerId).GetAvatarUrl());
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
            EmbedBuilder embedBuilder = new EmbedBuilder();

            foreach (var tags in TagService.GetAllTags())
            {
                embedBuilder.AddField(tags.Name, $"Owner: {tags.OwnerName}");
            }

            await ReplyAsync("", false, embedBuilder.Build());
        }
    }
}