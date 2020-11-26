using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using tMod64Bot.Services;

namespace tMod64Bot.Modules.Commons
{
    public sealed class BotManagementPerm : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var config = services.GetRequiredService<ConfigService>();

            var user = context.User as SocketGuildUser;

            var result = user.Roles.Contains(config.ManagerRole) || user.GuildPermissions.Administrator;

            return Task.FromResult(result ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Missing Bot Manager permissions"));
        }
    }
}
