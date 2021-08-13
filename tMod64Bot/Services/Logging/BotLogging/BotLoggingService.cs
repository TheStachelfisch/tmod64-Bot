using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Commons;
using tMod64Bot.Services.Config;
using tMod64Bot.Utils;

namespace tMod64Bot.Services.Logging.BotLogging
{
    public class BotLoggingService : ServiceBase, IInitializeable
    {
        private readonly ConfigService _config;
        private readonly LoggingService _loggingService;
        private readonly ModerationService _moderationService;
        private readonly WebhookService _webhook;
        private readonly InviteProtectionService _inviteProtectionService;

        public BotLoggingService(IServiceProvider services) : base(services)
        {
            _loggingService = services.GetRequiredService<LoggingService>();
            _config = services.GetRequiredService<ConfigService>();
            _webhook = services.GetRequiredService<WebhookService>();
            _moderationService = services.GetRequiredService<ModerationService>();
            _inviteProtectionService = services.GetRequiredService<InviteProtectionService>();
        }

        public async Task Initialize()
        {
            Client.UserJoined += HandleUserJoined;
            Client.UserLeft += HandleUserLeft;
            Client.MessageDeleted += HandleDeletedMessage;
            Client.MessageUpdated += HandleMessageUpdated;
            Client.UserUpdated += HandleUserUpdated;
            Client.GuildMemberUpdated += HandleGuildMemberUpdated;

            _moderationService.UserUnbanned += HandleUserUnbanned;
            _moderationService.UserKicked += HandleUserKicked;
            _moderationService.UserUnMuted += HandlerUserUnmuted;
            _moderationService.UserTempBanned += HandleUserTempBanned;
            _moderationService.UserMuted += HandleUserMuted;
            _moderationService.UserBanned += HandlerUserBanned;

            _inviteProtectionService.InviteDeleted += HandleInviteDeleted;
        }

        private async Task HandleGuildMemberUpdated(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            var userLoggingChannel = _config.Config.UserLoggingChannel;

            if (userLoggingChannel == 0 || !_config.Config.LogUserUpdated)
                return;
            
            if (userBefore.Nickname != userAfter.Nickname)
            {
                using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(userLoggingChannel));
                {
                    var embed = new EmbedBuilder
                    {
                        Title = $"{(userAfter.Nickname.IsNullOrWhitespace() ? "Nickname reset" : $"{(userBefore.Nickname.IsNullOrWhitespace() ? "Nickname added" : "Nickname changed")}")}",
                        Color = Color.Gold,
                        Description = $"{(userAfter.Nickname.IsNullOrWhitespace() ? $"**Before**: {userBefore.Nickname}" : $"{(userBefore.Nickname.IsNullOrWhitespace() ? $"**Nickname**: {userAfter.Nickname}" : $"**Before**: {userBefore.Nickname}\n**After**: {userAfter.Nickname}")}")}",
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"User id: {userAfter.Id}"
                        }
                    };

                    embed.WithAuthor(userAfter);

                    await client.SendMessageAsync(embeds: new[] {embed.Build()});
                }
            }
        }

        private void HandleInviteDeleted(SocketGuildUser user, SocketMessage message, Match match)
        {
            if (_config.Config.ModerationLoggingChannel == 0)
                return;
            
            using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(_config.Config.ModerationLoggingChannel));
            {
                List<EmbedFieldBuilder> fields = new();
                EmbedBuilder embed = new();

                fields.Add(new EmbedFieldBuilder
                {
                    Name = "User",
                    Value = user.ToString(),
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Message",
                    Value = message.Content
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Match",
                    Value = match.Value
                });
                
                embed.WithTitle($"Invite deleted from #{message.Channel}");
                embed.WithColor(Color.Orange);
                embed.WithFooter($"User id: {user.Id}");
                embed.WithFields(fields);
                embed.WithAuthor(user);

                client.SendMessageAsync(embeds: new[] {embed.Build()}).GetAwaiter();
            }
        }

        private async void HandleUserKicked(SocketUser user, SocketGuildUser moderator, SocketGuild guild, string reason)
        {
            if (_config.Config.ModerationLoggingChannel == 0)
                return;
            
            using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(_config.Config.ModerationLoggingChannel));
            {
                List<EmbedFieldBuilder> fields = new();
                EmbedBuilder embed = new();

                fields.Add(new EmbedFieldBuilder
                {
                    Name = "User",
                    Value = user.ToString(),
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Reason",
                    Value = reason,
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Moderator",
                    Value = moderator == null ? "Automated ban" : moderator.ToString(),
                    IsInline = true
                });
                
                embed.WithTitle("User kicked");
                embed.WithColor(Color.LightOrange);
                embed.WithFooter($"User id: {user.Id} | Moderator id: {moderator.Id}");
                embed.WithFields(fields);
                embed.WithAuthor(user);

                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }

        private async void HandlerUserUnmuted(SocketUser user, SocketGuildUser moderator, SocketGuild guild, string reason, TimeSpan mutetime)
        {
            if (_config.Config.ModerationLoggingChannel == 0)
                return;
            
            using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(_config.Config.ModerationLoggingChannel));
            {
                List<EmbedFieldBuilder> fields = new();
                EmbedBuilder embed = new();

                fields.Add(new EmbedFieldBuilder
                {
                    Name = "User",
                    Value = user.ToString(),
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Moderator",
                    Value = moderator != null ? moderator.ToString() : "Automatic Unmute",
                    IsInline = true
                });
                
                embed.WithTitle("User Un-muted");
                embed.WithColor(Color.Green);
                embed.WithFooter($"User id: {user.Id} | Moderator id: {(moderator != null ? moderator.Id : "N/A")}");
                embed.WithFields(fields);
                embed.WithAuthor(user);

                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }

        private async void HandleUserUnbanned(ulong userid, SocketGuildUser moderator, SocketGuild guild)
        {
            if (_config.Config.ModerationLoggingChannel == 0)
                return;
            
            using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(_config.Config.ModerationLoggingChannel));
            {
                List<EmbedFieldBuilder> fields = new();
                EmbedBuilder embed = new();

                fields.Add(new EmbedFieldBuilder
                {
                    Name = "User id",
                    Value = userid,
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Moderator",
                    Value = moderator != null ? moderator.ToString() : "Automatic unban",
                    IsInline = true
                });
                
                embed.WithTitle("User Un-banned");
                embed.WithColor(Color.Green);
                embed.WithFooter($"User id: {userid} | Moderator id: {(moderator != null ? moderator.Id : "N/A")}");
                embed.WithFields(fields);
                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }

        private async void HandleUserTempBanned(IUser user, SocketGuildUser moderator, SocketGuild guild, TimeSpan bantime, string reason)
        {
            if (_config.Config.ModerationLoggingChannel == 0)
                return;
            
            using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(_config.Config.ModerationLoggingChannel));
            {
                List<EmbedFieldBuilder> fields = new();
                EmbedBuilder embed = new();

                fields.Add(new EmbedFieldBuilder
                {
                    Name = "User",
                    Value = user.ToString(),
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Ban Time",
                    Value = bantime != TimeSpan.Zero ? bantime.FormatString(true) : "Indefinitely",
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Reason",
                    Value = reason,
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Moderator",
                    Value = moderator.ToString(),
                    IsInline = true
                });
                
                embed.WithTitle("User Temp-banned");
                embed.WithColor(Color.Red);
                embed.WithFooter($"User id: {user.Id} | Moderator id: {moderator.Id}");
                embed.WithFields(fields);
                embed.WithAuthor(user);

                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }

        private async void HandleUserMuted(SocketUser user, SocketGuildUser moderator, SocketGuild guild, string reason, TimeSpan mutetime)
        {
            if (_config.Config.ModerationLoggingChannel == 0)
                return;
            
            using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(_config.Config.ModerationLoggingChannel));
            {
                List<EmbedFieldBuilder> fields = new();
                EmbedBuilder embed = new();

                fields.Add(new EmbedFieldBuilder
                {
                    Name = "User",
                    Value = user.ToString(),
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Mute Time",
                    Value = mutetime != TimeSpan.Zero ? mutetime.FormatString(true) : "Indefinitely",
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Reason",
                    Value = reason,
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Moderator",
                    Value = moderator.ToString(),
                    IsInline = true
                });
                
                embed.WithTitle("User Muted");
                embed.WithColor(Color.Red);
                embed.WithFooter($"User id: {user.Id} | Moderator id: {moderator.Id}");
                embed.WithFields(fields);
                embed.WithAuthor(user);

                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }

        private async void HandlerUserBanned(IUser user, SocketGuildUser moderator, SocketGuild guild, string reason)
        {
            if (_config.Config.ModerationLoggingChannel == 0)
                return;
            
            using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(_config.Config.ModerationLoggingChannel));
            {
                List<EmbedFieldBuilder> fields = new();
                EmbedBuilder embed = new();

                fields.Add(new EmbedFieldBuilder
                {
                    Name = "User",
                    Value = user.ToString(),
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Reason",
                    Value = reason,
                    IsInline = true
                });
                fields.Add(new EmbedFieldBuilder
                {
                    Name = "Moderator",
                    Value = moderator.ToString(),
                    IsInline = true
                });
                
                embed.WithTitle("User Banned");
                embed.WithColor(Color.LightOrange);
                embed.WithFooter($"User id: {user.Id} | Moderator id: {moderator.Id}");
                embed.WithFields(fields);
                embed.WithAuthor(user);

                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }
        
        private async Task HandleUserLeft(SocketGuildUser user)
        {
            var userLoggingChannel = _config.Config.UserLoggingChannel;

            if (userLoggingChannel == 0 || !_config.Config.LogUserLeft)
                return;

            var fixedDate = DateTimeOffset.Now - user.JoinedAt!.Value;
            string description = "";

            if (fixedDate.Days == 0 && fixedDate.Days == 0 && fixedDate.Hours == 0 && fixedDate.Minutes == 0)
                description = $"Joined {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else if (fixedDate.Days == 0 && fixedDate.Hours == 0)
                description = $"Joined {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else if (fixedDate.Days == 0)
                description = $"Joined {fixedDate.Hours} {(fixedDate.Hours == 1 ? "Hour" : "Hours")}, {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else
                description = $"Joined {fixedDate.Days} {(fixedDate.Hours == 1 ? "Day" : "Days")}, {fixedDate.Hours} {(fixedDate.Hours == 1 ? "Hour" : "Hours")}, {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";

            using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(userLoggingChannel));
            {
                List<string> roles = new();
                user.Roles.Where(z => z.IsEveryone == false).ToList()
                    .ForEach(x => roles.Add(MentionUtils.MentionRole(x.Id)));

                EmbedBuilder embed = new()
                {
                    Title = "User Left",
                    Color = Color.Orange,
                    Description = $"{MentionUtils.MentionUser(user.Id)} {description}{(roles.Any() ? $"\nRoles: {string.Join(' ', roles)}" : string.Empty)}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"id: {user.Id}"
                    },
                    Timestamp = user.JoinedAt
                };

                embed.WithAuthor(user);
                embed.WithCurrentTimestamp();

                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }

        private async Task HandleUserUpdated(SocketUser userBefore, SocketUser userAfter)
        {
            var userLoggingChannel = _config.Config.UserLoggingChannel;

            if (userLoggingChannel == 0 || !_config.Config.LogUserUpdated)
                return;

            if (userBefore.Username != userAfter.Username)
            {
                using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(userLoggingChannel));
                {
                    var embed = new EmbedBuilder
                    {
                        Title = "Username changed",
                        Color = Color.Gold,
                        Description = $"**Before**: {userBefore.Username}\n**After**: {userAfter.Username}",
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"User id: {userAfter.Id}"
                        }
                    };

                    embed.WithAuthor(userAfter);

                    await client.SendMessageAsync(embeds: new[] {embed.Build()});
                }
            }
            else if (userBefore.DiscriminatorValue != userAfter.DiscriminatorValue)
            {
                using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(userLoggingChannel));
                {
                    var embed = new EmbedBuilder
                    {
                        Title = "Discriminator changed",
                        Color = Color.Gold,
                        Description = $"**Before**: {userBefore.Discriminator}\n**After**: {userAfter.Discriminator},",
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"User id: {userAfter.Id}"
                        }
                    };

                    embed.WithAuthor(userAfter);

                    await client.SendMessageAsync(embeds: new[] {embed.Build()});
                }
            }
            else if (userBefore.GetAvatarUrl() != userAfter.GetAvatarUrl())
            {
                using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(userLoggingChannel));
                {
                    var embed = new EmbedBuilder
                    {
                        Title = "Avatar changed",
                        Color = Color.Gold,
                        ImageUrl = userAfter.GetAvatarUrl(),
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"User id: {userAfter.Id}"
                        }
                    };

                    embed.WithAuthor(userAfter);

                    await client.SendMessageAsync(embeds: new[] {embed.Build()});
                }
            }
        }

        private async Task HandleUserJoined(SocketGuildUser user)
        {
            var userLoggingChannel = _config.Config.UserLoggingChannel;

            if (userLoggingChannel == 0 || !_config.Config.LogUserJoined)
                return;

            var fixedDate = DateTimeOffset.Now - user.CreatedAt;
            string description = "";

            if (fixedDate.Days == 0 && fixedDate.Days == 0 && fixedDate.Hours == 0 && fixedDate.Minutes == 0)
                description = $"Created {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else if (fixedDate.Days == 0 && fixedDate.Hours == 0)
                description = $"Created {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else if (fixedDate.Days == 0)
                description = $"Created {fixedDate.Hours} {(fixedDate.Hours == 1 ? "Hour" : "Hours")}, {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else
                description = $"Created {fixedDate.Days} {(fixedDate.Hours == 1 ? "Day" : "Days")}, {fixedDate.Hours} {(fixedDate.Hours == 1 ? "Hour" : "Hours")}, {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";

            using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(userLoggingChannel));
            {
                EmbedBuilder embed = new()
                {
                    Title = "User joined",
                    Color = Color.Green,
                    Description = $"{MentionUtils.MentionUser(user.Id)} {user.Guild.MemberCount}{user.Guild.MemberCount.NumberEnding()} to join\n{description}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"id: {user.Id}"
                    }
                };

                embed.WithAuthor(user);
                embed.WithTimestamp((DateTimeOffset) user.JoinedAt);

                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }

        private async Task HandleMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            if (messageBefore.HasValue && messageBefore.Value.Content == messageAfter.Content)
                return;

            if (_config.Config.LogMessageUpdated)
            {
                var userLoggingChannel = _config.Config.UserLoggingChannel;

                if (userLoggingChannel == 0 || !_config.Config.LogMessageUpdated)
                    return;

                if (messageAfter.Author.IsBot || messageAfter.Author.IsWebhook)
                    return;

                using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(userLoggingChannel));
                {
                    var embed = new EmbedBuilder
                    {
                        Title = $"Message edited in #{channel.Name}",
                        Color = Color.DarkMagenta,
                        Description = $"[Jump!]({messageAfter.GetJumpUrl()})\n\n" +
                                      $"**Before:** {(messageBefore.HasValue ? messageBefore.Value.Content : "Message Content could not be retrieved")}\n" +
                                      $"**After:** {messageAfter.Content}",
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"id: {messageAfter.Id}"
                        }
                    };

                    embed.WithAuthor(messageAfter.Author);
                    embed.WithTimestamp(messageAfter.Timestamp);

                    await client.SendMessageAsync(embeds: new[] {embed.Build()});
                }
            }
        }

        private async Task HandleDeletedMessage(Cacheable<IMessage, ulong> messageBefore, ISocketMessageChannel channel)
        {
            if (messageBefore.HasValue && (messageBefore.Value.Author.IsWebhook || messageBefore.Value.Embeds.Any()))
                return;

            var userLoggingChannel = _config.Config.UserLoggingChannel;

            if (userLoggingChannel == 0 || !_config.Config.LogMessageDeleted)
                return;
            using var client = new DiscordWebhookClient(_webhook.GetOrCreateWebhook(userLoggingChannel));
            {
                // Conditional Operator go brrrrrrr
                EmbedBuilder embed = new()
                {
                    Title = $"Message Deleted from #{channel.Name}",
                    Color = Color.Red,
                    Description = messageBefore.HasValue ? messageBefore.Value.Content : "Message could not be retrieved",
                    Footer = new EmbedFooterBuilder {Text = messageBefore.Id.ToString()},
                    Timestamp = messageBefore.HasValue ? messageBefore.Value.Timestamp : null
                };

                if (messageBefore.HasValue)
                    embed.WithAuthor(messageBefore.Value.Author);
                embed.WithTimestamp(messageBefore.HasValue ? messageBefore.Value.Timestamp : DateTimeOffset.Now);

                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }
    }
}