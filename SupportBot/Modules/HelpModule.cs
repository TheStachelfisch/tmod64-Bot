using Discord.Commands;
using System.Threading.Tasks;

namespace SupportBot.Modules
{
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		[Command("ping")]
		public async Task HelpAsync()
			=> await ReplyAsync($"{Context.User.Mention} pong motherfucker");
	}
}