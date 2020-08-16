using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using tMod64Bot.Modules.ConfigSystem;

namespace tMod64Bot.Handler
{
    public class CommandHandler
    {
        public static CommandService _commands;
        private readonly DiscordSocketClient _client;
        private CommandServiceConfig _config;
        private IServiceProvider _service;

        public CommandHandler(DiscordSocketClient client, CommandServiceConfig config)
        {
            _client = client;
            _config = config;
            
            _config.DefaultRunMode = RunMode.Async;
            
            _commands = new CommandService(_config);
            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _service);

            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var errorEmbed = new EmbedBuilder();

            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            var context = new SocketCommandContext(_client, msg);

            var argPos = 0;
            if (msg.HasStringPrefix(ConfigService.GetConfig(ConfigEnum.BotPrefix), ref argPos) ||
                msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _service);
                
                if (!result.IsSuccess)
                {
                    Console.WriteLine("Error happened while executing Command: " + result.ErrorReason + " ServerId: " + context.Guild.Id);
                    // Plant, your shit didn't work, mine works
                    if (result.Error != CommandError.BadArgCount && result.Error != CommandError.UnknownCommand && result.Error != CommandError.UnmetPrecondition && result.Error != CommandError.ObjectNotFound)
                    {
                        errorEmbed.WithTitle("Error Encountered");
                        errorEmbed.WithDescription($"{result.Error.ToString()}\n\n[Message Link]({context.Message.GetJumpUrl()})");
                        errorEmbed.WithCurrentTimestamp();
                        errorEmbed.WithColor(Color.Red);
                        await context.Guild.GetTextChannel(ulong.Parse(ConfigService.GetConfig(ConfigEnum.AdminChannel))).SendMessageAsync(MentionUtils.MentionRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.AdminRole))), false, errorEmbed.Build());
                    }
                }
            }
        }
    }
}