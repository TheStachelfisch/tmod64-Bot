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
            if (!File.Exists("update.bash"))
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed("`update.bash` doesn't exist. Stachel forgot to add the script"));
                return;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "source",
                    Arguments = "update.bash",
                    RedirectStandardOutput = true,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                }
            };

            process.ToString();
            
            process.Start();
            string result = process.StandardOutput.ReadToEndAsync().Result;
            process.WaitForExit();
            

            if (result == "Newest")
            {
                await ReplyAsync(embed:EmbedHelper.ErrorEmbed("Nothing to pull"));
                return;
            }

            await ReplyAsync($"Bot should already be down... Script most likely failed to execute\n```{result}```");
        }
    }
}