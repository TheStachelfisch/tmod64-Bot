using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using tMod64Bot.Handler;

namespace tMod64Bot
{
    internal class tMod64bot
    {
        private static readonly string _token = File.ReadAllText(@"token.txt");

        private static DiscordSocketClient _client;
        private static DiscordSocketConfig _config;

        private static InviteHandler _inviteHandler;
        private static CommandHandler _commandHandler;
        private static BadWordHandler _badWordHandler;

        private static void Main(string[] args)
            => StartBotAsync().GetAwaiter().GetResult();
        

        public static async Task StartBotAsync()
        {
            _client = new DiscordSocketClient();
            _config = new DiscordSocketConfig {MessageCacheSize = 100};

            await _client.LoginAsync(TokenType.Bot, _token);

            await _client.StartAsync();
            await _client.SetStatusAsync(UserStatus.Online);
            await _client.SetGameAsync(".help");

            _client.Log += Log;
            _client.Ready += ReadyEvent;

            await Task.Delay(-1);
        }

        //Add Handlers here
        private static async Task ReadyEvent()
        {
            await _client.SetStatusAsync(UserStatus.Online);
            _badWordHandler = new BadWordHandler(_client);
            _commandHandler = new CommandHandler(_client);
            _inviteHandler = new InviteHandler(_client);
        }

        private static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }
}