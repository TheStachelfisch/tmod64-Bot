using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Tag;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    [Group("tag"), Alias("tags")]
    public class TagModule : CommandBase
    {
        [Command("add"), Alias("create")]
        [Priority(5)]
        public async Task AddTag(string name, [Remainder]string content)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            TagService tagService = Services.GetRequiredService<TagService>();
            
            Tag tag = new()
            {
                Name = name.ToLower(),
                Content = content,
                Owner = Context.User.Id,
                CreatedAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Uses = 0,
            };
            
            var result = await tagService.AddTag(tag);

            if (!result.IsSuccess)
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed(result.ErrorReason));
                return;
            }

            sw.Stop();
            await LoggingService.Log($"Added Tag in {sw.ElapsedMilliseconds}ms");
            
            await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully added tag '{name}'"));
        }

        [Command("delete"), Alias("remove")]
        [Priority(5)]
        public async Task RemoveTag(string tagName)
        {
            TagService tagService = Services.GetRequiredService<TagService>();
            var guildUser = Context.User as SocketGuildUser;
            
            tagName = tagName.ToLower();

            if (ConfigService.Config.Tags.All(x => x.Name != tagName))
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed($"Tag `{tagName}` doesn't exist"));
                return;
            }
            
            if (ConfigService.Config.Tags.First(x => x.Name == tagName).Owner != Context.User.Id && !guildUser!.GuildPermissions.Administrator)
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed($"You can only delete your own tag/s"));
                return;
            }

            await tagService.RemoveTag(tagName);
            await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully removed tag '{tagName}'"));
        }
        
        [Command("edit")]
        [Priority(5)]
        public async Task EditTag(string tagName, [Remainder]string newContent)
        {
            TagService tagService = Services.GetRequiredService<TagService>();
            var guildUser = Context.User as SocketGuildUser;
            
            tagName = tagName.ToLower();

            if (ConfigService.Config.Tags.All(x => x.Name != tagName))
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed($"Tag `{tagName}` doesn't exist"));
                return;
            }
            
            if (ConfigService.Config.Tags.First(x => x.Name == tagName).Owner != Context.User.Id && !guildUser!.GuildPermissions.Administrator)
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed($"You can only edit your own tag/s"));
                return;
            }

            await tagService.EditTag(tagName, newContent);
            await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully edited tag '{tagName}'"));
        }

        [Command("list"), Alias("get")]
        [Priority(5)]
        public async Task ListTags()
        {
            Embed embed = new EmbedBuilder()
            {
                Title = "All tags",
                Description = $"``{String.Join(", ", ConfigService.Config.Tags.Select(x => x.Name))}``",
                Color = Color.Blue
            }.Build();

            await ReplyAsync(embed: embed);
        }
        
        [Command]
        public async Task GetTag(string tagName)
        {
            tagName = tagName.ToLower();
            
            TagService tagService = Services.GetRequiredService<TagService>();
            Stopwatch sw = Stopwatch.StartNew();

            Tag result;
            
            try { result = await tagService.GetTag(tagName); }
            catch (Exception e)
            {
                await ReplyAsync(embed:EmbedHelper.ErrorEmbed($"Tag '{tagName}' doesn't exist"));
                return;
            }

            sw.Stop();
            await LoggingService.Log($"Got tag in {sw.ElapsedTicks}ms");
            
            await ReplyAsync($"**Tag: {tagName}**\n{result.Content}");
        }
    }
}