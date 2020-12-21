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
        //private readonly ConfigService _config;
        private readonly LoggingService _loggingService;

        private string prefix;

        public CommandHandler(IServiceProvider services) : base(services)
        {
            _commands = services.GetRequiredService<CommandService>();
            
            //_config = services.GetRequiredService<ConfigService>();
            //prefix = _config.BotPrefix;
            prefix = ".";
        }
        
        public async Task InitializeAsync() => _client.MessageReceived += HandleCommandAsync;

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage msg))
                return;

            var context = new SocketCommandContext(_client, msg);

            var argPos = 0;
            if (msg.HasStringPrefix(prefix, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                await _commands.ExecuteAsync(context, argPos, _services);
        }
    }
}
