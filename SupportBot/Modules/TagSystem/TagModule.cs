using System.Threading.Tasks;
using Discord.Commands;

namespace SupportBot.Modules.TagSystem
{
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        [Command("tag"), Alias("")]
        public async Task TagCommand(string tagName)
            => await ReplyAsync(TagService.GetTagContentByName(tagName));
    }
}