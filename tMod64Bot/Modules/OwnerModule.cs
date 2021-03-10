using System.Threading.Tasks;
using Discord.Commands;

namespace tMod64Bot.Modules
{
    [RequireOwner]
    public class OwnerModule : ModuleBase
    {
        [Command("update")]
        public async Task Update()
        {
            await ReplyAsync("Test");
        }
    }
}