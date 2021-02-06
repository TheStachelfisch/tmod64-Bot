using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using tMod64Bot.Services;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging;
using tMod64Bot.Services.Logging.UserLogging;

namespace tMod64Bot
{
    //TODO: Log disconnects from Discord Gateway
    
    internal sealed class tMod64bot
    {
        public static readonly string GatewayToken = File.ReadAllText(@"token.txt");
        
        internal static readonly ServiceProvider _services = BuildServiceProvider();
        private readonly DiscordSocketClient _client = _services.GetRequiredService<DiscordSocketClient>();
        private readonly CommandService _commands = _services.GetRequiredService<CommandService>();
        private readonly LoggingService _log = _services.GetRequiredService<LoggingService>();

        private bool _shuttingDown;

        public async Task StartAsync()
        {
            if (!Directory.Exists(ServiceConstants.DATA_DIR))
                Directory.CreateDirectory(ServiceConstants.DATA_DIR);
            
            await _client.LoginAsync(TokenType.Bot, GatewayToken);
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

            await _log.Log($"Loaded {modules.Count()} modules and {modules.Sum(m => m.Commands.Count)}" +
                           $" commands in {sw.ElapsedMilliseconds}ms.");
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
                            case "stop":
                                ShutdownAsync().GetAwaiter();
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
            
            Stopwatch sw = Stopwatch.StartNew();
            
            await _client.StopAsync();
            sw.Stop();
            await _log.Log(LogSeverity.Verbose, LogSource.Self, $"Successfully Disconnected in {sw.ElapsedMilliseconds}ms");

            await _services.DisposeAsync();

            Environment.Exit(0);
        }

        private static async Task InitializeServicesAsync()
        {
            await _services.GetRequiredService<CommandHandler>().InitializeAsync();
            await _services.GetRequiredService<UserLoggingService>().InitializeAsync();
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
                MessageCacheSize = 100
            }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                //IgnoreExtraArgs = true,
                DefaultRunMode = RunMode.Async
            }))

            // base services
            .AddSingleton<CommandHandler>()
            .AddSingleton<LoggingService>()
            .AddSingleton<ConfigService>()
            .AddSingleton<UserLoggingService>()
            .BuildServiceProvider();
    }
}