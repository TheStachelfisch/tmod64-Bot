using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using tMod64Bot.Modules;
using tMod64Bot.Services;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging;
using tMod64Bot.Services.Logging.BotLogging;
using tMod64Bot.Services.Tag;

namespace tMod64Bot
{
    //TODO: Log disconnects from Discord Gateway
    
    internal sealed class tMod64bot
    {
        private static readonly string GatewayToken = File.ReadAllText($@"{ServiceConstants.DATA_DIR}token.txt");
        
        internal static readonly ServiceProvider _services = BuildServiceProvider();
        private static readonly DiscordSocketClient _client = _services.GetRequiredService<DiscordSocketClient>();
        private readonly CommandService _commands = _services.GetRequiredService<CommandService>();
        private readonly LoggingService _log = _services.GetRequiredService<LoggingService>();

        private Stopwatch _startUpStopwatch;
        private bool _shuttingDown;
        
        private static bool _initialized;
        private static long _startUpTime;

        public static long GetUptime => DateTimeOffset.Now.ToUnixTimeSeconds() - _startUpTime;

        public async Task StartAsync()
        {
            _startUpTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            _startUpStopwatch = Stopwatch.StartNew();
            
            if (!Directory.Exists(ServiceConstants.DATA_DIR))
                Directory.CreateDirectory(ServiceConstants.DATA_DIR);
            
            await _client.LoginAsync(TokenType.Bot, GatewayToken);
            await _client.StartAsync();
            _client.Ready += () =>
            {
                InitializeServicesAsync().GetAwaiter().GetResult();
                SetupAsync().GetAwaiter().GetResult();

                //Prevents Program from exiting without disposing services and disconnecting from gateway
                AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                {
                    if (!_shuttingDown)
                        ShutdownAsync().GetAwaiter().GetResult();
                };
                return Task.CompletedTask;
            };

            // HandleCmdLn();

            await Task.Delay(-1);
            
            await ShutdownAsync();
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
        }

        /*private void HandleCmdLn()
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

                cmd = Console.ReadLine();
            }
        }*/

        private async Task ShutdownAsync()
        {
            // Calculating these in the String only returns Int64(Loss of precision)
            decimal minuteUptime = Math.Round((decimal)GetUptime / 60, 2);
            decimal hourUptime = Math.Round(minuteUptime / 60, 2);
            
            _shuttingDown = true;
            
            Stopwatch sw = Stopwatch.StartNew();

            Task tS = Task.Run(() => _client.StopAsync());
            tS.Wait();
            
            await _client.StopAsync();
            
            sw.Stop();
            await _log.Log(LogSeverity.Info, LogSource.Self, $"Successfully Disconnected in {sw.ElapsedMilliseconds}ms");
            await _log.Log(LogSeverity.Info, LogSource.Self, $"Bot uptime was {GetUptime}s or {minuteUptime}min or {hourUptime}h");

            Task tD = Task.Run(() => _services.DisposeAsync());
            tD.Wait();
        }

        private static async Task InitializeServicesAsync()
        {
            if (!_initialized)
            {
                await _services.GetRequiredService<CommandHandler>().InitializeAsync();
                await _services.GetRequiredService<BotLoggingService>().InitializeAsync();
                await _services.GetRequiredService<TotalMemberService>().InitializeAsync();
                await _services.GetRequiredService<StickyRolesHandler>().InitializeAsync();
                await _services.GetRequiredService<ReactionRolesService>().InitializeAsync();
                await _services.GetRequiredService<ModerationService>().InitializeAsync();
                await _services.GetRequiredService<InviteProtectionService>().InitializeAsync();
                await _services.GetRequiredService<BadWordHandler>().InitializeAsync();

                _initialized = true;
            }
        }

        private static ServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
#if DEBUG
                LogLevel = LogSeverity.Verbose,
#else
                LogLevel = LogSeverity.Info,
#endif
                ExclusiveBulkDelete = true,
                UseSystemClock = true,
                DefaultRetryMode = RetryMode.RetryRatelimit,
                AlwaysDownloadUsers = true,
                ConnectionTimeout = 30000,
                MessageCacheSize = 100,
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
            .AddSingleton<InteractiveService>()
            .AddSingleton<TagService>()
            .AddSingleton<ConfigService>()
            .AddSingleton<BadWordHandler>()
            .AddSingleton<ReactionRolesService>()
            .AddSingleton<ModerationService>()
            .AddSingleton<BotLoggingService>()
            .AddSingleton<InviteProtectionService>()
            .AddSingleton<StickyRolesHandler>()
            .AddSingleton<TotalMemberService>()
            .BuildServiceProvider();
    }
}