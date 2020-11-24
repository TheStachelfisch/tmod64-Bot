using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using tMod64Bot.Modules.ConfigSystem;
using tMod64Bot.Utils;

namespace tMod64Bot.Handler
{
    public class LoggingHandler
    {
        private DiscordSocketClient _client;

        public LoggingHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageDeleted += OnMessageDelete;
            _client.UserJoined += OnUserJoin;
            _client.UserLeft += OnUserLeave;
            _client.MessageUpdated += OnMessageUpdate;
            _client.UserUpdated += OnUserUpdate;
            _client.MessagesBulkDeleted += OnBulkDelete;
            _client.UserBanned += OnUserBanned;
            _client.UserUnbanned += OnUserUnbanned;
        }

        private async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
        {
            if (ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()) == 0)
                return;

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithAuthor(user);
            embed.WithTitle("Member unbanned");
            embed.WithDescription($"{user.Mention}");
            embed.WithColor(Color.DarkBlue);
            embed.WithFooter(user.Id.ToString());
            embed.WithCurrentTimestamp();

            _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString()))
                .GetTextChannel(ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()))
                .SendMessageAsync("", false, embed.Build());
        }

        private async Task OnUserBanned(SocketUser user, SocketGuild guild)
        {
            if (ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()) == 0)
                return;

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithAuthor(user);
            embed.WithTitle("Member banned");
            embed.WithDescription($"{user.Mention}");
            embed.WithColor(Color.Red);
            embed.WithFooter(user.Id.ToString());
            embed.WithCurrentTimestamp();

            _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString()))
                .GetTextChannel(ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()))
                .SendMessageAsync("", false, embed.Build());
        }

        private async Task OnBulkDelete(IReadOnlyCollection<Cacheable<IMessage, ulong>> message, ISocketMessageChannel channel)
        {
            TextBuilder text = new TextBuilder();

            foreach (var messages in message.Reverse())
            {
                if (messages.HasValue)
                {
                    if (messages.Value.Embeds.Count >= 1)
                    {
                        text.AddField("<Embed>", $"{messages.Value.Author.Mention}\nSent at: {messages.Value.CreatedAt.ToString("g")}\nId: {messages.Value.Id}");
                    }
                    else
                    {

                        text.WithTitle($"Messages purged in #{channel.Name}");
                        text.AddField($"'{messages.Value.Content}'", $"Author Id: {messages.Value.Author.Id}\nAuthor Name: {messages.Value.Author.Username}\nSent at: {messages.Value.CreatedAt.ToString("g")}\nMessage Id: {messages.Value.Id}");
                    }
                }
            }

            try
            {
                using (StreamWriter writer = new StreamWriter("purged.txt"))
                {
                    writer.Write("");
                    writer.Write(text.Build());
                    writer.Close();
                }

                await channel.SendFileAsync("purged.txt");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task OnUserUpdate(SocketUser userBefore, SocketUser userAfter)
        {
            if (ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()) == 0)
                return;

            EmbedBuilder embed = new EmbedBuilder();

            if (userAfter.Username != userBefore.Username)
            {
                embed.WithAuthor(userAfter);
                embed.WithTitle("Username update");
                embed.WithDescription($"Before: {userBefore.Username}\nAfter: {userAfter.Username}");
                embed.WithColor(255, 192, 203); // Pink
                embed.WithFooter($"Id: {userAfter.Id}");
                embed.WithCurrentTimestamp();
            }
            else if (userAfter.Discriminator != userBefore.Discriminator)
            {
                embed.WithAuthor(userAfter);
                embed.WithTitle("Discriminator update");
                embed.WithDescription($"Before: {userBefore.Username}#{userBefore.Discriminator}\nAfter: {userAfter.Username}#{userAfter.Discriminator}");
                embed.WithColor(128, 0, 128); // Purple
                embed.WithFooter($"Id: {userAfter.Id}");
                embed.WithCurrentTimestamp();
            }
            else if (userAfter.GetAvatarUrl() != userBefore.GetAvatarUrl())
            {
                embed.WithAuthor(userAfter);
                embed.WithTitle("Avatar update");
                embed.WithDescription($"[Avatar Before]({userBefore.GetAvatarUrl()})\n[Avatar After]({userAfter.GetAvatarUrl()})");
                embed.WithThumbnailUrl(userAfter.GetAvatarUrl());
                embed.WithColor(255, 255, 255); // White
                embed.WithFooter($"Id: {userAfter.Id}");
                embed.WithCurrentTimestamp();
            }

            await _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString()))
                .GetTextChannel(ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()))
                .SendMessageAsync("", false, embed.Build());
        }

        private async Task OnMessageUpdate(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            if (ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()) == 0)
                return;

            EmbedBuilder embed = new EmbedBuilder();

            if (newMessage.Channel.Id == ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()) || newMessage.Embeds.Count >= 1 || oldMessage.Value.Embeds.Count >= 1)
                ;
            return;

            if (oldMessage.HasValue)
            {
                embed.WithAuthor(newMessage.Author);
                embed.WithTitle($"Message edited in #{newMessage.Channel.Name}");
                embed.WithDescription($"Before: {oldMessage.Value.Content}\nAfter: {newMessage.Content}");
                embed.WithFooter($"Id: {newMessage.Id}");
                embed.WithColor(Color.Orange);
                embed.WithCurrentTimestamp();
            }
            else
            {
                embed.WithAuthor(newMessage.Author);
                embed.WithTitle($"Message edited in #{newMessage.Channel.Name}");
                embed.WithDescription($"Before: <Message content could not be retrieved>\nAfter: {newMessage.Content}");
                embed.WithFooter($"Id: {newMessage.Id}");
                embed.WithColor(Color.Orange);
                embed.WithCurrentTimestamp();
            }

            await _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString()))
                .GetTextChannel(ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()))
                .SendMessageAsync("", false, embed.Build());
        }

        private Task OnUserLeave(SocketGuildUser user)
        {
            if (ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()) == 0)
                return Task.CompletedTask;

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithAuthor(user);
            embed.WithTitle("Member left");
            embed.WithDescription($"{user.Mention} \n Joined at {user.JoinedAt}\n");
            embed.WithColor(255, 255, 0); // Yellow
            embed.WithFooter(user.Id.ToString());
            embed.WithCurrentTimestamp();

            _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString()))
                .GetTextChannel(ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()))
                .SendMessageAsync("", false, embed.Build());
            return Task.CompletedTask;
        }

        private Task OnUserJoin(SocketGuildUser user)
        {
            if (ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()) == 0)
                return Task.CompletedTask;

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithAuthor(user);
            embed.WithTitle("Member joined");
            embed.WithDescription($"{user.Mention} \n Created at {user.CreatedAt.ToString("MM/dd/yyyy")}\n {user.Guild.MemberCount.ToString("n0")}{NumberUtil.NumberEnding(user.Guild.MemberCount)} to join");
            embed.WithColor(Color.Green);
            embed.WithFooter(user.Id.ToString());
            embed.WithCurrentTimestamp();

            _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString()))
                .GetTextChannel(ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()))
                .SendMessageAsync("", false, embed.Build());
            return Task.CompletedTask;
        }

        private async Task OnMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()) == 0)
                return;

            EmbedBuilder embed = new EmbedBuilder();

            if (message.HasValue)
            {
                if (message.Value.Embeds.Count >= 1)
                    return;

                embed.WithAuthor(message.Value.Author);
                embed.WithTitle($"Message deleted in #{channel.Name}");
                embed.WithDescription(message.Value.Content);
                embed.WithColor(Color.Red);
                embed.WithFooter($"Id: {message.Value.Id}");
                embed.WithCurrentTimestamp();
            }
            else
            {
                embed.WithTitle($"Message deleted in #{channel.Name}");
                embed.WithDescription("<Message Value could not be retrieved>");
                embed.WithColor(Color.Red);
                embed.WithCurrentTimestamp();
            }


            await _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString()))
                .GetTextChannel(ulong.Parse(ConfigService.GetConfig(ConfigEnum.LoggingChannel)?.ToString()))
                .SendMessageAsync("", false, embed.Build());
        }
    }
}