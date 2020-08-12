using System.Threading.Tasks;
using Discord.Commands;

namespace tMod64Bot.Modules.ConfigSystem
{
    [Group("config")]
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        [Command("BotManagerRole")]
        public async Task ConfigTest()
        {
            await ReplyAsync(ConfigService.GetConfig(ConfigEnum.BotOwner).ToString());
        }
    }
}