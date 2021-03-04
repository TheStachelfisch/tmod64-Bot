using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Modules.Commons
{
    public class RequireBotManager : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var config = services.GetRequiredService<ConfigService>();

            var user = context.User as SocketGuildUser;

            var result = user.Roles.ToList().Contains(context.Guild.GetRole(config.Config.BotManagerRole)) || user.GuildPermissions.Administrator;

            return Task.FromResult(result ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Missing Bot Manager permissions"));
        }
    }
}