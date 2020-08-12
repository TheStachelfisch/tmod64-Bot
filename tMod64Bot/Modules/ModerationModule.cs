using System;
using System.Linq;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using tMod64Bot.Modules.ConfigSystem;

namespace tMod64Bot.Modules
{
	public class ModerationModule : ModuleBase<SocketCommandContext>
	{
		private ulong MutedRole = ulong.Parse(ConfigService.GetConfig(ConfigEnum.MutedRole));

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
			await user.AddRoleAsync(Context.Client.GetGuild(Context.Guild.Id).GetRole(MutedRole));
			IUserMessage MessageToDelete = await ReplyAsync("User " + user.Username + " was muted for " + "Infinity" + " seconds. Reason: " + reason);
			await Task.Delay(2500);
			await MessageToDelete.DeleteAsync();
		}

		[Command("softban"), Alias("sb")]
		[RequireUserPermission(GuildPermission.BanMembers)]
		[Summary("Banishes a user to the shadow realm!")]
		public async Task SBanAsync(IGuildUser user, [Remainder] string reason = "No reason specified.")
		{
			await user.AddRoleAsync(Context.Client.GetGuild(Context.Guild.Id).GetRole(MutedRole));
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

	[Group("purge")]
	public class PurgeModule : ModuleBase<SocketCommandContext>
	{
		[Command]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		[Summary("purges a specified amount of messages")]
		public async Task PurgeMessageAsync(int amount)
		{
			//Embeds are pretty you know
			EmbedBuilder amountErrorEmbed = new EmbedBuilder();
			EmbedBuilder successEmbed = new EmbedBuilder();
			
			if (amount <= 0)
			{
				amountErrorEmbed.WithTitle("Error!");
				amountErrorEmbed.WithDescription("The amount of messages cant be zero or negative");
				amountErrorEmbed.WithColor(Color.Red);
				amountErrorEmbed.WithCurrentTimestamp();
				await ReplyAsync("", false, amountErrorEmbed.Build());
			}

			var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

			var filteredMessages = messages.Where(x => (DateTimeOffset.Now - x.Timestamp).TotalDays <= 14);

			var count = filteredMessages.Count();

			if (count == 0)
			{
				amountErrorEmbed.WithTitle("Error!");
				amountErrorEmbed.WithDescription("No messages found to delete");
				amountErrorEmbed.WithColor(Color.Red);
				amountErrorEmbed.WithCurrentTimestamp();
				await ReplyAsync("", false, amountErrorEmbed.Build());
			}
			else
			{
				await (Context?.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
				successEmbed.WithTitle($"Successfully deleted {filteredMessages.Count()} messages");
				successEmbed.WithColor(Color.DarkGreen);
				successEmbed.WithCurrentTimestamp();

				var embedMessage = await ReplyAsync("", false, successEmbed.Build());
				await Task.Delay(2500);
				
				await embedMessage.DeleteAsync();
				await Context.Message.DeleteAsync();
			}
		}

		[Command("user")]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		[Summary("purges a specified amount of messages from a specific user")]
		public async Task PurgeUserAsync(IGuildUser user, int messageCount)
		{
			EmbedBuilder amountErrorEmbed = new EmbedBuilder();
			EmbedBuilder successEmbed = new EmbedBuilder();
			
			if (messageCount <= 0)
			{
				amountErrorEmbed.WithTitle("Error!");
				amountErrorEmbed.WithDescription("The amount of messages cant be zero or negative");
				amountErrorEmbed.WithColor(Color.Red);
				amountErrorEmbed.WithCurrentTimestamp();
				var embedMessage = await ReplyAsync("", false, amountErrorEmbed.Build());
				
				await Task.Delay(2500);
				
				await embedMessage.DeleteAsync();
				await Context.Message.DeleteAsync();
			}

			var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, messageCount).FlattenAsync();

			var filteredMessages = messages.Where(x => (DateTimeOffset.Now - x.Timestamp).TotalDays <= 14).Where(u => u.Author.Id.Equals(user.Id));

			var count = filteredMessages.Count();

			if (count == 0)
			{
				amountErrorEmbed.WithTitle("Error!");
				amountErrorEmbed.WithDescription("No messages found to delete");
				amountErrorEmbed.WithColor(Color.Red);
				amountErrorEmbed.WithCurrentTimestamp();
				var embedMessage = await ReplyAsync("", false, amountErrorEmbed.Build());
				
				await Task.Delay(2500);
				
				await embedMessage.DeleteAsync();
				await Context.Message.DeleteAsync();
			}
			else
			{
				await (Context?.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
				successEmbed.WithTitle($"Successfully deleted {filteredMessages.Count()} messages from");
				successEmbed.WithDescription(MentionUtils.MentionUser(user.Id));
				successEmbed.WithColor(Color.DarkGreen);
				successEmbed.WithCurrentTimestamp();

				var embedMessage = await ReplyAsync("", false, successEmbed.Build());
				await Task.Delay(2500);
				
				await embedMessage.DeleteAsync();
				await Context.Message.DeleteAsync();
			}
		}

		[Command("contains"), Alias("contain")]
		[Summary("Deletes a specified amount of message that contain a certain string")]
		[RequireBotPermission(GuildPermission.ManageMessages)]
		public async Task PurgeContainAsync(int amount, string contain)
		{
			contain = contain.ToLower();
			
			EmbedBuilder amountErrorEmbed = new EmbedBuilder();
			EmbedBuilder successEmbed = new EmbedBuilder();
			
			if (amount <= 0)
			{
				amountErrorEmbed.WithTitle("Error!");
				amountErrorEmbed.WithDescription("The amount of messages cant be zero or negative");
				amountErrorEmbed.WithColor(Color.Red);
				amountErrorEmbed.WithCurrentTimestamp();
				await ReplyAsync("", false, amountErrorEmbed.Build());
			}

			var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

			var filteredMessages = messages.Where(x => (DateTimeOffset.Now - x.Timestamp).TotalDays <= 14).Where(m => m.Content.ToLower().Contains(contain));

			var count = filteredMessages.Count();

			if (count == 0)
			{
				amountErrorEmbed.WithTitle("Error!");
				amountErrorEmbed.WithDescription($"No messages found that contained **'{contain}'***");
				amountErrorEmbed.WithColor(Color.Red);
				amountErrorEmbed.WithCurrentTimestamp();
				await ReplyAsync("", false, amountErrorEmbed.Build());
			}
			else
			{
				await (Context?.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
				successEmbed.WithTitle($"Successfully deleted {filteredMessages.Count()} messages that contained '**{contain}**'");
				successEmbed.WithColor(Color.DarkGreen);
				successEmbed.WithCurrentTimestamp();

				var embedMessage = await ReplyAsync("", false, successEmbed.Build());
				await Task.Delay(2500);
				
				await embedMessage.DeleteAsync();
				await Context.Message.DeleteAsync();
			}
		}
	}
}