using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Commons;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging;

namespace tMod64Bot.Services
{
    public class ModerationService : ServiceBase, IInitializeable
    {
        private Timer _timer;
        private readonly ConfigService _configService;

        public delegate void BanEventHandler(IUser user, SocketGuildUser moderator, SocketGuild guild, string reason);
        public delegate void UnbanEventHandler(ulong userId, SocketGuildUser moderator, SocketGuild guild);
        public delegate void TempBanEventHandler(IUser user, SocketGuildUser moderator, SocketGuild guild, TimeSpan banTime, string reason);
        public delegate void MuteEventHandler(SocketUser user, SocketGuildUser moderator, SocketGuild guild, string reason, TimeSpan muteTime);
        public delegate void KickEventHandler(SocketUser user, SocketGuildUser moderator, SocketGuild guild, string reason, string moderationLogReason = "");
        public event BanEventHandler UserBanned;
        public event KickEventHandler UserKicked;
        public event UnbanEventHandler UserUnbanned;
        public event TempBanEventHandler UserTempBanned;
        public event MuteEventHandler UserMuted;
        public event MuteEventHandler UserUnMuted;

        public ModerationService(IServiceProvider services) : base(services)
        {
            _configService = services.GetRequiredService<ConfigService>();

            _timer = new();
            _timer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
        }

        public void InvokeKick(SocketUser user, SocketGuildUser moderator, SocketGuild guild, string reason, string moderationLogReason = "")
        {
            UserKicked?.Invoke(user, moderator, guild, reason, moderationLogReason = "");
        }
        
        public async Task Initialize()
        {
            _timer.Elapsed += CheckIfExpired;
            _timer.Start();
        }

        private async void CheckIfExpired(object sender, ElapsedEventArgs e)
        {
            var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            if (_configService.Config.MutedRole == 0)
                goto A;
            
            foreach (var mutedUser in _configService.Config.MutedUsers)
            {
                if (mutedUser.ExpireTime <= currentTime)
                {
                    await Task.Run(async () =>
                    {
                        var guild = Client.GetGuild(mutedUser.ServerId);
                        var user = guild.GetUser(mutedUser.UserId);

                        if (user == null || user.Roles.All(x => x.Id != _configService.Config.MutedRole))
                        {
                            _configService.Config.MutedUsers.RemoveWhere(x => x.UserId == mutedUser.UserId);
                            _configService.SaveData();
                        }
                        else
                        {
                            await user!.RemoveRoleAsync(guild.GetRole(_configService.Config.MutedRole));
                            UserUnMuted?.Invoke(user, null, guild, null, TimeSpan.Zero);

                            _configService.Config.MutedUsers.RemoveWhere(x => x.UserId == mutedUser.UserId);
                            _configService.SaveData();
                        }
                    });
                }
            }
            A:

            foreach (var tempBannedUser in _configService.Config.TempBannedUsers)
            {
                if (tempBannedUser.ExpireTime <= currentTime)
                {
                    await Task.Run(async () =>
                    {
                        var guild = Client.GetGuild(tempBannedUser.ServerId);

                        if (guild.GetBansAsync().Result.All(x => x.User.Id != tempBannedUser.UserId))
                        {
                            _configService.Config.TempBannedUsers.RemoveWhere(x => x.UserId == tempBannedUser.UserId);
                            _configService.SaveData();
                        }

                        await guild.RemoveBanAsync(tempBannedUser.UserId);
                        UserUnbanned?.Invoke(tempBannedUser.UserId, null, guild);

                        _configService.Config.TempBannedUsers.RemoveWhere(x => x.UserId == tempBannedUser.UserId);
                        _configService.SaveData();
                    });
                }
            }
        }

        public static TimeSpan GetTimeSpan(string input)
        {
            TimeSpan temp = TimeSpan.Zero;
            AddTimeSpan("d", i => temp += TimeSpan.FromDays(i));
            AddTimeSpan("h", i => temp += TimeSpan.FromHours(i));
            AddTimeSpan("m", i => temp += TimeSpan.FromMinutes(i));
            AddTimeSpan("s", i => temp += TimeSpan.FromSeconds(i));
            
            if (temp.TotalSeconds == 0) 
                throw new ArgumentException("Malformated string or time is less than or equal to 0 seconds");
            if (temp.TotalSeconds < 60)
                throw new ArgumentException("Time may not be less than 60 seconds");

            void AddTimeSpan(string letter, Action<int> additionMethod)
            {
                var match = Regex.Match(input, @"(\d{1,6})" + letter, RegexOptions.CultureInvariant);
                
                if (match.Success)
                    additionMethod(int.Parse(match.Groups[1].Value));
            }

            return temp;
        }
        
        public async Task<TaskResult> BanUser(SocketGuildUser moderator, IUser user, string reason)
        {
            // if (moderator.Guild.GetBansAsync().Result.Any(x => x.User.Id == user.Id))
            //     return TaskResult.FromError("User is already banned");

            await moderator.Guild.AddBanAsync(user, 0, $"{reason}\nBanned by {moderator}");
            UserBanned?.Invoke(user, moderator, moderator.Guild, reason);
            
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

                    if (_configService.Config.MessageWithModerator)
                    {
                        fields.Add(new()
                        {
                            Name = "Responsible Moderator",
                            Value = moderator.ToString(),
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
            
            return TaskResult.FromSuccess();
        }

        // SocketGuild is needed; Since it can't be retrieved from SocketUser
        public async Task<TaskResult> UnbanUser(ulong userId, SocketGuild guild, SocketGuildUser moderator)
        {
            try
            {
                if (guild.GetBansAsync().Result.All(x => x.User.Id != userId))
                    return TaskResult.FromError("User isn't banned");

                await guild.RemoveBanAsync(userId);
                
                if (_configService.Config.TempBannedUsers.Any(x => x.UserId == userId))
                {
                    _configService.Config.TempBannedUsers.RemoveWhere(x => x.UserId == userId);
                    _configService.SaveData();
                }
                
            }
            catch (Exception e)
            {
                return TaskResult.FromError(e.Message);
            }
            
            UserUnbanned?.Invoke(userId, moderator, guild);
            return TaskResult.FromSuccess();
        }

        public async Task<TaskResult> MuteUser(SocketGuildUser user, SocketGuildUser moderator, TimeSpan muteTime, string reason)
        {
            if (_configService.Config.MutedRole == 0)
                return TaskResult.FromError("No Muted-role has been assigned in the config");

            if (_configService.Config.MutedUsers.Select(x => x.UserId).Contains(user.Id))
            {
                await user.AddRoleAsync(user.Guild.GetRole(_configService.Config.MutedRole));
                
                var entry = _configService.Config.MutedUsers.FirstOrDefault(x => x.UserId == user.Id);
                entry.Reason = reason;
                entry.ExpireTime = DateTimeOffset.Now.AddSeconds(muteTime.TotalSeconds).ToUnixTimeSeconds();
                
                _configService.SaveData();
            }
            else if (muteTime != TimeSpan.Zero)
            {
                if (user.Roles.Any(x => x.Id == _configService.Config.MutedRole))
                    return TaskResult.FromError("User is already muted");
                
                await user.AddRoleAsync(user.Guild.GetRole(_configService.Config.MutedRole));
                
                _configService.Config.MutedUsers.Add(new()
                {
                    ExpireTime = DateTimeOffset.Now.AddSeconds(muteTime.TotalSeconds).ToUnixTimeSeconds(),
                    ServerId = moderator.Guild.Id,
                    UserId = user.Id,
                    Reason = reason
                });
                _configService.SaveData();
            }
            else if (muteTime == TimeSpan.Zero)
            {
                await user.AddRoleAsync(user.Guild.GetRole(_configService.Config.MutedRole));
            }
            
            UserMuted?.Invoke(user, moderator, moderator.Guild, reason, muteTime);
            
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
                        Value = muteTime != TimeSpan.Zero ? muteTime.ToString("g") : "Indefinitely",
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
                        Title = $"You have been muted in tModLoader 64 bit",
                        Fields = fields,
                        Color = Color.DarkRed,
                        Timestamp = DateTimeOffset.Now
                    };

                    await user.SendMessageAsync(embed:embed.Build());
                }
            }
            catch (Exception) { /* Ignore -- Happens when unable to dm user*/ }
            
            return TaskResult.FromSuccess();
        }

        public async Task<TaskResult> UnMuteUser(SocketGuildUser user, SocketGuildUser moderator)
        {
            try
            {
                if (_configService.Config.MutedRole == 0)
                    return TaskResult.FromError("No Muted-role has been assigned");

                if (_configService.Config.MutedUsers.Any(x => x.UserId == user.Id))
                {
                    _configService.Config.MutedUsers.RemoveWhere(x => x.UserId == user.Id);
                    _configService.SaveData();
                }
                
                if (user.Roles.Any(x => x.Id == _configService.Config.MutedRole))
                    await user.RemoveRoleAsync(user.Guild.GetRole(_configService.Config.MutedRole));
                else
                    return TaskResult.FromError("User wasn't muted");
            }
            catch (Exception e)
            {
                return TaskResult.FromError(e.Message);
            }

            UserUnMuted?.Invoke(user, moderator, moderator.Guild, null, TimeSpan.Zero);
            return TaskResult.FromSuccess();
        }

        public async Task<TaskResult> TempBanUser(IUser user, SocketGuildUser moderator, TimeSpan banTime, string reason)
        {
            if (_configService.Config.TempBannedUsers.Any(x => x.UserId == user.Id))
                return TaskResult.FromError("User is already temp-banned");

            try
            {
                var bans = moderator.Guild.GetBansAsync();

                // User is banned but not temp banned; Convert to tempban then
                if (bans.Result.Any(x => x.User.Id == user.Id) && _configService.Config.TempBannedUsers.All(x => x.UserId != user.Id))
                {
                    _configService.Config.TempBannedUsers.Add(new TempBannedUser()
                    {
                        UserId = user.Id,
                        ServerId = moderator.Guild.Id,
                        Reason = reason,
                        ExpireTime = DateTimeOffset.Now.AddSeconds(banTime.TotalSeconds).ToUnixTimeSeconds()
                    });
                    _configService.SaveData();
                    
                    return TaskResult.FromSuccess();
                }

                if (bans.Result.All(x => x.User.Id != user.Id) && _configService.Config.TempBannedUsers.All(x => x.UserId != user.Id))
                {
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

                            if (_configService.Config.MessageWithModerator)
                            {
                                fields.Add(new()
                                {
                                    Name = "Responsible Moderator",
                                    Value = moderator.ToString(),
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
                    
                    // TODO: Add config option for prune days
                    await moderator.Guild.AddBanAsync(user, 0, $"{reason}\nBanned by {moderator}");
                    UserTempBanned?.Invoke(user, moderator, moderator.Guild, banTime, reason);
                    
                    _configService.Config.TempBannedUsers.Add(new TempBannedUser()
                    {
                        UserId = user.Id,
                        ServerId = moderator.Guild.Id,
                        Reason = reason,
                        ExpireTime = DateTimeOffset.Now.AddSeconds(banTime.TotalSeconds).ToUnixTimeSeconds()
                    });
                    _configService.SaveData();

                    
                    return TaskResult.FromSuccess();
                }
            }
            catch (Exception e)
            {
                return TaskResult.FromError(e.Message);
            }
            
            return TaskResult.FromSuccess();
        }

        public async Task<TaskResult> KickUser(SocketGuildUser user, SocketGuildUser moderator, string reason, string moderationLogReason = "")
        {
            try { await user.KickAsync(reason); }
            catch (Exception e) { return TaskResult.FromError(e.Message); }
            
            UserKicked?.Invoke(user, moderator, moderator.Guild, reason, moderationLogReason);
            
            if (_configService.Config.MessageOnKick)
            {
                EmbedBuilder embed = new EmbedBuilder();

                embed.WithTitle("You have been kicked from tModLoader 64 bit");
                embed.WithCurrentTimestamp();
                embed.WithColor(Color.Red);

                embed.AddField(new EmbedFieldBuilder
                {
                    Name = "Reason",
                    Value = reason,
                    IsInline = true
                });
                if (_configService.Config.MessageWithModerator)
                {
                    embed.AddField(new EmbedFieldBuilder
                    {
                        Name = "Responsible Moderator",
                        Value = moderator.ToString(),
                        IsInline = true
                    });
                }

                try { await user.SendMessageAsync(embed: embed.Build()); }
                catch (Exception e) { /* Ignore */ }
            }
            
            return TaskResult.FromSuccess();
        }
    }
}