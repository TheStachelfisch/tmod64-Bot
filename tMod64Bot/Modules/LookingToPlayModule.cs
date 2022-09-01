using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using tMod64Bot.Modules.Commons;
using tMod64Bot.Services.Config;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
	[DoesNotHaveRole(1012166628374495262)]
	internal class LookingToPlayModule : CommandBase
	{
		[Command("lookingtoplay"), Alias("play", "ltp")]
		public async Task LookingToPlay()
		{
			var mb = new ModalBuilder()
				.WithTitle("Add a Looking to Play request")
				.WithCustomId("looking_menu")
				.AddTextInput("Mod list in Json format", "modlist", TextInputStyle.Paragraph,
					"[\r\n  \"ExampleMod\"\r\n]" +
					"\nCreate a modpack using tModLoader in the Mods section, then locate the modpack.json file and paste the contents." +
					"\nUsually found in C:\\Users\\YOURUSER\\Documents\\My Games\\Terraria\\ModLoader\\Mods\\ModPacks")
				.AddTextInput("tModLoader version", "version", placeholder: "1.3 - 0.11.8.9 64 Bit")
				.AddTextInput("Additional Players", "addplayers", placeholder: "1");

				await Context.Interaction.RespondWithModalAsync(mb.Build());
			
		}
	}
}
