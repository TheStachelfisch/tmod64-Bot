using System.Net.Sockets;
using System.Threading.Tasks;
using Discord.Commands;

namespace SupportBot.Modules.TagSystem
{
    [Group("tag")]
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        [Command("")]
        public async Task TagGetCommand(string tagName)
            => await ReplyAsync(TagService.GetTagContentByName(tagName));

        [Command("add"), Alias("Create")]
        public async Task TagCreateCommand(string tagName, [Remainder]string tagContent)
        {
            await TagService.CreateTag(tagName, tagContent, Context.User.Username, Context.User.Id);
            await ReplyAsync($"Tag '{tagName}' successfully created");
        }

        [Command("delete"), Alias("d", "remove")]
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
    }
}