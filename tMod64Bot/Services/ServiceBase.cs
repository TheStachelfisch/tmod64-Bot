using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using Discord.Addons.Interactive;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Services
{
    public abstract class ServiceBase
    {
        protected readonly IServiceProvider Services;
        protected readonly DiscordSocketClient Client;

        public ServiceBase(IServiceProvider services)
        {
            Services = services;
            Client = services.GetRequiredService<DiscordSocketClient>();
        }
    }
}
