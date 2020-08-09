using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

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
            config = _config;

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
            //TODO: Replace with prefix from Config
            if (msg.HasCharPrefix('.', ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _service);

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
                        await context.Guild.GetTextChannel((ulong)AdminChannelID).SendMessageAsync(MentionUtils.MentionRole(AdminID), false, errorEmbed.Build());
                    }
                }
            }
        }
        //TODO: Delete this
        private ulong AdminID = 741727508306722849;
        private ulong AdminChannelID = 741727264588169332;
    }
}
