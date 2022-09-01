using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Modules.Commons
{
	internal class DoesNotHaveRole : PreconditionAttribute
	{
		public ulong Role { get; set; }
		public DoesNotHaveRole(ulong role)
		{
			Role = role;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			var config = services.GetRequiredService<ConfigService>();

			var user = context.User as SocketGuildUser;

			var result = !user.Roles.ToList().Contains(context.Guild.GetRole(Role));
			return Task.FromResult(result ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("A role you have denies you from using this command"));
		}
	}
}
