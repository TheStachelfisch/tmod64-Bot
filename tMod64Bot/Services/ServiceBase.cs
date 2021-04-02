using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace tMod64Bot.Services
{
    public abstract class ServiceBase
    {
        protected readonly IServiceProvider Services;
        protected readonly DiscordSocketClient Client;

        protected ServiceBase(IServiceProvider services)
        {
            Services = services;
            Client = services.GetRequiredService<DiscordSocketClient>();
        }
    }
}
