using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services;
using tMod64Bot.Services.Logging;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    public class ModerationModule : CommandBase
    {
        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(SocketGuildUser user, [Remainder]string reason = "No Reason provided")
        {
            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("Look at this looser trying to ban himself");
                return;
            }

            if (user.Id == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("Get Kanyed");
                return;
            }
            
            try
            {
                await Services.GetRequiredService<ModerationService>().BanUser((SocketGuildUser) Context.User, user, reason);
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error occured while banning", e);
                return;
            }
            
        }

        [Command("mute")]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task MuteAsync(SocketGuildUser user, string? duration = null, [Remainder] string reason = "No Reason provided")
        {
            var span = duration == null ? TimeSpan.Zero : ModerationService.GetTimeSpan(duration);
            
            if (user.Id == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("Please don't. This already broke Carl bot");
                return;
            }

            if (user.Id == Context.User.Id)
            {
                await ReplyAsync("Look at this looser trying to mute himself");
                return;
            }
            
            try
            {
                var service = Services.GetRequiredService<ModerationService>();

                await service.MuteUser(span, (SocketGuildUser) Context.User, user, reason);
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error occured while muting", e);
                return;
            }
            
            await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully muted {MentionUtils.MentionUser(user.Id)} {(span == TimeSpan.Zero ? "indefinitely" : $"for {span.TotalHours}h")}"));
        }
    }
}