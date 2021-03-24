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

            var message = await ReplyAsync("Starting command...");
            
            ProcessStartInfo startInfo = new() { FileName = "/bin/bash", Arguments = "../update.bash", };
            Process proc = new() {StartInfo = startInfo,};
            proc.Start();

            string result = await proc.StandardOutput.ReadToEndAsync();
            await message.ModifyAsync(x => x.Content = result);
        }
    }
}