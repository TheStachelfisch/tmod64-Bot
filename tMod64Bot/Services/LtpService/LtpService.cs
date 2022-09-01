using System;
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
        throw new NotImplementedException();
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
                            Label = "Special rules for laythrough",
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
						Placeholder = "Paste your modpack here:\n[\n\"Example Mod\"\n]" +
						"\nUsually found in C:\\Users\\YOURUSER\\Documents\\My Games\\Terraria\\ModLoader\\Mods\\ModPacks",
						MinLength = 4
					});
                await component.RespondWithModalAsync(ltpModal.Build());
                break;
            }
        }
    }
}