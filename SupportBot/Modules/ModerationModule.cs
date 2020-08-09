using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace SupportBot.Modules
{
	public class ModerationModule : ModuleBase<SocketCommandContext>
	{
		//TODO: Add ban, kick etc. -Doing

		[Command("restart"), Alias("r")]
		[RequireOwner]
		[Summary("Restarts the bot")]
		public async Task HelpAsync()
		{
			await Context.Client.SetStatusAsync(UserStatus.Invisible);
			await Task.Delay(1000);
			Program.StartBotAsync().GetAwaiter().GetResult();
		}

		[Command("kick")]
		[RequireUserPermission(GuildPermission.KickMembers)]
		[Summary("Kicks a user mentioned.")]
		public async Task KickAsync(IGuildUser user, [Remainder] string reason = "No reason specified.")
		{
			await user.KickAsync(reason);
			IUserMessage MessageToDelete = await ReplyAsync("User " + user.Username + " has been kicked. Reason: " + reason);
			await Task.Delay(2500);
			await MessageToDelete.DeleteAsync();
		}
		[Command("ban")]
		[RequireUserPermission(GuildPermission.BanMembers)]
		[Summary("Bans a user mentioned.")]
		public async Task BanAsync(IGuildUser user, int daystoprune = 2, [Remainder] string reason = "No reason specified.")
		{
			
			await user.BanAsync(daystoprune, reason);
			IUserMessage MessageToDelete = await ReplyAsync("User " + user.Username + " has been banned. Reason: " + reason);
			await Task.Delay(5000);
			await MessageToDelete.DeleteAsync();
		}
	}
}