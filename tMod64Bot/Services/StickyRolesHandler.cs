using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Services
{
    public class StickyRolesHandler : ServiceBase
    {
        private readonly ConfigService _configService;
        private readonly WebhookService _webhook;
        
        public StickyRolesHandler(IServiceProvider services) : base(services)
        {
            _configService = services.GetRequiredService<ConfigService>();
            _webhook = services.GetRequiredService<WebhookService>();
        }

        public async Task InitializeAsync()
        {
            Client.UserLeft += HandlerUserLeft;
            Client.UserJoined += HandleUserJoined;
        }

        private async Task HandleUserJoined(SocketGuildUser user)
        {
            if (user.IsBot || !_configService.Config.StickiedUsers.ContainsKey(user.Id))
                return;

            List<IRole> roles = new();

            foreach (var stickiedUsersValue in _configService.Config.StickiedUsers.Values)
            {
                foreach (var id in stickiedUsersValue)
                {
                    try
                    {
                        roles.Add(user.Guild.GetRole(id));
                    }
                    catch (ArgumentNullException e)
                    {
                        /* Ignore */
                    }
                }
            } 
            
            await user.AddRolesAsync(roles);
        }

        private async Task HandlerUserLeft(SocketGuildUser user)
        {
            if (user.IsBot || !user.Roles.Any())
                return;
            
            var roles = user.Roles.Select(x => x.Id).Where(p => _configService.Config.StickiedRoles.Any(p2 => p2 == p));

            var enumerable = roles.ToList();
            if (!enumerable.Any())
                return;

            if (_configService.Config.StickiedUsers.ContainsKey(user.Id))
            {
                if (_configService.Config.StickiedUsers.ContainsValue(enumerable.ToList()))
                    return;
                
                _configService.Config.StickiedUsers[user.Id] = enumerable;
                _configService.SaveData();
            }
            else
            {
                _configService.Config.StickiedUsers.Add(user.Id, enumerable.ToList());
                _configService.SaveData();
            }
        }
    }
}