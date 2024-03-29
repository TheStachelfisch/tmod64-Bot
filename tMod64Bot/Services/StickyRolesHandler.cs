﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Commons;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Services
{
    public class StickyRolesHandler : ServiceBase, IInitializeable
    {
        private readonly ConfigService _configService;
        private readonly WebhookService _webhook;
        
        public StickyRolesHandler(IServiceProvider services) : base(services)
        {
            _configService = services.GetRequiredService<ConfigService>();
            _webhook = services.GetRequiredService<WebhookService>();
        }

        public async Task Initialize()
        {
            Client.UserLeft += HandlerUserLeft;
            Client.UserJoined += HandleUserJoined;
        }

        private async Task HandleUserJoined(SocketGuildUser user)
        {
            if (user.IsBot || !_configService.Config.StickiedUsers.ContainsKey(user.Id))
                return;

            await user.AddRolesAsync(_configService.Config.StickiedUsers[user.Id]);
        }

        private async Task HandlerUserLeft(SocketGuild socketGuild, SocketUser socketUser)
        {
            var user = socketUser as SocketGuildUser;
            
            if (user == null || user.IsBot || !user.Roles.Any())
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