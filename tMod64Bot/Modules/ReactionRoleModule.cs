using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using tMod64Bot.Modules.Commons;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    [Group("rr"), Alias("ReactionMessage", "ReactionRole")]
    [RequireBotManager]
    public class ReactionRoleModule : CommandBase
    {
        [Command("add"), Alias("create")]
        public async Task AddReactionMessage(string messageLink, string emote, IRole role)
        {
            var ids = messageLink.DeconstructMessageLink();
            ITextChannel? channel;

            try
            {
                channel = Context.Guild.GetChannel(ids.Item2) as ITextChannel;
            }
            catch (Exception e)
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed("Invalid message link"));
                return;
            }

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

            ConfigService.Config.ReactionRoleMessages.Add(
                new Tuple<ulong, ulong, string>(ids.Item3, role.Id, new Emoji(emote).Name));
            ConfigService.SaveData();

            await ReplyAsync(embed: EmbedHelper.SuccessEmbed(
                $"Successfully added {ids.Item3} to the watched reaction roles.\nOn reaction added {role.Mention} will be assigned"));
        }
        
        [Command("remove"), Alias("delete")]
        public async Task RemoveReactionMessage(string messageLink)
        {
            var ids = messageLink.DeconstructMessageLink();
            if (ids == null)
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed("Invalid message link"));
                return;
            }

            if (!ConfigService.Config.ReactionRoleMessages.Select(x => x.Item1).Contains(ids.Item3))
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed("Message isn't a Reaction-role message"));
                return;
            }
            ConfigService.Config.ReactionRoleMessages.RemoveWhere(i => i.Item1.Equals(ids.Item3));
            ConfigService.SaveData();
            await ReplyAsync(embed: EmbedHelper.SuccessEmbed(
                $"Successfully removed this message from the reaction role messages"));
        }

        [Command("messages"), Alias("values")]
        public async Task GetReactionMessages()
        {
            Embed embed;

            if (ConfigService.Config.ReactionRoleMessages.Count == 0)
            {
                embed = new EmbedBuilder
                {
                    Title = "Reaction-role messages",
                    Color = Color.DarkGreen,
                    Description = "No reaction-role messages"
                }.Build();
            }
            else
            {
                string temp = "";
                foreach (var reactionRoleMessage in ConfigService.Config.ReactionRoleMessages)
                {
                    temp += $"{reactionRoleMessage.Item1} : {MentionUtils.MentionRole(reactionRoleMessage.Item2)}\n";
                }
                
                embed = new EmbedBuilder
                {
                    Title = "Reaction-role messages",
                    Color = Color.DarkGreen,
                    Description = temp
                }.Build();
            }

            await ReplyAsync(embed: embed);
        }
    }
}