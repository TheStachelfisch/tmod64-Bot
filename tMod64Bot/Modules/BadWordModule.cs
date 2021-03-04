using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using tMod64Bot.Modules.Commons;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    [Group("badword"), Alias("bw", "bannedword")]
    [RequireBotManager]
    public class BadWordModule : CommandBase
    {
        [Command("add")]
        public async Task AddWord([Remainder]string word)
        {
            if (ConfigService.Config.BannedWords.Contains(word))
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed($"Bad word list already contains {word}"));
                return;
            }

            ConfigService.Config.BannedWords.Add(word);
            ConfigService.SaveData();
            await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully added {word} to the banned word list"));
        }

        [Command("remove")]
        public async Task RemoveWord([Remainder]string word)
        {
            if (!ConfigService.Config.BannedWords.Contains(word))
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed($"Bad word list doesn't contain {word}"));
                return;
            }

            ConfigService.Config.BannedWords.Remove(word);
            ConfigService.SaveData();
            await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully removed '{word}' from the banned word list"));
        }

        [Command("words"), Alias("get", "list")]
        public async Task GetWords()
        {
            if (!ConfigService.Config.BannedWords.Any())
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed("No banned words have been added yet"));
                return;
            }

            EmbedBuilder embed = new()
            {
                Title = "Banned Words",
                Color = Color.Green,
                Description = $"**{ConfigService.Config.BannedWords.Count} {(ConfigService.Config.BannedWords.Count == 1 ? "word" : "words")}**\n```{String.Join(',', ConfigService.Config.BannedWords)}```",
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}