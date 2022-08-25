using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using tMod64Bot.Modules;
using tMod64Bot.Services;
using tMod64Bot.Services.Commons;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging;
using tMod64Bot.Services.Logging.BotLogging;
using tMod64Bot.Services.Tag;
using tMod64Bot.Utils;

#pragma warning disable 8618

namespace tMod64Bot
{
    // ReSharper disable once InconsistentNaming
    internal sealed class tMod64bot
    {
        private static CancellationTokenSource _stopTokenSource = new ();
        private CancellationToken _stopToken = _stopTokenSource.Token;
        
        private static readonly string? GatewayToken = File.Exists($@"{ServiceConstants.DATA_DIR}token.txt") ? File.ReadAllText($@"{ServiceConstants.DATA_DIR}token.txt") : Environment.GetEnvironmentVariable("TOKEN");

        private static IServiceCollection _serviceCollection;
        
        internal static readonly ServiceProvider _services = BuildServiceProvider();
        private static readonly DiscordSocketClient _client = _services.GetRequiredService<DiscordSocketClient>();
        private readonly CommandService _commands = _services.GetRequiredService<CommandService>();
        private readonly LoggingService _log = _services.GetRequiredService<LoggingService>();

        private Stopwatch _startUpStopwatch;
        private bool _shuttingDown;
        
        public static TimeSpan GetUptime => DateTimeOffset.Now - Process.GetCurrentProcess().StartTime;

        public async Task StartAsync()
        {
            _startUpStopwatch = Stopwatch.StartNew();
            
            if (!Directory.Exists(ServiceConstants.DATA_DIR))
                Directory.CreateDirectory(ServiceConstants.DATA_DIR);
            
            await _client.LoginAsync(TokenType.Bot, GatewayToken);
            await _client.StartAsync();
            
            Task ReadyEvent()
            {
                _client.Ready -= ReadyEvent;
                
                InitializeServicesAsync().GetAwaiter().GetResult();
                SetupAsync().GetAwaiter().GetResult();

                return Task.CompletedTask;
            }

            _client.Ready += ReadyEvent;
            
            new Thread(HandleCmdLn).Start();
            
            //Prevents Program from exiting without disposing services and disconnecting from gateway
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                if (!_shuttingDown)
                    ShutdownAsync().GetAwaiter().GetResult();
            };
            
            try { await Task.Delay(-1, _stopToken); }
            catch (TaskCanceledException e) { }
        }

        private async Task SetupAsync()
        {
            var sw = Stopwatch.StartNew();

            var modules = await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            sw.Stop();
            _startUpStopwatch.Stop();

            await _log.Log($"Loaded {modules.Count()} modules and {modules.Sum(m => m.Commands.Count)}" +
                           $" commands in {sw.ElapsedMilliseconds}ms.");
            await _log.Log($"Completed Startup in {_startUpStopwatch.ElapsedMilliseconds}ms");

            new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = true
            }.Elapsed += async (_, _) => { await _client.SetActivityAsync(new MemberActivity(_client)); };
        }

        private void HandleCmdLn()
        {
            var cmd = "";
            
            while (!_stopToken.IsCancellationRequested)
            {
                cmd = Console.In.ReadLineAsync().GetAwaiter().GetResult();
                
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
                                ShutdownAsync().GetAwaiter().GetResult();
                                break;
                            case "commands":
                            {
                                var modules = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                                    .Where(x => typeof(CommandBase).IsAssignableFrom(x));

                                string commandString = "";
            
                                foreach (var module in modules)
                                {
                                    commandString += $"{module.Name}{(module.GetCustomAttribute<PreconditionAttribute>() != null ? $" -- {module.GetCustomAttribute<PreconditionAttribute>()!.GetType().Name}" : "")}\n";
                
                                    var commands = module.GetMethods().Where(x =>
                                        x.CustomAttributes.Any(x => x.AttributeType == typeof(CommandAttribute)));

                                    foreach (var command in commands)
                                    {
                                        var parameter = command.GetParameters();
                    
                                        commandString += $"   {command.GetCustomAttribute<CommandAttribute>()!.Text}{(parameter.Length != 0 ? $" ({String.Join(',', parameter.Select(x => x.ToString()))})" : "")}\n";

                                        var aliasAttributes = command.GetCustomAttribute<AliasAttribute>();

                                        if (aliasAttributes != null)
                                            commandString += $"     - Alias {String.Join(',', aliasAttributes.Aliases).PadLeft(36)}\n";

                                        var permissionsAttribute = String.Join(',', command.GetCustomAttributes<RequireUserPermissionAttribute>().Select(x => x.GuildPermission));
                    
                                        commandString += $"     - Permissions {(permissionsAttribute.IsNullOrWhitespace() ? "None" : permissionsAttribute).PadLeft(30)}\n\n";
                                    }

                                    commandString += "\n";
                                }

                                Console.WriteLine(commandString);
                                
                                File.WriteAllText($"{ServiceConstants.DATA_DIR}/Commands.txt", commandString);
                                
                                break;
                            };
                        }
                    } while (index < args.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async Task ShutdownAsync()
        {
            _stopTokenSource.Cancel();

            _shuttingDown = true;
            
            Stopwatch sw = Stopwatch.StartNew();

            Task tS = Task.Run(() => _client.StopAsync());
            tS.Wait();
            
            await _client.StopAsync();
            
            sw.Stop();
            await _log.Log(LogSeverity.Info, LogSource.Self, $"Successfully Disconnected in {sw.ElapsedMilliseconds}ms");
            await _log.Log(LogSeverity.Info, LogSource.Self, $"Bot uptime was {GetUptime.FormatString(false)}");

            Task tD = Task.Run(() => _services.DisposeAsync());
            tD.Wait();
        }

        private static async Task InitializeServicesAsync()
        {
            foreach (var initializeable in _serviceCollection.Where(x => typeof(IInitializeable).IsAssignableFrom(x.ServiceType)))
                foreach (var service in _services.GetServices(initializeable.ServiceType))
                    await (service as IInitializeable)!.Initialize();
        }

        private static ServiceProvider BuildServiceProvider()
        {
            _serviceCollection = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
#if DEBUG
                    LogLevel = LogSeverity.Verbose,
#else
                    LogLevel = LogSeverity.Info,
#endif
                    UseSystemClock = true,
                    GatewayIntents = GatewayIntents.All,
                    DefaultRetryMode = RetryMode.RetryRatelimit,
                    AlwaysDownloadUsers = true,
                    ConnectionTimeout = 30000,
                    MessageCacheSize = 100
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    //IgnoreExtraArgs = true,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton(new MemoryCache("GlobalCache"))

                // base services
                .AddSingleton<CommandHandler>()
                .AddSingleton<LoggingService>()
                .AddSingleton<WebhookService>()
                .AddSingleton<TagService>()
                .AddSingleton<ConfigService>()
                .AddSingleton<BadWordHandler>()
                .AddSingleton<ReactionRolesService>()
                .AddSingleton<ModerationService>()
                .AddSingleton<BotLoggingService>()
                .AddSingleton<InviteProtectionService>()
                .AddSingleton<StickyRolesHandler>()
                .AddSingleton<WelcomeService>()
                .AddSingleton<LogService>();

            return _serviceCollection.BuildServiceProvider();
        }
    }
}