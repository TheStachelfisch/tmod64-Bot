using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using tMod64Bot.Services;

namespace tMod64Bot.Modules
{
    public class ModEventArgs : EventArgs
    {
        public IGuildUser OffendingUser { get; set; }
        public SocketUser ResponsibleModerator { get; set; }
        public string Reason { get; set; }
    }

    public class MuteEventArgs : ModEventArgs
    {
        public string Time { get; set; }
    }

    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        public ConfigService Config { get; set; }

        public delegate void KickEventHandler(object source, ModEventArgs args);
        public static event KickEventHandler UserKicked;

        public delegate void BanEventHandler(object source, ModEventArgs args);
        public static event BanEventHandler UserBanned;

        public delegate void SoftBanEventHandler(object source, ModEventArgs args);
        public static event SoftBanEventHandler UserSoftBanned;

        public delegate void MuteEventHandler(object source, MuteEventArgs args);

        public static event MuteEventHandler UserMuted;

        [Command("kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [Summary("Kicks a user mentioned.")]
        public async Task KickAsync(IGuildUser user, [Remainder] string reason = "No reason specified.")
        {
            var victim = user as SocketGuildUser;
            var author = Context.User as SocketGuildUser;

            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("Just leave");
                return;
            }

            if (victim.Hierarchy >= author.Hierarchy)
                await ReplyAsync("You are not high enough in the Role Hierarchy to do that");
            else
            {
                await user.KickAsync(reason);
                UserKicked?.Invoke(this, new ModEventArgs() { OffendingUser = user, ResponsibleModerator = Context.User, Reason = reason });
                await ReplyAsync("User " + user.Username + " has been kicked. Reason: " + reason);
            }
        }

        [Command("ban"), Alias("b")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Summary("Bans a user mentioned.")]
        public async Task BanAsync(IGuildUser user, [Remainder] string reason = "No reason specified.")
        {
            var victim = user as SocketGuildUser;
            var author = Context.User as SocketGuildUser;

            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("Just leave");
                return;
            }

            if (victim.Hierarchy >= author.Hierarchy)
                await ReplyAsync("You are not high enough in the Role Hierarchy to do that");
            else
            {
                await user.BanAsync(0, reason);
                UserBanned?.Invoke(this, new ModEventArgs() { OffendingUser = user, ResponsibleModerator = Context.User, Reason = reason });
                await ReplyAsync("User " + user.Username + " has been banned. Reason: " + reason);
            }
        }

        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Adds a muted role onto the user.")]
        public async Task MuteAsync(IGuildUser user, string time, [Remainder] string reason = "No reason specified.") //TODO: temporary unmute, time to unmute
        {
            await user.AddRoleAsync(Context.Client.GetGuild(Context.Guild.Id).GetRole(Config.MutedRole));
            UserMuted?.Invoke(this, new MuteEventArgs() { OffendingUser = user, ResponsibleModerator = Context.User, Reason = reason, Time = time });
            IUserMessage MessageToDelete = await ReplyAsync($"User {user.Username} was muted for {time}, Reason: {reason}");
            await Task.Delay(2500);
            await MessageToDelete.DeleteAsync();
        }

        [Command("softban"), Alias("sb")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Summary("Banishes a user to the shadow realm!")]
        public async Task SBanAsync(IGuildUser user, [Remainder] string reason = "No reason specified.")
        {
            var victim = user as SocketGuildUser;
            var author = Context.User as SocketGuildUser;

            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("Just leave");
                return;
            }

            if (victim.Hierarchy >= author.Hierarchy)
                await ReplyAsync("You are not high enough in the Role Hierarchy to do that");
            else
            {
                await user.AddRoleAsync(Context.Client.GetGuild(Context.Guild.Id).GetRole(Config.SoftbanRole));
                UserSoftBanned?.Invoke(this, new ModEventArgs() { OffendingUser = user, ResponsibleModerator = Context.User, Reason = reason });
                await ReplyAsync("User " + user.Username + " was banished to the Shadow Realm. Reason: " + reason);
            }
        }

        [Command("echo")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Makes the bot say something in the specified chat, anonymously")]
        public async Task EchoAsync(IGuildChannel channel, [Remainder] string message)
        {
            await Context.Guild.GetTextChannel(channel.Id).SendMessageAsync(message);
        }
    }

    [Group("purge")]
    public class PurgeModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("purges a specified amount of messages")]
        public async Task PurgeMessageAsync(int amount)
        {
            //Embeds are pretty you know
            EmbedBuilder amountErrorEmbed = new EmbedBuilder();
            EmbedBuilder successEmbed = new EmbedBuilder();

            if (amount <= 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription("The amount of messages cant be zero or negative");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                await ReplyAsync("", false, amountErrorEmbed.Build());
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

            var filteredMessages = messages.Where(x => (DateTimeOffset.Now - x.Timestamp).TotalDays <= 14);

            var count = filteredMessages.Count();

            if (count == 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription("No messages found to delete");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                await ReplyAsync("", false, amountErrorEmbed.Build());
            }
            else
            {
                await (Context?.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                successEmbed.WithTitle($"Successfully deleted {filteredMessages.Count()} messages");
                successEmbed.WithColor(Color.DarkGreen);
                successEmbed.WithCurrentTimestamp();

                var embedMessage = await ReplyAsync("", false, successEmbed.Build());
                await Task.Delay(5000);

                await embedMessage.DeleteAsync();
                await Context.Message.DeleteAsync();
            }
        }

        [Command("user")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("purges a specified amount of messages from a specific user")]
        public async Task PurgeUserAsync(IGuildUser user, int messageCount)
        {
            EmbedBuilder amountErrorEmbed = new EmbedBuilder();
            EmbedBuilder successEmbed = new EmbedBuilder();

            if (messageCount <= 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription("The amount of messages cant be zero or negative");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                var embedMessage = await ReplyAsync("", false, amountErrorEmbed.Build());

                await Task.Delay(2500);

                await embedMessage.DeleteAsync();
                await Context.Message.DeleteAsync();
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, messageCount).FlattenAsync();

            var filteredMessages = messages.Where(x => (DateTimeOffset.Now - x.Timestamp).TotalDays <= 14).Where(u => u.Author.Id.Equals(user.Id));

            var count = filteredMessages.Count();

            if (count == 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription("No messages found to delete");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                var embedMessage = await ReplyAsync("", false, amountErrorEmbed.Build());

                await Task.Delay(2500);

                await embedMessage.DeleteAsync();
                await Context.Message.DeleteAsync();
            }
            else
            {
                await (Context?.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                successEmbed.WithTitle($"Successfully deleted {filteredMessages.Count()} messages from");
                successEmbed.WithDescription(MentionUtils.MentionUser(user.Id));
                successEmbed.WithColor(Color.DarkGreen);
                successEmbed.WithCurrentTimestamp();

                var embedMessage = await ReplyAsync("", false, successEmbed.Build());
                await Task.Delay(2500);

                await embedMessage.DeleteAsync();
                await Context.Message.DeleteAsync();
            }
        }

        [Command("contains"), Alias("contain")]
        [Summary("Deletes a specified amount of message that contain a certain string")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeContainAsync(int amount, string contain)
        {
            contain = contain.ToLower();

            EmbedBuilder amountErrorEmbed = new EmbedBuilder();
            EmbedBuilder successEmbed = new EmbedBuilder();

            if (amount <= 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription("The amount of messages cant be zero or negative");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                await ReplyAsync("", false, amountErrorEmbed.Build());
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

            var filteredMessages = messages.Where(x => (DateTimeOffset.Now - x.Timestamp).TotalDays <= 14).Where(m => m.Content.ToLower().Contains(contain));

            var count = filteredMessages.Count();

            if (count == 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription($"No messages found that contained **'{contain}'***");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                await ReplyAsync("", false, amountErrorEmbed.Build());
            }
            else
            {
                await (Context?.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                successEmbed.WithTitle($"Successfully deleted {filteredMessages.Count()} messages that contained '**{contain}**'");
                successEmbed.WithColor(Color.DarkGreen);
                successEmbed.WithCurrentTimestamp();

                var embedMessage = await ReplyAsync("", false, successEmbed.Build());
                await Task.Delay(2500);

                await embedMessage.DeleteAsync();
                await Context.Message.DeleteAsync();
            }
        }
    }
}