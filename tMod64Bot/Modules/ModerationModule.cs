using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services;
using tMod64Bot.Services.Logging;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    // TODO: Add hierarchy checking. IMPORTANT!!!
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
                await ReplyAsync("Don't try to ban me");
                return;
            }
            
            try
            {
                var success = await Services.GetRequiredService<ModerationService>().BanUser((SocketGuildUser) Context.User, user, reason);

                if (success.IsSuccess)
                    await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"{user} has been successfully banned"));
                else
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed(success.ErrorReason!));
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error occured while banning", e);
            }
        }

        [Command("unban"), Alias("Pardon")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task UnbanAsync(ulong userId)
        {
            if (Context.User.Id == userId)
            {
                await ReplyAsync("Look at this looser trying to unban himself");
                return;
            }

            if (userId == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("Don't know how this is gonna work out");
                return;
            }
            
            try
            {
                var success = await Services.GetRequiredService<ModerationService>().UnbanUser(userId, Context.Guild, (SocketGuildUser)Context.User);

                if (success.IsSuccess)
                    await ReplyAsync(embed: EmbedHelper.SuccessEmbed("User has been successfully unbanned"));
                else
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed(success.ErrorReason!));
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error occured while unbanning", e);
            }
        }
        

        [Command("tempban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task TempBanAsync(SocketGuildUser user, string? banTime = null, [Remainder]string reason = "No Reason provided")
        {
            var span = banTime == null ? TimeSpan.Zero : ModerationService.GetTimeSpan(banTime);

            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("Look at this looser trying to ban himself");
                return;
            }

            if (user.Id == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("Don't try to ban me");
                return;
            }

            try
            {
                var success = await Services.GetRequiredService<ModerationService>().TempBanUser(user, (SocketGuildUser)Context.User, span, reason);

                if (success.IsSuccess)
                    await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"{user} has been successfully banned"));
                else
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed(success.ErrorReason!));
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error occured while banning", e);
            }
        }
        
        [Command("mute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
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

                var success = await service.MuteUser(user, (SocketGuildUser) Context.User, span,  reason);

                if (success.IsSuccess)
                    await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully muted {MentionUtils.MentionUser(user.Id)} {(span == TimeSpan.Zero ? "indefinitely" : $"for {span.TotalHours}h")}"));
                else
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed(success.ErrorReason!));
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error occured while muting", e);
            }
        }

        [Command("unmute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task UnMuteAsync(SocketGuildUser user)
        {
            if (user.Id == Context.User.Id)
            {
                await ReplyAsync("Look at this looser trying to unmute himself");
                return;
            }

            try
            {
                var service = Services.GetRequiredService<ModerationService>();

                var success = await service.UnMuteUser(user, (SocketGuildUser) Context.User);

                if (success.IsSuccess)
                    await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully un-muted {user}"));
                else
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed(success.ErrorReason!));
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error occured while un-muting", e);
            }
        }

        [Command("kick"), Alias("boot")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickUserAsync(SocketGuildUser user, [Remainder] string reason = "No reason provided")
        {
            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("Look at this looser trying to kick himself");
                return;
            }

            if (user.Id == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("Don't try to kick me");
                return;
            }
            
            try
            {
                var success = await Services.GetRequiredService<ModerationService>().KickUser(user, (SocketGuildUser)Context.User, reason);

                if (success.IsSuccess)
                    await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"{user} has been successfully kicked"));
                else
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed(success.ErrorReason!));
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error occured while kicking", e);
            }
        }
    }
}