using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace tMod64Bot.Handler
{
    public class InviteHandler
    {
        private DiscordSocketClient _client;
        
        public InviteHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += OnMessageReceive;
        }

        //TODO: Check if user has role that allows to send invite
        private async Task OnMessageReceive(SocketMessage arg)
        {
            EmbedBuilder dmEmbed = new EmbedBuilder();
            
            if (MessageContainsInvite(arg.Content))
            {
                dmEmbed.WithTitle("Your message was Deleted");
                dmEmbed.WithDescription($"Your message was deleted because it may have contained a Invite\n\n{arg.Content}");
                dmEmbed.WithColor(Color.Red);
                dmEmbed.WithCurrentTimestamp();
                dmEmbed.WithFooter("This message was sent by a bot");
                
                await arg.DeleteAsync();
                
                await arg.Author.SendMessageAsync("", false, dmEmbed.Build());
            }
        }

        private static bool MessageContainsInvite(string message)
        {
            if (message.Contains("discord.gg/") || message.Contains("https://discord.gg/") || message.Contains("discord.com/invite/")) return true;

            return false;
        }
            
    }
}