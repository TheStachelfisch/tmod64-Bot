using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    [RequireOwner]
    public class OwnerModule : ModuleBase
    {
        [Command("update")]
        public async Task Update()
        {
            if (!File.Exists("../update.bash"))
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed("`update.bash` doesn't exist. Stachel forgot to add the script"));
                return;
            }

            var message = await ReplyAsync(embed:new EmbedBuilder()
            {
                Title = "Command Output",
                Description = "Starting command..."
            }.Build());

            string result = "../update.bash".Bash();
            await message.ModifyAsync(x => x.Embed = new EmbedBuilder
            {
                Title = "Command Output",
                Description = result
            }.Build());
        }
    }
}