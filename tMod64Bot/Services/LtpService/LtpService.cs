using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Commons;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging;

namespace tMod64Bot.Services.LtpService;

public class LtpService : ServiceBase, IInitializeable
{
    private readonly LoggingService _loggingService;
    private readonly ConfigService _config;

    public LtpService(IServiceProvider services) : base(services)
    {
        _loggingService = services.GetRequiredService<LoggingService>();
        _config = services.GetRequiredService<ConfigService>();
    }

    public async Task Initialize()
    {
        Client.ButtonExecuted += HandleModalRequest;
        Client.ModalSubmitted += HandleModalSubmit;
    }

    private Task HandleModalSubmit(SocketModal modal)
    {
        Embed embed;
        string modlist;
		switch (modal.Data.CustomId)
        {
            case "looking-to-play-modal":
                {
					List<SocketMessageComponentData> components =
		            modal.Data.Components.ToList();

                    string version = components
                        .First(x => x.CustomId == "version").Value;
					string description = components
						.First(x => x.CustomId == "description").Value;
					string worldsettings = components
						.First(x => x.CustomId == "world_settings").Value;
					string additionalplayers = components
						.First(x => x.CustomId == "additional_players").Value;
					modlist = components
						.First(x => x.CustomId == "mod_list").Value;

					ulong channel = _config.Config.LookingToPlayChannel;

                    var requestembed = new EmbedBuilder();
                    requestembed.Author.WithName(modal.User.Mention);
                    requestembed.WithTitle("tModLoader Version");
					requestembed.WithDescription(version);

                    var worldfield = new EmbedFieldBuilder();
                    worldfield.Name = "World Settings";
                    worldfield.Value = worldsettings;
					requestembed.Fields.Add(worldfield);

					var descriptionfield = new EmbedFieldBuilder();
                    descriptionfield.Name = "Additional Rules";
                    descriptionfield.Value = description;
                    requestembed.Fields.Add(descriptionfield);

					requestembed.WithFooter("Looking for " + additionalplayers + " players!");
					requestembed.WithCurrentTimestamp();
                    embed = requestembed.Build();
				}
				break;
            default:
                embed = null;
                break;

		}
		return modal.RespondAsync(embed: embed, components: new ComponentBuilder().WithButton("Create request", "looking-to-play-request").Build());
		//await modal.RespondWithFileAsync(); TODO: create and send modlist file
	}

	private async Task HandleModalRequest(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "looking-to-play-request":
                {
                    var ltpModal = new ModalBuilder().WithTitle("Add a looking to play Request")
                        .WithCustomId("looking-to-play-modal")
                        .WithCustomId("looking_menu")
                        .AddTextInput(new TextInputBuilder
                        {
                            Label = "Special rules for playthrough",
                            CustomId = "description",
                            Required = false,
                            Placeholder = "One of each class, fighting until moon lord and no further.",
                            Style = TextInputStyle.Paragraph,
                        })
                    .AddTextInput(new TextInputBuilder
                    {
                        Label = "Version of the game",
                        CustomId = "version",
                        Required = true,
                        Placeholder = "vanilla 1.3, vanilla 1.4, tMl 1.3, tMl 1.4",
                        Style = TextInputStyle.Short,
                        MinLength = 7,
                        MaxLength = 20
                    })
                    .AddTextInput(new TextInputBuilder
                    {
                        Label = "World Settings",
                        CustomId = "world_settings",
                        Required = false,
                        Style = TextInputStyle.Paragraph,
                        Placeholder = "Small, Expert, Seed 2135342364"
                    })
                    .AddTextInput(new TextInputBuilder
                    {
                        Label = "Additional Players",
                        CustomId = "additional_players",
                        Required = false,
                        Placeholder = "2",
                        Style = TextInputStyle.Short,
                        MaxLength = 2,
                        MinLength = 1
                    })
					.AddTextInput(new TextInputBuilder
					{
						Label = "Mod list in Json format",
						CustomId = "mod_list",
						Required = true,
						Style = TextInputStyle.Paragraph,
						Placeholder = "[\n\"Example Mod\"\n]", //TODO: find a way to show where to find the modpacks folder
						MinLength = 4
					});
                await component.RespondWithModalAsync(ltpModal.Build());
                break;
            }
        }
    }
}