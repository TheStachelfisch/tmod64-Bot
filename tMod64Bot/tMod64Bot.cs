using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using tMod64Bot.Services;

namespace tMod64Bot
{
    internal sealed class tMod64bot
    {
        private static readonly ServiceProvider _services = BuildServiceProvider();
        private readonly DiscordSocketClient _client = _services.GetRequiredService<DiscordSocketClient>();
        private readonly CommandService _commands = _services.GetRequiredService<CommandService>();
        private readonly LoggingService _log = _services.GetRequiredService<LoggingService>();

        public async Task StartAsync()
        {
            await _client.LoginAsync(TokenType.Bot, Program.GatewayToken);
            await _client.StartAsync();

            HandleCmdLn();

            await ShutdownAsync();
        }

        private async Task SetupAsync()
        {
            var sw = Stopwatch.StartNew();

            var modules = await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            sw.Stop();

            await _log.Log(LogSeverity.Info, LogSource.Self,
                $"Loaded {modules.Count()} modules and {modules.Sum(m => m.Commands.Count)}" +
                $" commands loaded in {sw.ElapsedMilliseconds}ms.");
        }

        private void HandleCmdLn()
        {
            string cmd = Console.ReadLine();
            while (cmd != "stop")
            {
                var args = cmd.Split(' ');
                int index = 0;
                try
                {
                    do
                    {
                        switch (args[index++])
                        {
                            default:
                                Console.WriteLine($"Unknown command `{cmd}`");
                                break;
                        }
                    }
                    while (index < args.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                cmd = Console.ReadLine();
            }
        }

        private async Task ShutdownAsync()
        {
            await _client.StopAsync();
            _services.Dispose();
            Environment.Exit(0);
        }


        private static ServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
#if DEBUG
                LogLevel = LogSeverity.Verbose,
#else
                LogLevel = LogSeverity.Info,
#endif
                AlwaysDownloadUsers = true,
                ConnectionTimeout = 10000,
                MessageCacheSize = 50
            }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                IgnoreExtraArgs = true,
                DefaultRunMode = RunMode.Sync
            }))

            // base services
            .AddSingleton<ConfigService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<InviteBlockerService>()

            .BuildServiceProvider();
    }
}