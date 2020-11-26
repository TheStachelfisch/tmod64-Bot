using System.IO;

namespace tMod64Bot
{
    internal static class Program
    {
        public static readonly string GatewayToken = File.ReadAllText(@"token.txt");

        public static void Main(string[] args) => new tMod64bot().StartAsync().GetAwaiter().GetResult();
    }
}
