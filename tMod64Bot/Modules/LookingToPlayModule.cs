using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using tMod64Bot.Modules.Commons;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Commons;
using tMod64Bot.Services;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace tMod64Bot.Modules
{
    [RequireBotManager]
    public class LookingToPlayModule : CommandBase
    {
        private readonly ConfigService _configService;
        private readonly WebhookService _webhook;

		public LookingToPlayModule(IServiceProvider services)
		{
			_webhook = services.GetRequiredService<WebhookService>();
			_configService = services.GetRequiredService<ConfigService>();
		}
		[Command("ltpbutton")]
		public async Task LookingToPlay()
        {
			await (Context.Guild.GetChannel(_configService.Config.LookingToPlayChannel) as SocketTextChannel)!.SendMessageAsync("Cock", components: new ComponentBuilder().WithButton("Test", "looking-to-play-request").Build());
        }
    }
}