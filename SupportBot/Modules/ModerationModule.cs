using System;
using System.Net.Mime;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace SupportBot.Modules
{
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        //TODO: Add ban, kick etc.

        [Command("restart"), Alias("r")]
        [RequireOwner]
        [Summary("Shuts down the bot")]
        public async Task HelpAsync()
        {
            await Context.Client.SetStatusAsync(UserStatus.Invisible);
            await Task.Delay(1000);
            Program.StartBotAsync().GetAwaiter().GetResult();
        }
    }
}