using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging;

namespace tMod64Bot.Services
{
    public class ModerationService : ServiceBase
    {
        private Timer _timer;
        private readonly ConfigService _configService;
        private readonly LoggingService _loggingService;
        
        public delegate void BanEventHandler(SocketUser user, SocketGuildUser moderator, SocketGuild guild, string reason);
        public delegate void MuteEventHandler(SocketUser user, SocketGuildUser moderator, SocketGuild guild, string reason, TimeSpan muteTime);
        public event BanEventHandler UserBanned;
        public event MuteEventHandler UserMuted;

        public ModerationService(IServiceProvider services) : base(services)
        {
            _configService = services.GetRequiredService<ConfigService>();
            _loggingService = services.GetRequiredService<LoggingService>();
            
            _timer = new();
            _timer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
        }

        public async Task InitializeAsync()
        {
            await _loggingService.Log("Init called");
            
            _timer.Elapsed += CheckIfExpired;
            _timer.Start();
        }

        private async void CheckIfExpired(object sender, ElapsedEventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (_configService.Config.MutedRole == 0)
                return;
            
            var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            
            foreach (var mutedUser in _configService.Config.MutedUsers)
            {
                if (mutedUser.ExpireTime <= currentTime)
                {
                    var guild = Client.GetGuild(mutedUser.ServerId);
                    var user = guild.GetUser(mutedUser.UserId);
                    
                    if (user == null || user.Roles.All(x => x.Id != _configService.Config.MutedRole))
                    {
                        _configService.Config.MutedUsers.RemoveWhere(x => x.UserId == mutedUser.UserId);
                        _configService.SaveData();
                    }

                    await user!.RemoveRoleAsync(guild.GetRole(_configService.Config.MutedRole));
                }
            }

            sw.Stop();
            await _loggingService.Log($"Took {sw.ElapsedMilliseconds}ms to check if expired");
        }

        public static TimeSpan GetTimeSpan(string input)
        {
            TimeSpan temp = TimeSpan.Zero;

            var dayMatch = Regex.Match(input, @"(\d|\d\d|\d\d\d)[d|day|days]");
            if (dayMatch.Success)
                temp = temp.Add(TimeSpan.FromDays(int.Parse(dayMatch.Groups[1].Value)));
            
            var hourMatch = Regex.Match(input, @"(\d|\d\d|\d\d\d)[h|hour|hours]");
            if (hourMatch.Success)
                temp = temp.Add(TimeSpan.FromHours(int.Parse(hourMatch.Groups[1].Value)));

            var minuteMatch = Regex.Match(input, @"(\d|\d\d|\d\d\d)[m|minute|minutes]");
            if (minuteMatch.Success)
                temp = temp.Add(TimeSpan.FromMinutes(int.Parse(minuteMatch.Groups[1].Value)));

            if (temp.TotalSeconds == 0)
                throw new ArgumentNullException(nameof(input), "Value cannot be null or zero");

            return temp;
        }
        
        public async Task BanUser(SocketGuildUser moderator, SocketGuildUser user, string reason)
        {
            try
            {
                if (_configService.Config.MessageOnBan)
                {
                    List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();

                    fields.Add(new()
                    {
                        Name = "Reason",
                        Value = reason,
                        IsInline = true
                    });

                    if (_configService.Config.MessageWithModerator)
                    {
                        fields.Add(new()
                        {
                            Name = "Responsible Moderator",
                            Value = moderator.Username,
                            IsInline = true
                        });
                    }
                    
                    EmbedBuilder embed = new()
                    {
                        Title = $"You have been banned from tModLoader 64 bit",
                        Fields = fields,
                        Color = Color.DarkRed,
                        Timestamp = DateTimeOffset.Now
                    };

                    await user.SendMessageAsync(embed:embed.Build());
                }
            }
            catch (Exception) { /* Ignore */ }
            
            await user.Guild.AddBanAsync(user, 0, reason);
            UserBanned?.Invoke(user, moderator, user.Guild, reason);
        }

        public async Task MuteUser(TimeSpan muteTime, SocketGuildUser moderator, SocketGuildUser user, string reason)
        {
            if (_configService.Config.MutedRole == 0)
                return;

            if (user.Roles.Any(x => x.Id == _configService.Config.MutedRole))
                return;

            try
            {
                if (_configService.Config.MessageOnBan)
                {
                    List<EmbedFieldBuilder> fields = new();

                    fields.Add(new()
                    {
                        Name = "Reason",
                        Value = reason,
                        IsInline = true
                    });
                    
                    fields.Add(new()
                    {
                        Name = "Mute Duration",
                        Value = muteTime.ToString("g")
                    });

                    if (_configService.Config.MessageWithModerator)
                    {
                        fields.Add(new()
                        {
                            Name = "Responsible Moderator",
                            Value = moderator.Username,
                            IsInline = true
                        });
                    }
                    
                    EmbedBuilder embed = new()
                    {
                        Title = $"You have been muted in tModLoader 64 bit",
                        Fields = fields,
                        Color = Color.DarkRed,
                        Timestamp = DateTimeOffset.Now
                    };

                    await user.SendMessageAsync(embed:embed.Build());
                }
            }
            catch (Exception) { /* Ignore */ }
            
            await user.AddRoleAsync(user.Guild.GetRole(_configService.Config.MutedRole));
            if (muteTime != TimeSpan.Zero && !_configService.Config.MutedUsers.Select(x => x.UserId).Contains(user.Id))
            {
                _configService.Config.MutedUsers.Add(new()
                {
                    ExpireTime = DateTimeOffset.Now.Add(muteTime).ToUnixTimeSeconds(),
                    ServerId = moderator.Guild.Id,
                    UserId = user.Id
                });
                _configService.SaveData();
            }
        }
    }
}