using Discord;
using Discord.WebSocket;
using SupportBot.Handler;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SupportBot
{
    class Program
    {
        private static string _token = File.ReadAllText(@"token.txt");

        private static DiscordSocketClient _client;
        private static DiscordSocketConfig _config;

        private static CommandHandler _commandHandler;

        static void Main(string[] args)
            => StartBotAsync().GetAwaiter().GetResult();


        public static async Task StartBotAsync()
        {
            _client = new DiscordSocketClient();
            _config = new DiscordSocketConfig { MessageCacheSize = 100 };

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
            _commandHandler = new CommandHandler(_client);
        }

        private static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }
}
