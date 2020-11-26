using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace tMod64Bot.Services
{
    public abstract class ServiceBase
    {
        protected readonly IServiceProvider _services;
        protected readonly DiscordSocketClient _client;

        public ServiceBase(IServiceProvider services)
        {
            _services = services;
            _client = services.GetRequiredService<DiscordSocketClient>();
        }
    }
}
