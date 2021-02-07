using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;
using tMod64Bot.Utils;

namespace tMod64Bot.Services.Logging.BotLogging
{
    public class BotLoggingService : ServiceBase
    {
        private readonly ConfigService _config;
        private readonly LoggingService _loggingService;
        private MemoryCache _cache;

        public BotLoggingService(IServiceProvider services) : base(services)
        {
            _loggingService = services.GetRequiredService<LoggingService>();
            _config = services.GetRequiredService<ConfigService>();
            _cache = services.GetRequiredService<MemoryCache>();
        }

        public async Task InitializeAsync()
        {
            Client.UserJoined += HandleUserJoined;
            Client.UserLeft += HandleUserLeft;
            Client.MessageDeleted += HandleDeletedMessage;
            Client.MessageUpdated += HandleMessageUpdated;
            Client.GuildMemberUpdated += HandleUserUpdated;

            //Handle with custom Moderation events
            //Client.UserUnbanned
            //Client.UserUnbanned
        }

        public IWebhook GetOrCreateWebhook(ulong channelId)
        {
            if (_cache.Contains(channelId.ToString()))
            {
                _loggingService.Log(LogSeverity.Verbose, LogSource.Service, "Got Webhook from cache");
                return (_cache.Get(channelId.ToString()) as IWebhook)!;
            }

            CacheItemPolicy policy = new()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
            };

            var channel = Client.GetChannel(channelId) as ITextChannel;
            var webhook = channel?.GetWebhooksAsync().Result
                .FirstOrDefault(x => x.Name.Equals("tMod64-Logging"));

            if (webhook == null)
            {
                FileStream avatar = new("Data/tmod_tree.png", FileMode.Open);

                webhook = channel.CreateWebhookAsync("tMod64-Logging", avatar).Result;
            }

            _cache.Add(new CacheItem(channelId.ToString(), webhook), policy);

            return webhook;
        }

        private async Task HandleUserLeft(SocketGuildUser user)
        {
            var userLoggingChannel = _config.Config.UserLoggingChannel;

            if (userLoggingChannel == 0)
                return;

            var fixedDate = DateTimeOffset.Now - user.JoinedAt!.Value;
            string description = "";

            if (fixedDate.Days == 0 && fixedDate.Days == 0 && fixedDate.Hours == 0 && fixedDate.Minutes == 0)
                description = $"Joined {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else if (fixedDate.Days == 0 && fixedDate.Hours == 0)
                description =
                    $"Joined {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else if (fixedDate.Days == 0)
                description =
                    $"Joined {fixedDate.Hours} {(fixedDate.Hours == 1 ? "Hour" : "Hours")}, {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else
                description =
                    $"Joined {fixedDate.Days} {(fixedDate.Hours == 1 ? "Day" : "Days")}, {fixedDate.Hours} {(fixedDate.Hours == 1 ? "Hour" : "Hours")}, {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";

            using var client = new DiscordWebhookClient(GetOrCreateWebhook(userLoggingChannel));
            {
                List<string> roles = new();
                user.Roles.Where(z => z.IsEveryone == false).ToList().ForEach(x => roles.Add(MentionUtils.MentionRole(x.Id)));

                EmbedBuilder embed = new()
                {
                    Title = "User Left",
                    Color = Color.Orange,
                    Description = $"{MentionUtils.MentionUser(user.Id)} {description}{(roles.Any() ? $"\nRoles: {String.Join(' ', roles)}" : String.Empty)}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"id: {user.Id}"
                    },
                    Timestamp = user.JoinedAt
                };

                embed.WithAuthor(user);

                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
        }

        private async Task HandleUserUpdated(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            var userLoggingChannel = _config.Config.UserLoggingChannel;

            if (userLoggingChannel == 0)
                return;
            
            using var client = new DiscordWebhookClient(GetOrCreateWebhook(userLoggingChannel));
            {
                await client.SendMessageAsync($"Before: {userBefore.Nickname}\nAfter: {userAfter.Nickname}");
            }
        }

        private async Task HandleUserJoined(SocketGuildUser user)
        {
            if (_config.Config.LogUserJoined)
            {
                            var userLoggingChannel = _config.Config.UserLoggingChannel;

            if (userLoggingChannel == 0)
                return;

            var fixedDate = DateTimeOffset.Now - user.CreatedAt;
            string description = "";

            if (fixedDate.Days == 0 && fixedDate.Days == 0 && fixedDate.Hours == 0 && fixedDate.Minutes == 0)
                description = $"Created {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else if (fixedDate.Days == 0 && fixedDate.Hours == 0)
                description =
                    $"Created {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else if (fixedDate.Days == 0)
                description =
                    $"Created {fixedDate.Hours} {(fixedDate.Hours == 1 ? "Hour" : "Hours")}, {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";
            else
                description =
                    $"Created {fixedDate.Days} {(fixedDate.Hours == 1 ? "Day" : "Days")}, {fixedDate.Hours} {(fixedDate.Hours == 1 ? "Hour" : "Hours")}, {fixedDate.Minutes} {(fixedDate.Minutes == 1 ? "Minute" : "Minutes")} and {fixedDate.Seconds} {(fixedDate.Seconds == 1 ? "Second" : "Seconds")} ago";

            using var client = new DiscordWebhookClient(GetOrCreateWebhook(userLoggingChannel));
            {
                EmbedBuilder embed = new()
                {
                    Title = "User joined",
                    Color = Color.Green,
                    Description =
                        $"{MentionUtils.MentionUser(user.Id)} {user.Guild.MemberCount + 1}{(user.Guild.MemberCount).NumberEnding()} to join\n{description}",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"id: {user.Id}"
                    }
                };

                embed.WithAuthor(user);

                await client.SendMessageAsync(embeds: new[] {embed.Build()});
            }
            }
        }

        private async Task HandleMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage message, ISocketMessageChannel channel)
        {
            if (_config.Config.LogMessageUpdated)
            {
                var userLoggingChannel = _config.Config.UserLoggingChannel;

                if (userLoggingChannel == 0)
                    return;

                var guildChannel = channel as SocketGuildChannel;
            }
        }

        private async Task HandleDeletedMessage(Cacheable<IMessage, ulong> messageBefore, ISocketMessageChannel channel)
        {
            if (_config.Config.LogMessageDeleted)
            {
                if (messageBefore.HasValue && (messageBefore.Value.Author.IsWebhook || messageBefore.Value.Embeds.Any()))
                    return;

                var userLoggingChannel = _config.Config.UserLoggingChannel;

                if (userLoggingChannel == 0)
                    return;
                using var client = new DiscordWebhookClient(GetOrCreateWebhook(userLoggingChannel));
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

                    await client.SendMessageAsync(embeds: new[] {embed.Build()});
                }
            }
        }
    }
}