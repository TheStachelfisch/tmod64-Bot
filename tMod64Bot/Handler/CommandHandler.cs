using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using tMod64Bot.Modules.ConfigSystem;

namespace tMod64Bot.Handler
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
            _config = config;

            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _service);

            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            EmbedBuilder errorEmbed = new EmbedBuilder();
            
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            var context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            if (msg.HasStringPrefix(ConfigService.GetConfig(ConfigEnum.BotPrefix), ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                IResult result = await _commands.ExecuteAsync(context, argPos, _service);

                //Remove if you want
                if (!result.IsSuccess)
                {
                    Debug.WriteLine("Error happened while executing Command: " + result.ErrorReason + " ServerId: " + context.Guild.Id);
                    if (result.Error != (CommandError.BadArgCount | CommandError.UnknownCommand | CommandError.ObjectNotFound))
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
