﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Modules.Commons;
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
        public async Task BanAsync([RequiresHierarchy]IUser user, [Remainder]string reason = "No Reason provided")
        {
            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("Look at this loser trying to ban himself");
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

        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(ulong userId, [Remainder] string reason = "No Reason provided")
            => await BanAsync(Context.Client.Rest.GetUserAsync(userId).Result, reason);

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
        public async Task TempBanAsync([RequiresHierarchy]IUser user, string? banTime = null, [Remainder]string reason = "No Reason provided")
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

            if (span == TimeSpan.Zero)
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed("Missing tempban time"));
                return;
            }
            
            try
            {
                var success = await Services.GetRequiredService<ModerationService>().TempBanUser(user, (SocketGuildUser)Context.User, span, reason);

                if (success.IsSuccess)
                    await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"{user} has been successfully temp-banned for {span.FormatString(true)}"));
                else
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed(success.ErrorReason!));
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error occured while banning", e);
            }
        }

        [Command("tempban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task TempBanAsync(ulong userId, string? banTime = null, [Remainder] string reason = "No Reason provided")
            => await TempBanAsync(Context.Client.Rest.GetUserAsync(userId).Result, banTime, reason);
        
        [Command("mute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync([RequiresHierarchy]SocketGuildUser user, string? duration, [Remainder] string reason = "No Reason provided")
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
                    await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully muted {MentionUtils.MentionUser(user.Id)} {(span == TimeSpan.Zero ? "indefinitely" : $"for {span.FormatString(true)}")}"));
                else
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed(success.ErrorReason!));
            }
            catch (Exception e)
            {
                await LoggingService.Log(LogSeverity.Error, LogSource.Module, "Error occured while muting", e);
            }
        }

        [Command("mute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Priority(5)]
        public async Task MuteAsync([RequiresHierarchy] SocketGuildUser user, [Remainder] string reason = "No Reason provided")
        {
            try
            {
                ModerationService.GetTimeSpan(reason.Split(' ')[0]);
            }
            catch (Exception e)
            {
                await MuteAsync(user, null, reason);
                return;
            }

            await MuteAsync(user, reason.Split(' ')[0], reason.Substring(1));
        }

        [Command("unmute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task UnMuteAsync([RequiresHierarchy]SocketGuildUser user)
        {
            if (user.Id == Context.User.Id)
            {
                await ReplyAsync("Look at this loser trying to unmute himself");
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
        public async Task KickUserAsync([RequiresHierarchy]SocketGuildUser user, [Remainder] string reason = "No reason provided")
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

        [Command("purge")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeChannel(ulong amount)
        {
            amount += 1;
            var channel = Context.Channel as ITextChannel;
            
            var messages = await channel!.GetMessagesAsync(Convert.ToInt32(amount)).FlattenAsync();
            var filteredMessages = messages.Where(x => x.Timestamp < DateTimeOffset.Now.AddDays(14)).ToList();

            if (!filteredMessages.Any())
            {
                await ReplyAsync("Nothing to delete");
            }
            else
            {
                await channel.DeleteMessagesAsync(filteredMessages);
                await ReplyAsync($"Deleted {filteredMessages.Count} {(filteredMessages.Count == 1 ? "message" : "messages")}.");
            }
        }

        [Command("echo")]
        [RequireBotManager]
        public async Task EchoAsync(ISocketMessageChannel channel, [Remainder] string message)
            => await channel.SendMessageAsync(message);
    }
}