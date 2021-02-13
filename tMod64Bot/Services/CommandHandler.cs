using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging;
using tMod64Bot.Services.Tag;
using tMod64Bot.Utils;

namespace tMod64Bot.Services
{
    public sealed class CommandHandler : ServiceBase
    {
        private static readonly CommandError IGNORED_ERRORS = CommandError.BadArgCount | CommandError.UnknownCommand | CommandError.UnmetPrecondition | CommandError.ObjectNotFound;

        private readonly CommandService _commands;
        private readonly ConfigService _config;
        private readonly LoggingService _loggingService;

        public CommandHandler(IServiceProvider services) : base(services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _loggingService = services.GetRequiredService<LoggingService>();
            _config = services.GetRequiredService<ConfigService>();
        }

        public async Task InitializeAsync()
        {
            Client.MessageReceived += HandleCommandAsync;

#if DEBUG
            _commands.CommandExecuted += CommandExecutedAsync;      
#endif
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified || result.IsSuccess)
                return;

            var error = EmbedHelper.ErrorEmbed(result.ToString()!);

            await context.Channel.SendMessageAsync(embed:error);
#if DEBUG
            _loggingService.Log(LogSeverity.Error, LogSource.Service, $"Error in command Execution");
#endif
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage msg))
                return;

            var context = new SocketCommandContext(Client, msg);

            var argPos = 0;
            if (msg.HasStringPrefix(_config.Config.BotPrefix, ref argPos) || msg.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, Services);

                if (result.Error == CommandError.UnknownCommand && !arg.Content.Remove(0, _config.Config.BotPrefix.Length).Contains(' '))
                {
                    var tagName = arg.Content.Remove(0, _config.Config.BotPrefix.Length).ToLower();
                    
                    TagService tagService = Services.GetRequiredService<TagService>();
                    Stopwatch sw = Stopwatch.StartNew();

                    Config.Tag tagResult;
                    
                    try { tagResult = await tagService.GetTag(tagName); }
                    catch (Exception e)
                    {
                        await context.Channel.SendMessageAsync(embed:EmbedHelper.ErrorEmbed($"Tag '{tagName}' doesn't exist"));
                        return;
                    }
            
                    sw.Stop();
                    await _loggingService.Log($"Got tag in {sw.ElapsedTicks}ms");
            
                    await context.Channel.SendMessageAsync($"**Tag: {tagName}**\n{tagResult.Content}");
                }
            }
        }
    }
}
