using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SupportBot.Handler
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandServiceConfig _config;
        private IServiceProvider _service;
        public static CommandService _commands;

        public CommandHandler(DiscordSocketClient client, CommandServiceConfig config = null)
        {
            _commands = new CommandService();
            _client = client;

            //Check if null before doing anything with it
            config = _config;

            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _service);

            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            var context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            //TODO: Replace with prefix from Config
            if (msg.HasCharPrefix('.', ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _service);

                //Remove if you want
                if (!result.IsSuccess)
                {
                    Console.WriteLine("Error happened while executing Command: " + result.ErrorReason + " ServerId: " + context.Guild.Id);
                    if (result.Error != (CommandError.BadArgCount | CommandError.UnknownCommand | CommandError.ObjectNotFound))
					{
                        await context.Guild.GetTextChannel((ulong)AdminChannelID).SendMessageAsync("An error was encountered while command executing:" + result.Error + " " + MentionUtils.MentionRole((ulong)AdminID));
                    }
                }
            }
        }
        private long AdminID = 741727508306722849; //DELETE THIS
        private long AdminChannelID = 741727264588169332;
    }
}
