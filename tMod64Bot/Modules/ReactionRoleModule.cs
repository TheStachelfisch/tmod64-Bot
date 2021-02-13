using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using tMod64Bot.Modules.Commons;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    [Group("rr"), Alias("ReactionMessage", "ReactionRole")]
    [BotManagementPerms]
    public class ReactionRoleModule : CommandBase
    {
        [Command("add"), Alias("create")]
        public async Task AddReactionMessage(string messageLink, string emote, IRole role)
        {
            try
            {
                var ids = messageLink.DeconstructMessageLink();

                await LoggingService.Log($"\nItem2: {ids.Item3}");
                
                var channel = Context.Guild.GetChannel(ids.Item2) as ITextChannel;
            
                if (channel == null)
                {
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed("Channel not found"));
                    return;
                }
            
                var message = await channel.GetMessageAsync(ids.Item3);

                if (message == null)
                {
                    await ReplyAsync(embed: EmbedHelper.ErrorEmbed("Message not found"));
                    return;
                }

                if (!message.Reactions.Any(x => Equals(new Emoji(emote))))
                    await message.AddReactionAsync(new Emoji(emote));

                ConfigService.Config.ReactionRoleMessages.Add(new Tuple<ulong, ulong, string>(ids.Item3, role.Id, new Emoji(emote).Name));
                ConfigService.SaveData();

                await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully added {ids.Item3} to the watched reaction roles.\nOn reaction added {role.Mention} will be assigned"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}