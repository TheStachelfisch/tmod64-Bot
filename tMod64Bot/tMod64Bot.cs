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

        private static bool _shuttingDown = false;

        public async Task StartAsync()
        {
            await _client.LoginAsync(TokenType.Bot, Program.GatewayToken);
            await _client.StartAsync();
            _client.Ready += () =>
            {
                InitializeServicesAsync().GetAwaiter().GetResult();
                SetupAsync().GetAwaiter().GetResult();
                
                //Prevents Program from exiting without disposing services and disconnecting from gateway
                AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
                {
                    if (!_shuttingDown)
                        ShutdownAsync().GetAwaiter().GetResult();
                }; 
                return Task.CompletedTask;
            };
            
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
            var cmd = Console.ReadLine();
            while (cmd != "stop")
            {
                var args = cmd.Split(' ');
                var index = 0;
                try
                {
                    do
                    {
                        switch (args[index++])
                        {
                            default:
                                Console.WriteLine($"Unknown command `{cmd}`");
                                break;
                            case "clear":
                                Console.Clear();
                                break;
                        }
                    } while (index < args.Length);
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
            _shuttingDown = true;
            
            await _log.Log(LogSeverity.Info, LogSource.Self, "Shutdown requested by Command");
            await _client.StopAsync();
            await _services.DisposeAsync();

            Environment.Exit(0);
        }

        private static async Task InitializeServicesAsync()
        {
            await _services.GetRequiredService<CensorshipService>().InitializeAsync();
            await _services.GetRequiredService<CommandHandler>().InitializeAsync();
            await _services.GetRequiredService<InviteBlockerService>().InitializeAsync();
        }
        
        private static ServiceProvider BuildServiceProvider() => new ServiceCollection().AddSingleton(
                new DiscordSocketClient(new DiscordSocketConfig
                {
#if DEBUG
                    LogLevel = LogSeverity.Verbose,
#else
                    LogLevel = LogSeverity.Info,
#endif
                    AlwaysDownloadUsers = true,
                    ConnectionTimeout = 10000,
                    MessageCacheSize = 100
                }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                //IgnoreExtraArgs = true,
                DefaultRunMode = RunMode.Async
            }))

            // base services
            .AddSingleton<ConfigService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<InviteBlockerService>()
            .AddSingleton<LoggingService>()
            .AddSingleton<CensorshipService>()
            .BuildServiceProvider();
    }
}