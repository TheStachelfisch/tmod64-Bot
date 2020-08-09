using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace tMod64Bot.Modules
{
	public class ModerationModule : ModuleBase<SocketCommandContext>
	{

		private long TemporaryMutedRole = 742064452970741811; //NOTE: Stachel, once you add the config.json stuff, please delete this.

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

		[Command("ban"), Alias("b")]
		[RequireUserPermission(GuildPermission.BanMembers)]
		[Summary("Bans a user mentioned.")]
		public async Task BanAsync(IGuildUser user, int daystoprune = 2, [Remainder] string reason = "No reason specified.")
		{
			
			await user.BanAsync(daystoprune, reason);
			IUserMessage MessageToDelete = await ReplyAsync("User " + user.Username + " has been banned. Reason: " + reason);
			await Task.Delay(5000);
			await MessageToDelete.DeleteAsync();
		}

		[Command("mute")]
		[RequireUserPermission(GuildPermission.ManageRoles)]
		[Summary("Adds a muted role onto the user.")]
		public async Task MuteAsync(IGuildUser user, [Remainder] string reason = "No reason specified.") //TODO: temporary unmute, time to unmute
		{
			await user.AddRoleAsync(Context.Client.GetGuild(Context.Guild.Id).GetRole((ulong)TemporaryMutedRole));
			IUserMessage MessageToDelete = await ReplyAsync("User " + user.Username + " was muted for " + "Infinity" + " seconds. Reason: " + reason);
			await Task.Delay(2500);
			await MessageToDelete.DeleteAsync();
		}

		[Command("softban"), Alias("sb")]
		[RequireUserPermission(GuildPermission.BanMembers)]
		[Summary("Banishes a user to the shadow realm!")]
		public async Task SBanAsync(IGuildUser user, [Remainder] string reason = "No reason specified.") //Clone of MuteAsync LUL
		{
			await user.AddRoleAsync(Context.Client.GetGuild(Context.Guild.Id).GetRole((ulong)TemporaryMutedRole));
			IUserMessage MessageToDelete = await ReplyAsync("User " + user.Username + " was banished to the Shadow Realm. Reason: " + reason);
			await Task.Delay(2500);
			await MessageToDelete.DeleteAsync();
		}
		[Command("echo")]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		[Summary("Makes the bot say something in the specified chat, anonymously")]
		public async Task EchoAsync(IGuildChannel channel, [Remainder] string message)
		{
			await Context.Guild.GetTextChannel(channel.Id).SendMessageAsync(message);
		}
	}
}