using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace SupportBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpAsync()
            => await ReplyAsync($"{Context.User.Mention} Gay!");
    }
}