using System.Xml.Schema;
using Discord;

namespace tMod64Bot.Utils
{
    public class EmbedHelper
    {
        public static Embed SuccessEmbed(string description, string footer = "")
        {
            var successEmbed = new EmbedBuilder();

            successEmbed.WithTitle("Success!");
            successEmbed.WithColor(Color.Green);
            successEmbed.WithDescription(description);
            successEmbed.WithFooter(footer);
            successEmbed.WithCurrentTimestamp();
            
            return successEmbed.Build();
        }

        public static Embed ErrorEmbed(string description, string footer = "")
        {
            var successEmbed = new EmbedBuilder();

            successEmbed.WithTitle("Error!");
            successEmbed.WithColor(Color.Red);
            successEmbed.WithDescription(description);
            successEmbed.WithFooter(footer);
            successEmbed.WithCurrentTimestamp();
            
            return successEmbed.Build();
        }
    }
}