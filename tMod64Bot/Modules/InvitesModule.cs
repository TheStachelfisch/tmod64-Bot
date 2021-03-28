using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Modules.Commons;
using tMod64Bot.Services;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    [RequireBotManager]
    [Group("Invites")]
    public class InvitesModule : CommandBase
    {
        [Command("list"), Alias("all")]
        public async Task ListWhiteListedInvites()
        {
            Embed embed = new EmbedBuilder()
            {
                Title = "Exempt Invites",
                Description = String.Join(',', ConfigService.Config.ExemptInvites)
            }.WithCurrentTimestamp().Build();

            await ReplyAsync(embed: embed);
        }

        [Command("add")]
        public async Task AddInvite(string invite)
        {
            var inviteService = Services.GetRequiredService<InviteProtectionService>();

            if (!inviteService.ContainsInvite(invite).Success)
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed("Doesn't seem to be a valid invite"));
                return;
            }

            var id = inviteService.GetInviteId(invite);

            if (!id.Success)
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed("Doesn't seem to be a valid invite"));
                return;
            }

            if (ConfigService.Config.ExemptInvites.Contains(id.Value))
            {
                await ReplyAsync(embed: EmbedHelper.ErrorEmbed($"Exempt invites already contains {id}"));
                return;
            }
            
            ConfigService.Config.ExemptInvites.Add(id.Value);
            ConfigService.SaveData();
            await ReplyAsync(embed:EmbedHelper.SuccessEmbed($"Successfully added {id.Value} to the exempt invites list"));
        }

        [Command("remove")]
        public async Task RemoveInvite(string invite)
        {
            var inviteService = Services.GetRequiredService<InviteProtectionService>();

            var id = inviteService.GetInviteId(invite);

            if (inviteService.GetInviteId(invite).Success && ConfigService.Config.ExemptInvites.Contains(id.Value))
            {
                ConfigService.Config.ExemptInvites.Remove(id.Value);
                ConfigService.SaveData();
                await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully removed {id.Value} from the exempt invites list"));
                return;
            }

            if (ConfigService.Config.ExemptInvites.Contains(invite))
            {
                ConfigService.Config.ExemptInvites.Remove(invite);
                ConfigService.SaveData();
                await ReplyAsync(embed: EmbedHelper.SuccessEmbed($"Successfully removed {invite} from the exempt invites list"));
                return;
            }

            await ReplyAsync(embed: EmbedHelper.ErrorEmbed($"Error while removing invite, may not be a valid invite or list doesn't contain this invite"));
        }
    }
}