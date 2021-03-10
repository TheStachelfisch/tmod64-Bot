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
            if (!File.Exists("update.bash"))
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed("`update.bash` doesn't exist. Stachel forgot to add the script"));
                return;
            }

            var output = "source update.bash".Bash();

            if (output == "Newest")
            {
                await ReplyAsync(embed:EmbedHelper.ErrorEmbed("Nothing to pull"));
                return;
            }
        }
    }
}