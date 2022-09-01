using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using tMod64Bot.Modules.Commons;

namespace tMod64Bot.Modules
{
    [RequireBotManager]
    public class LookingToPlayModule : CommandBase
    {
        [Command("ltpbutton")]
        public async Task LookingToPlay()
        {
            // TODO: Replace with ltp channel from Config
            await (Context.Guild.GetChannel(834388466468651028) as SocketTextChannel)!.SendMessageAsync("Cock", components: new ComponentBuilder().WithButton("Test", "looking-to-play-request").Build());
        }
    }
}