using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace tMod64Bot.Modules.ConfigSystem
{
    [Group("config")]
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task Config()
        {
            var messageEmbed = new EmbedBuilder();

            messageEmbed.WithTitle("Documentation for Config");
            messageEmbed.WithUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            messageEmbed.WithFooter("Sent at ");
            messageEmbed.WithCurrentTimestamp();

            await ReplyAsync("", false, messageEmbed.Build());
        }

        [Command("all")]
        [Alias("get")]
        public async Task ConfigAll()
        {
            var rss = JObject.Parse(ConfigService.GetJsonData());

            var configEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                configEmbed.WithColor(Color.Green);
                configEmbed.WithTitle("Config");
                configEmbed.WithDescription("Values with **'0'** or **'null'** have not been added yet.");

                foreach (var property in rss.Properties())
                    configEmbed.AddField(property.Name, property.Value);

                await ReplyAsync("", false, configEmbed.Build());
            }
            else
            {
                configEmbed.WithTitle("Error!");
                configEmbed.WithDescription("Missing Bot Manager permissions");
                configEmbed.WithColor(Color.Red);
                configEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, configEmbed.Build());
            }
        }

        [Command("BotPrefix")]
        [Alias("prefix")]
        public async Task ConfigBotPrefix(string prefix)
        {
            var successEmbed = new EmbedBuilder();
            var errorEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                await ConfigService.UpdateBotPrefix(prefix);

                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription($"Bot Prefix has successfully been changed to '**{prefix}**'");
                successEmbed.WithColor(Color.Green);
                successEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription("Missing Bot Manager permissions");
                errorEmbed.WithColor(Color.Red);
                errorEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, errorEmbed.Build());
            }
        }

        [Command("BotManagerRole")]
        [Alias("BotManager")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigBotManagerRole(long newId)
        {
            var successEmbed = new EmbedBuilder();

            await ConfigService.UpdateBotManager(newId);

            successEmbed.WithTitle("Success!");
            successEmbed.WithDescription($"Bot Manager has successfully been changed to '**{newId}**'");
            successEmbed.WithColor(Color.Green);
            successEmbed.WithCurrentTimestamp();

            await ReplyAsync("", false, successEmbed.Build());
        }

        [Command("LoggingChannel")]
        [Alias("Logging")]
        public async Task ConfigLoggingChannel(long newChannelId)
        {
            var successEmbed = new EmbedBuilder();
            var errorEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                await ConfigService.UpdateLoggingChannel(newChannelId);

                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription($"Logging channel has successfully been changed to '**{newChannelId}**'");
                successEmbed.WithColor(Color.Green);
                successEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription("Missing Bot Manager permissions");
                errorEmbed.WithColor(Color.Red);
                errorEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, errorEmbed.Build());
            }
        }

        [Command("ModLoggingChannel")]
        [Alias("ModLogging")]
        public async Task ConfigModLoggingChannel(long newChannelId)
        {
            var successEmbed = new EmbedBuilder();
            var errorEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                await ConfigService.UpdateModLoggingChannel(newChannelId);

                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription(
                    $"Mod Logging channel has successfully been changed to '**{newChannelId}**'");
                successEmbed.WithColor(Color.Green);
                successEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription("Missing Bot Manager permissions");
                errorEmbed.WithColor(Color.Red);
                errorEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, errorEmbed.Build());
            }
        }

        [Command("AdminChannel")]
        [Alias("Admin")]
        public async Task ConfigAdminChannel(long newChannelId)
        {
            var successEmbed = new EmbedBuilder();
            var errorEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                await ConfigService.UpdateAdminChannel(newChannelId);

                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription($"Admin channel has successfully been changed to '**{newChannelId}**'");
                successEmbed.WithColor(Color.Green);
                successEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription("Missing Bot Manager permissions");
                errorEmbed.WithColor(Color.Red);
                errorEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, errorEmbed.Build());
            }
        }

        [Command("AdminRole")]
        public async Task ConfigAdminRole(long newChannelId)
        {
            var successEmbed = new EmbedBuilder();
            var errorEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                await ConfigService.UpdateAdminRole(newChannelId);

                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription($"Admin role has successfully been changed to '**{newChannelId}**'");
                successEmbed.WithColor(Color.Green);
                successEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription("Missing Bot Manager permissions");
                errorEmbed.WithColor(Color.Red);
                errorEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, errorEmbed.Build());
            }
        }

        [Command("MutedRole")]
        [Alias("Muted")]
        public async Task ConfigMutedRole(long newChannelId)
        {
            var successEmbed = new EmbedBuilder();
            var errorEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                await ConfigService.UpdateMutedRole(newChannelId);

                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription($"Muted role has successfully been changed to '**{newChannelId}**'");
                successEmbed.WithColor(Color.Green);
                successEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription("Missing Bot Manager permissions");
                errorEmbed.WithColor(Color.Red);
                errorEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, errorEmbed.Build());
            }
        }

        [Command("SoftbanRole")]
        [Alias("Softban")]
        public async Task ConfigSoftbanRole(long newChannelId)
        {
            var successEmbed = new EmbedBuilder();
            var errorEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                await ConfigService.UpdateSoftbanRole(newChannelId);

                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription($"Softban role has successfully been changed to '**{newChannelId}**'");
                successEmbed.WithColor(Color.Green);
                successEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription("Missing Bot Manager permissions");
                errorEmbed.WithColor(Color.Red);
                errorEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, errorEmbed.Build());
            }
        }

        [Command("SupportStaffRole")]
        [Alias("SupportStaff", "Support")]
        public async Task ConfigSupportRole(long newChannelId)
        {
            var successEmbed = new EmbedBuilder();
            var errorEmbed = new EmbedBuilder();

            var user = Context.User as SocketGuildUser;

            var role = Context.Guild.GetRole(ulong.Parse(ConfigService.GetConfig(ConfigEnum.BotManagerRole)?.ToString()));

            if (user.Roles.Contains(role) || user.GuildPermissions.Administrator)
            {
                await ConfigService.UpdateSupportStaffRole(newChannelId);

                successEmbed.WithTitle("Success!");
                successEmbed.WithDescription($"Support staff role has successfully been changed to '**{newChannelId}**'");
                successEmbed.WithColor(Color.Green);
                successEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, successEmbed.Build());
            }
            else
            {
                errorEmbed.WithTitle("Error!");
                errorEmbed.WithDescription("Missing Bot Manager permissions");
                errorEmbed.WithColor(Color.Red);
                errorEmbed.WithCurrentTimestamp();

                await ReplyAsync("", false, errorEmbed.Build());
            }
        }
    }
}