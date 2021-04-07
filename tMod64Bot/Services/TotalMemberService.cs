using System;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using tMod64Bot.Services.Commons;

namespace tMod64Bot.Services
{
    public class TotalMemberService : ServiceBase, IDisposable, IInitializeable
    {
        private Timer timer;
        
        public int GetTotalMembers()
        {
            int temp = 0;
            foreach (var clientGuild in Client.Guilds)
                temp = temp + clientGuild.MemberCount;

            return temp;
        }
        
        public TotalMemberService(IServiceProvider services) : base(services)
        {
            //First time setup
            Client.SetGameAsync($"{GetTotalMembers()} Members", type:ActivityType.Watching);
            
            timer = new();
            timer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
        }

        public async Task Initialize()
        {
            timer.Elapsed += UpdateStatus;
            timer.Start();
        }

        private void UpdateStatus(object sender, ElapsedEventArgs e)
        {
            Client.SetGameAsync($"{GetTotalMembers()} Members", type:ActivityType.Watching);
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}