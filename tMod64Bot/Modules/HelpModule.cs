using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using tMod64Bot.Handler;

namespace tMod64Bot.Modules
{
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		[Command("help"), Alias("h")]
		[Summary("Help command with all commands")]
		public async Task Help()
		{
			List<CommandInfo> commands = CommandHandler._commands.Commands.ToList();
			EmbedBuilder embedBuilder = new EmbedBuilder();

			foreach (CommandInfo command in commands)
			{
				foreach (var aliases in command.Aliases.Where(x => !x.Equals(command.Name)))
				{
					//Dont add more than one Alias, that breaks stuff
					string embedFieldText = command.Summary ?? "No description available";

					embedBuilder.AddField($"{command.Name}/{aliases}", $"{embedFieldText}");
				}
			}
			
			await ReplyAsync("", false, embedBuilder.Build());
		}
	}
}