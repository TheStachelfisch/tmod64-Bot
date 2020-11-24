using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using tMod64Bot.Modules.ConfigSystem;

namespace tMod64Bot.Handler
{
    public class BadWordHandler
    {
        private const string FileName = "badWords.json";
        private static readonly string FullPath = Path.GetFullPath($"{Environment.CurrentDirectory + @"\..\..\..\"}{FileName}");

        private static List<string> _badWords = new List<string>();

        private DiscordSocketClient _client;

        public BadWordHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += OnMessage;

            _badWords = JsonConvert.DeserializeObject<List<string>>(GetJson());
        }

        private string GetJson() => File.ReadAllText(FullPath);

        private static async Task WriteJsonData(string jsonData)
        {
            await File.WriteAllTextAsync(FullPath, jsonData);
        }

        public async static Task<bool> AddBadWord(string word)
        {
            word = word.ToLower();

            if (!ContainsWord(word))
            {
                try
                {
                    _badWords.Add(word.ToLower());

                    string json = JsonConvert.SerializeObject(_badWords, Formatting.Indented);
                    await WriteJsonData(json);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            return false;
        }

        public static List<string> GetList() => _badWords;

        public static async Task<bool> RemoveBadWord(string word)
        {
            word = word.ToLower();

            if (ContainsWord(word))
            {
                try
                {
                    _badWords.Remove(word);

                    string json = JsonConvert.SerializeObject(_badWords, Formatting.Indented);
                    await WriteJsonData(json);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            return false;
        }

        public static bool ContainsWord(string word)
        {
            word = word.ToLower();

            foreach (var words in _badWords)
            {
                if (_badWords.Contains(word))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool StringContainsWord(string text)
        {
            text = text.ToLower();

            foreach (var words in _badWords)
            {
                if (text.Contains(words.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task OnMessage(SocketMessage arg)
        {
            if (ConfigService.GetBadWordChannelWhitelist().Contains(arg.Channel.Id))
                return;

            var botManagerRole = _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString())).GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));
            var supportStaffRole = _client.GetGuild(ulong.Parse(ConfigService.GetConfig(ConfigEnum.GuildId)?.ToString())).GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.SupportStaffRole)?.ToString()));

            var user = arg.Author as SocketGuildUser;

            if (!arg.Author.IsWebhook && !arg.Author.IsBot)
            {
                //It somehow doesn't work the other way around
                if (user.Roles.Contains(botManagerRole) || user.Roles.Contains(supportStaffRole) || user.GuildPermissions.ManageMessages)
                {
                    return;
                }
                else
                {
                    if (arg.Embeds.Count() == 0 && StringContainsWord(arg.Content))
                    {
                        try
                        {
                            await arg.DeleteAsync();
                        }
                        // This error is thrown when a message contains multiple banned words
                        catch (HttpException e)
                        {
                        }
                    }
                }
            }
        }
    }
}