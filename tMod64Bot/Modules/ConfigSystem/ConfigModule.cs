using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace tMod64Bot.Modules.ConfigSystem
{
    [Group("config")]
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task Config()
        {
            EmbedBuilder messageEmbed = new EmbedBuilder();

            messageEmbed.WithTitle("Documentation for Config");
            messageEmbed.WithUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            messageEmbed.WithFooter("Sent at ");
            messageEmbed.WithCurrentTimestamp();

            await ReplyAsync("", false, messageEmbed.Build());
        }
        
        [Command("all"), Alias("get")]
        public async Task ConfigAll()
        {
            JObject rss = JObject.Parse(ConfigService.GetJsonData());
            
            EmbedBuilder configEmbed = new EmbedBuilder();

            configEmbed.WithColor(Color.Green);
            configEmbed.WithTitle("Config");
            configEmbed.WithDescription("Values with **'0'** or **'null'** have not been added yet.");
            
            foreach (var property in rss.Properties())
            {
                configEmbed.AddField(property.Name, property.Value);
            }

            await ReplyAsync("", false, configEmbed.Build());
        }
        
        [Command("BotManagerRole")]
        public async Task ConfigTest(long NewId)
        {
            EmbedBuilder successEmbed = new EmbedBuilder();

            await ConfigService.UpdateBotManager(NewId);

            successEmbed.WithTitle("Success!");
            successEmbed.WithDescription($"Bot Manager has successfully been changed to '**{NewId}**'");
            successEmbed.WithColor(Color.Green);
            successEmbed.WithCurrentTimestamp();
            
            await ReplyAsync("", false, successEmbed.Build());
        }
    }
}