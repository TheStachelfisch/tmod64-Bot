using System;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Commons;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Services
{
    public class WelcomeService : ServiceBase, IInitializeable
    {
        private readonly ConfigService ConfigService;
        
        public WelcomeService(IServiceProvider services) : base(services)
        {
            ConfigService = services.GetRequiredService<ConfigService>();
        }

        public async Task Initialize()
        {
            Client.UserJoined += HandleJoined;
        }

        private async Task HandleJoined(SocketGuildUser user)
        {
            if (ConfigService.Config.WelcomeMessage.IsNullOrWhitespace() || user.IsBot)
                return;

            try { await user.SendMessageAsync(ConfigService.Config.WelcomeMessage); }
            catch { /* Ignore */ }
        }
    }
}