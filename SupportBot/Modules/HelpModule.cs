using Discord.Commands;
using System.Threading.Tasks;

namespace SupportBot.Modules
{
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		[Command("help")]
		public async Task HelpAsync()
			=> await ReplyAsync($"{Context.User.Mention} Gay!");
	}
}