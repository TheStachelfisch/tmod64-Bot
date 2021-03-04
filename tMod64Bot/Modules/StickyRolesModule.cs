using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using tMod64Bot.Modules.Commons;
using tMod64Bot.Utils;

namespace tMod64Bot.Modules
{
    [RequireBotManager]
    [Group("sticky"), Alias("stickyRoles")]
    public class StickyRolesModule : CommandBase
    {
        [Command("remove")]
        public async Task RemoveRole(SocketRole role)
        {
            Embed embed;

            if (!ConfigService.Config.StickiedRoles.Contains(role.Id))
                embed = EmbedHelper.ErrorEmbed($"Stickied Roles doesn't contain {role.Mention}");
            else
            {
                ConfigService.Config.StickiedRoles.Remove(role.Id);
                ConfigService.SaveData();
                embed = EmbedHelper.SuccessEmbed($"Successfully removed {role.Mention} from the stickied roles");
            }

            await ReplyAsync(embed: embed);
        }

        [Command("add")]
        public async Task AddRole(SocketRole role)
        {
            Embed embed;

            if (role.IsEveryone)
            {
                embed = EmbedHelper.ErrorEmbed($"Can't add {role.Mention}");
                // Sudden jumps in code, hell yeah
                goto A;
            }
            
            if (ConfigService.Config.StickiedRoles.Contains(role.Id))
                embed = EmbedHelper.ErrorEmbed($"Stickied Roles already contains {role.Mention}");
            else
            {
                ConfigService.Config.StickiedRoles.Add(role.Id);
                ConfigService.SaveData();
                embed = EmbedHelper.SuccessEmbed($"Successfully added {role.Mention} to the stickied roles");
            }

            A:
            
            await ReplyAsync(embed: embed);
        }
        
        [Command("values"), Alias("roles")]
        public async Task GetValues(bool mention = true)
        {
            string s = "";
            
            foreach (var configStickiedRole in ConfigService.Config.StickiedRoles)
                s += $"{Context.Guild.GetRole(configStickiedRole).Mention} - ID: {configStickiedRole}\n";

            Embed embed = new EmbedBuilder
            {       
                Title = "Stickied Roles",
                Color = Color.DarkGreen,
                Description = $"{s}"
            }.Build();

            await ReplyAsync(embed: embed);
        }

        [Command("remove")]
        public async Task RemoveUser(SocketGuildUser user)
        {
            Embed embed;

            if (!ConfigService.Config.StickiedUsers.Keys.ToList().Contains(user.Id))
                embed = EmbedHelper.ErrorEmbed($"{user.Mention} doesn't seem to be stickied");
            else
            {
                ConfigService.Config.StickiedUsers.Remove(user.Id);
                ConfigService.SaveData();
                embed = EmbedHelper.SuccessEmbed($"Successfully removed {user.Mention} from the stickied users");
            }

            await ReplyAsync(embed: embed);
        }
    }
}