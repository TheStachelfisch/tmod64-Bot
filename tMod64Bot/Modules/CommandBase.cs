using System;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging;

namespace tMod64Bot.Modules
{
    public abstract class CommandBase : ModuleBase<SocketCommandContext>
    {
        protected readonly IServiceProvider Services = tMod64bot._services;
        protected readonly ConfigService ConfigService = tMod64bot._services.GetRequiredService<ConfigService>();
        protected readonly LoggingService LoggingService = tMod64bot._services.GetRequiredService<LoggingService>();
    }
}