using System.Threading.Tasks;
using Discord.Commands;
using tMod64Bot.Modules.Commons;

namespace tMod64Bot.Modules
{
    [Group("bw"), Alias("badword")]
    [BotManagementPerms]
    public class BadWordModule : CommandBase
    {
        public async Task AddWord(string word)
        {
            
        }

        public async Task RemoveWord(string word)
        {
            
        }

        public async Task GetWords()
        {
            
        }
    }
}