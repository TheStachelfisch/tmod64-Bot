using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace tMod64Bot.Services
{
    public sealed class CommandHandler : ServiceBase
    {
        private static readonly CommandError IGNORED_ERRORS = CommandError.BadArgCount | CommandError.UnknownCommand | CommandError.UnmetPrecondition | CommandError.ObjectNotFound;

        private readonly CommandService _commands;
        private readonly ConfigService _config;

        private string prefix;

        public CommandHandler(IServiceProvider services) : base(services)
        {
            _client.MessageReceived += HandleCommandAsync;

            _config = services.GetRequiredService<ConfigService>();
            prefix = _config.BotPrefix;
        }


        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage msg))
                return;

            var context = new SocketCommandContext(_client, msg);

            var argPos = 0;
            if (msg.HasStringPrefix(prefix, ref argPos) ||
                msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    Console.WriteLine("Error happened while executing Command: " + result.ErrorReason + " ServerId: " + context.Guild.Id);

                    if ((result.Error & IGNORED_ERRORS) == 0)
                    {
                        var errorEmbed = new EmbedBuilder()
                            .WithTitle("Error Encountered")
                            .WithDescription($"{result.Error}\n[Message Link]({context.Message.GetJumpUrl()})")
                            .WithCurrentTimestamp()
                            .WithColor(Color.Red);

                        await context.Guild.GetTextChannel(_config.AdminChannel).SendMessageAsync(embed: errorEmbed.Build());
                    }
                }
            }
        }
    }
}
