using System;
using System.Net.Mime;
using System.Net.Sockets;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace SupportBot.Modules
{
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        //TODO: Add ban, kick etc.

        [Command("suicide")]
        public async Task HelpAsync()
        {
            await Program.SetShuttingDown();
            Environment.Exit(1);   
        }
    }
}