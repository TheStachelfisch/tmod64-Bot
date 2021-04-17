using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using tMod64Bot.Services.Commons;
using tMod64Bot.Utils;

namespace tMod64Bot.Services
{
    public class LogService : ServiceBase, IInitializeable
    {
        public LogService(IServiceProvider services) : base(services)
        {
        }

        public Task Initialize()
        {
            Client.MessageReceived += HandleMessage;
            return Task.CompletedTask;
        }

        private async Task HandleMessage(SocketMessage message)
        {
            if (message is not SocketUserMessage || message.Author.IsBot || message.Author.IsWebhook)
                return;
            
            // ReSharper disable once PatternAlwaysOfType
            if (message.Attachments.Count == 1 && message.Attachments.ElementAt(0) is Attachment attachment)
            {
                if (attachment.Filename.EndsWith(".log"))
                {
                    if (attachment.Size < 80000)
                    {
                                            
                        string link;
                    
                        using (var client = new HttpClient())
                        {
                            var logContents = await client.GetStringAsync(attachment.Url);

                            var response = await client.PostAsync("https://paste.mod.gg/documents", new StringContent(logContents));

                            link = $"https://paste.mod.gg/{JObject.Parse(await response.Content.ReadAsStringAsync())["key"]!}";
                        }

                        Embed embed = new EmbedBuilder
                        {
                            Color = Color.Green,
                            Description = $"[Automatically generated paste]({link})",
                            Author = new EmbedAuthorBuilder
                            {
                                Name = $"Pastebin for {message.Author}"
                            }
                        }.WithCurrentTimestamp().Build();

                        await (message as IUserMessage).ReplyAsync(embed: embed, allowedMentions:AllowedMentions.None);   
                    }
                    else
                    {
                        await (message as IUserMessage).ReplyAsync(embed: EmbedHelper.ErrorEmbed("paste.mod.gg max file size is 80kb"), allowedMentions:AllowedMentions.None);
                    }
                }
            }
        }
    }
}