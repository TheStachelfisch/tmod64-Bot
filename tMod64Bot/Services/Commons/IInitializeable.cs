using System.Threading.Tasks;

namespace tMod64Bot.Services.Commons
{
    public interface IInitializeable
    {
        public Task Initialize();
    }
}