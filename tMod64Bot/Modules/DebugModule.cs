using System;
using System.Threading.Tasks;
using Discord.Commands;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Modules
{
    public class DebugModule : CommandBase
    {
        [Command("ct")]
        public async Task ConfigDebug(string key, string value)
        {
            await ReplyAsync($"Old Prefix {ConfigService.Config.BotPrefix}");

            try
            {
                var success = ConfigService.ChangeKey(key, value);

                if (!success)
                {
                    await ReplyAsync("Key does not exist");
                    return;
                }
            }
            catch (Exception e)
            {
                await ReplyAsync($"`Error: {e.Message}`\n**Note: You can only change String And Integer config values**");
                return;
            }

            await ReplyAsync("Successfully updated Data");
        }
    }
}