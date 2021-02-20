using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Config;

namespace tMod64Bot.Services
{
    public class ModerationService : ServiceBase
    {
        private Timer _timer;
        private readonly ConfigService _configService;
        
        public delegate void UserBannedEventHandler(SocketUser user, SocketGuild guild, string reason);
        public event UserBannedEventHandler UserBanned;
        
        public ModerationService(IServiceProvider services) : base(services)
        {
            _configService = services.GetRequiredService<ConfigService>();
            
            _timer = new();
            _timer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
        }

        public async Task InitializeAsync()
        {
            _timer.Elapsed += CheckIfExpired;
            _timer.Start();
        }

        private void CheckIfExpired(object sender, ElapsedEventArgs e)
        {
            if (_configService.Config.MutedRole == 0)
                return;
            
            var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            
            foreach (var mutedUser in _configService.Config.MutedUsers)
            {
                if (mutedUser.ExpireTime <= currentTime)
                {
                    var guild = Client.GetGuild(mutedUser.ServerId);

                    guild.GetUser(mutedUser.UserId).RemoveRoleAsync(guild.GetRole(_configService.Config.MutedRole));
                }
            }
        }

        public static double GetSeconds(string input)
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

            return temp.TotalSeconds;
        }
        
        public async Task BanUser(SocketGuildUser user, string reason)
        {
            await user.Guild.AddBanAsync(user, 0, reason);
            UserBanned.Invoke(user, user.Guild, reason);
        }
    }
}