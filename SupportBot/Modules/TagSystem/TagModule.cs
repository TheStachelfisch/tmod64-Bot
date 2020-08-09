using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace SupportBot.Modules.TagSystem
{
    [Group("tag")]
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        [Command, Alias("t", "g", "get")]
        public async Task TagGetCommand(string tagName)
        {
            if (TagService.GetIfTagExists(tagName))
            {
                await ReplyAsync(TagService.GetTagContentByName(tagName));
            }
            else
            {
                await ReplyAsync($"Tag '**{tagName}**' doesn't exist");
            }
        }
        
        [Command("add"), Alias("Create")]
        public async Task TagCreateCommand(string tagName, [Remainder] string tagContent)
        {
            await TagService.CreateTag(tagName, tagContent, Context.User.Username, Context.User.Id);
            await ReplyAsync($"Tag '{tagName}' successfully created");
        }

        [Command("delete"), Alias("d", "remove", "r")]
        public async Task TagDeleteCommand(string tagName)
        {
            if (TagService.GetIfTagExists(tagName))
            {
                await TagService.DeleteTagByName(tagName);
                await ReplyAsync($"Tag '{tagName}' successfully deleted");
            }
            else
            {
                await ReplyAsync($"Tag '**{tagName}**' doesn't exists");
            }
        }

        [Command("edit"), Alias("e", "change")]
        public async Task TagEditCommand(string tagName, [Remainder]string tagNewContent)
        {
            if (TagService.GetIfTagExists(tagName))
            {
                await TagService.EditTag(tagName, tagNewContent);
                await ReplyAsync($"Edited tag '**{tagName}**' successfully");
            }
            else
            {
                await ReplyAsync($"Tag '**{tagName}**' doesn't exist");
            }
        }

        [Command("info")]
        public async Task TagInfoCommand(string tagName)
        {               
            EmbedBuilder embedBuilder = new EmbedBuilder();

            if (TagService.GetIfTagExists(tagName))
            {
                foreach (var tag in TagService.GetTag(tagName))
                {
                    embedBuilder.WithTitle($"Tag: {tag.Name}");
                    embedBuilder.WithDescription(
                        $"**Owner:** {tag.OwnerName}\n**Owner Id:** {tag.OwnerId}\n**Created at:** {Helper.UnixTimeStampToDateTime(tag.CreatedAt)} UTC");
                    embedBuilder.WithColor(Color.DarkGreen);
                    embedBuilder.WithFooter("Sent at ");
                    embedBuilder.WithCurrentTimestamp();
                    
                    await ReplyAsync("", false, embedBuilder.Build());
                }
            }
            else
            {
                await ReplyAsync($"Tag '**{tagName}**' doesn't exist");
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