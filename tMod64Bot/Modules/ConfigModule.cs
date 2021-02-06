using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using tMod64Bot.Modules.Commons;
using tMod64Bot.Services.Config;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    [Group("config"), Alias("cfg")]
    [BotManagementPerms]
    public class ConfigModule : CommandBase
    {
        IEnumerable<FieldInfo> fields = typeof(Config).GetFields().Where(x => !x.FieldType.ToString().Contains("System.Collections.Generic"));
        
        [Command("change"), Alias("update")]
        public async Task ChangeValue(string key, string value)
        {
            try
            {
                var success = ConfigService.ChangeKey(key, value);
                
                if (!success)
                {
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed("The key you are trying to access doesn't exist"));
                    return;
                }
            }
            catch (Exception e)
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed($"{e.Message}\n**Note: You can only change Integer and String values in the config**"));
                return;
            }
            
            await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully changed the value of key '**{key}**' to '**{value}**'"));
        }

        [Command("fields")]
        public async Task GetFields()
        {
            var fieldEmbed = new EmbedBuilder();
            var fieldText = "```\n";

            foreach (var fieldInfo in fields)
            {
                if (Attribute.IsDefined(fieldInfo, typeof(SpaceAttribute)))
                    fieldText += "\n";
                
                fieldText += $"{fieldInfo.FieldType.Name, -25} {fieldInfo.Name}\n";
            }

            fieldText += "```";
            fieldText = fieldText.Replace(" ", "\u200b ");

            fieldEmbed.WithTitle("Config Fields");
            fieldEmbed.WithColor(Color.Blue);
            fieldEmbed.WithDescription(fieldText);

            await ReplyAsync(embed:fieldEmbed.Build());
        }
        
        [Command("values")]
        public async Task GetValues()
        {
            var fieldEmbed = new EmbedBuilder();
            var fieldValue = ConfigService.Config;
            var fieldText = "```\n";

            foreach (var fieldInfo in fields)
            {
                if (Attribute.IsDefined(fieldInfo, typeof(SpaceAttribute)))
                    fieldText += "\n";
                
                fieldText += $"{fieldInfo.Name, -25} {fieldInfo.GetValue(fieldValue)}\n";
            }

            fieldText += "```";
            fieldText = fieldText.Replace(" ", "\u200b ");

            fieldEmbed.WithTitle("Config Values");
            fieldEmbed.WithColor(Color.Blue);
            fieldEmbed.WithDescription(fieldText);

            await ReplyAsync(embed:fieldEmbed.Build());
        }
    }
}