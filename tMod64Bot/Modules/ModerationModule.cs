using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tMod64Bot.Services;

namespace tMod64Bot.Modules
{
    public class ModEventArgs : EventArgs
    {
        public IGuildUser OffendingUser { get; set; }
        public SocketUser ResponsibleModerator { get; set; }
        public string Reason { get; set; }
    }

    public class MuteEventArgs : ModEventArgs
    {
        public string Time { get; set; }
    }

    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        public ConfigService Config { get; set; }

        public delegate void KickEventHandler(object source, ModEventArgs args);
        public static event KickEventHandler UserKicked;

        public delegate void BanEventHandler(object source, ModEventArgs args);
        public static event BanEventHandler UserBanned;

        public delegate void SoftBanEventHandler(object source, ModEventArgs args);
        public static event SoftBanEventHandler UserSoftBanned;

        public delegate void MuteEventHandler(object source, MuteEventArgs args);

        public static event MuteEventHandler UserMuted;

        [Command("kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [Summary("Kicks a user mentioned.")]
        public async Task KickAsync(IGuildUser user, [Remainder] string reason = "No reason specified.")
        {
            var victim = user as SocketGuildUser;
            var author = Context.User as SocketGuildUser;

            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("Just leave");
                return;
            }

            if (victim.Hierarchy >= author.Hierarchy)
                await ReplyAsync("You are not high enough in the Role Hierarchy to do that");
            else
            {
                await user.KickAsync(reason);
                UserKicked?.Invoke(this, new ModEventArgs() { OffendingUser = user, ResponsibleModerator = Context.User, Reason = reason });
                await ReplyAsync("User " + user.Username + " has been kicked. Reason: " + reason);
            }
        }

        [Command("ban"), Alias("b")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Summary("Bans a user mentioned.")]
        public async Task BanAsync(IGuildUser user, [Remainder] string reason = "No reason specified.")
        {
            var victim = user as SocketGuildUser;
            var author = Context.User as SocketGuildUser;

            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("Just leave");
                return;
            }

            if (victim.Hierarchy >= author.Hierarchy)
                await ReplyAsync("You are not high enough in the Role Hierarchy to do that");
            else
            {
                await user.BanAsync(0, reason);
                UserBanned?.Invoke(this, new ModEventArgs() { OffendingUser = user, ResponsibleModerator = Context.User, Reason = reason });
                await ReplyAsync("User " + user.Username + " has been banned. Reason: " + reason);
            }
        }

        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Adds a muted role onto the user.")]
        public async Task MuteAsync(IGuildUser user, string time, [Remainder] string reason = "No reason specified.") //TODO: temporary unmute, time to unmute
        {
            await user.AddRoleAsync(Context.Client.GetGuild(Context.Guild.Id).GetRole(Config.MutedRoleId));
            UserMuted?.Invoke(this, new MuteEventArgs() { OffendingUser = user, ResponsibleModerator = Context.User, Reason = reason, Time = time });
            IUserMessage MessageToDelete = await ReplyAsync($"User {user.Username} was muted for {time}, Reason: {reason}");
            await Task.Delay(2500);
            await MessageToDelete.DeleteAsync();
        }

        [Command("softban"), Alias("sb")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Summary("Banishes a user to the shadow realm!")]
        public async Task SBanAsync(IGuildUser user, [Remainder] string reason = "No reason specified.")
        {
            var victim = user as SocketGuildUser;
            var author = Context.User as SocketGuildUser;

            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("Just leave");
                return;
            }

            if (victim.Hierarchy >= author.Hierarchy)
                await ReplyAsync("You are not high enough in the Role Hierarchy to do that");
            else
            {
                await user.AddRoleAsync(Context.Client.GetGuild(Context.Guild.Id).GetRole(Config.SoftbanRoleId));
                UserSoftBanned?.Invoke(this, new ModEventArgs() { OffendingUser = user, ResponsibleModerator = Context.User, Reason = reason });
                await ReplyAsync("User " + user.Username + " was banished to the Shadow Realm. Reason: " + reason);
            }
        }

        [Command("echo")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Makes the bot say something in the specified chat, anonymously")]
        public async Task EchoAsync(IGuildChannel channel, [Remainder] string message)
        {
            await Context.Guild.GetTextChannel(channel.Id).SendMessageAsync(message);
        }

        [Command("install")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task InstallAsync()
        {
            //IT WORKS! Don't touch my spaghetti
            
            List<EmbedFieldBuilder> installFields = new List<EmbedFieldBuilder>();
            List<EmbedFieldBuilder> steamFields = new List<EmbedFieldBuilder>();
            List<EmbedFieldBuilder> vulkanFields = new List<EmbedFieldBuilder>();
            List<EmbedFieldBuilder> exceptionFields = new List<EmbedFieldBuilder>();
            
            var before = new EmbedBuilder()
            {
                Title = "Before Installing!",
                Description = "Please make sure that\n" +
                              "- You have bought Terraria, [Steam Link](https://store.steampowered.com/app/105600/Terraria/)\n" +
                              "- You have installed Terraria 1.4 and that its unmodified, if it isn't then verify it by right clicking Terraria on Steam > Properties > Local files > Verify integrity of game files\n" +
                              "- You have [tModLoader](https://store.steampowered.com/app/1281930/tModLoader/) and [Terraria](https://store.steampowered.com/app/105600/Terraria/) installed\n" +
                              "- Both the tModLoader and Terraria folders are in the same sub folder, e.g: [steamapps/common](https://cdn.discordapp.com/attachments/745279550279254138/745279561528246413/unknown.png)",
                Footer = new EmbedFooterBuilder().WithText("Guide made by TheStachelfisch#0395"),
                Color = Color.Orange
            };

            installFields.Add(new EmbedFieldBuilder()
            {
                Name = "Download the tModLoader 64 bit files",
                Value = "Download the [tModLoader 64 bit files](https://github.com/Dradonhunter11/tModLoader/releases/). Download the top zip file, the top zip file should look something like [this](https://cdn.discordapp.com/attachments/745279550279254138/745281956622630922/unknown.png). Click on that zip to download it. Then let it finish downloading"
            });
            installFields.Add(new EmbedFieldBuilder()
            {
                Name = "Unpack the tModLoader 64 bit files",
                Value = "Unpack the tModLoader 64 bit files using a zip extractor like 7zip or using the standard extractor provided with Windows 10. Right click the .zip file and press ['Extract all'](https://cdn.discordapp.com/attachments/745279550279254138/745286539545935882/unknown.png). This will open a new window, click ['Extract'](https://cdn.discordapp.com/attachments/745279550279254138/745286837127479426/unknown.png)"
            });
            installFields.Add(new EmbedFieldBuilder()
            {
                Name = "Cut the files",
                Value = "Open the folder where you extracted the files, it should have the same name as the zip file. Select all the files by pressing Ctrl+A then right click on the file and press 'cut'"
            });
            installFields.Add(new EmbedFieldBuilder()
            {
                Name = "Paste the files",
                Value = "Open the tModLoader folder by opening steam and then searching for tModLoader. Right click tModLoader>Properties>Local Files>Browse local files. This will open the tModLoader folder. Right click anywhere in the folder and then press 'Paste'. If it asks you if you want to override things, click 'Replace Files in the destination'"
            });
            installFields.Add(new EmbedFieldBuilder()
            {
                Name = "How to start tModLoader",
                Value = "You can now either start tModLoader 64 bit directly by double clicking the tModLoader 64 bit .exe file or you can follow the guide under this one and add tModLoader 64 bit to your steam library"
            });
            
            var install = new EmbedBuilder()
            {
                Title = "Installation for tModLoader 64 bit",
                Fields = installFields,
                Footer = new EmbedFooterBuilder().WithText("Guide made by TheStachelfisch#0395"),
                Color = Color.Green
            };
            
            steamFields.Add(new EmbedFieldBuilder()
            {
                Name = "Click the 'Add a game' button",
                Value = "Open your Steam library and click on the 'Add a game' button in the bottom left corner of your library, if you are using a Steam skin that button might be located somewhere else. After clicking that button a new window should open"
            });
            steamFields.Add(new EmbedFieldBuilder()
            {
                Name = "Locate the tModLoader 64 bit .exe file",
                Value = "In the window click 'Browse...', after clicking that it should open a explorer window, in that explorer window you should already be in the normal Steam path. Now go to this path: 'steamapps/common/tModLoader' (If you installed tModLoader on a different drive/path this path might differ)."
            });
            steamFields.Add(new EmbedFieldBuilder()
            {
                Name = "Adding tModLoader 64 bit",
                Value = "In that folder double click on 'tModLoader64bit.exe'. After doing that the explorer window should close, the last thing you have to do is click 'Add selected programs' in the steam window."
            });
            
            var steamAdd = new EmbedBuilder()
            {
                Title = "Adding tModLoader 64 bit to your Steam library",
                Footer = new EmbedFooterBuilder().WithText("Guide made by TheStachelfisch#0395"),
                Fields = steamFields,
                Color = Color.DarkGreen
            };
            
            vulkanFields.Add(new EmbedFieldBuilder()
            {
                Name = "When should you use the Vulkan version",
                Value = "You should use the Vulkan version when you either have a AMD Gpu or your pc is pretty weak"
            });
            vulkanFields.Add(new EmbedFieldBuilder()
            {
                Name = "Before installing the Vulkan version",
                Value = "Make sure that you already installed tModLoader 64 bit with the guide above"
            });
            vulkanFields.Add(new EmbedFieldBuilder()
            {
                Name = "Download the Vulkan files",
                Value = "Download the [tModLoader 64 bit vulkan files](https://github.com/Dradonhunter11/tModLoader/releases/). The zip should look like [this](https://cdn.discordapp.com/attachments/745279550279254138/745307657392619571/unknown.png). Click on that zip file and let it finish downloading"
            });
            vulkanFields.Add(new EmbedFieldBuilder()
            {
                Name = "Unpack the Vulkan files",
                Value = "Unpack the Vulkan files using a zip extractor like 7zip or using the standard extractor provided with Windows 10. Right click the .zip file and press ['Extract all'](https://cdn.discordapp.com/attachments/745279550279254138/745286539545935882/unknown.png). This will open a new window, click ['Extract'](https://cdn.discordapp.com/attachments/745279550279254138/745286837127479426/unknown.png)"
            });
            vulkanFields.Add(new EmbedFieldBuilder()
            {
                Name = "Cut the files",
                Value = "Open the folder where you extracted the files, it should have the same name as the zip file. Select all the files by pressing Ctrl+A then right click on the file and press 'cut'"
            });
            vulkanFields.Add(new EmbedFieldBuilder()
            {
                Name = "Paste the files",
                Value = "Open the tModLoader folder by opening steam and then searching for tModLoader. Right click tModLoader>Properties>Local Files>Browse local files. This will open the tModLoader folder. Right click anywhere in the folder and then press 'Paste'. If it asks you if you want to override things, click 'Replace Files in the destination'"
            });
            
            var vulkanInstall = new EmbedBuilder()
            {
                Title = "How to install tModLoader 64 bit Vulkan",
                Footer = new EmbedFooterBuilder().WithText("Guide made by TheStachelfisch#0395"),
                Fields = vulkanFields,
                Color = Color.Blue
            };
            
            exceptionFields.Add(new EmbedFieldBuilder()
            {
                Name = "House banner missing",
                Value = "[This Error](https://cdn.discordapp.com/attachments/574597033135177733/744964709979258910/unknown.png)\nWhy this happens: This happens because you installed tModLoader 64 bit into the Terraria folder and not the tModLoader folder\nHow to Solve: Clean both the Terraria and tModLoader folder by deleting everything in them and then verifying the game through steam and then slowly follow the install guide."
            });
            exceptionFields.Add(new EmbedFieldBuilder()
            {
                Name = "Terraria Content folder/directory not found",
                Value = "Why this happens: This happens because tModLoader can't find the Terraria folder\nHow to solve: Move the Terraria or tModLoader folder into the steamapps/common folder or another folder so it looks like [this](https://cdn.discordapp.com/attachments/745279550279254138/745279561528246413/unknown.png)"
            });
            exceptionFields.Add(new EmbedFieldBuilder()
            {
                Name = "...must have a unmodified Terraria install",
                Value = "Why this happens: This happens due to how tModLoader detects Gog installs\nHow to solve: Clean both the Terraria and tModLoader folder by deleting everything in them and then verifying the game through steam and then slowly follow the install guide."
            });
            exceptionFields.Add(new EmbedFieldBuilder()
            {
                Name = "Resolution stuck at 800x600 when launching",
                Value = "Why this happens: There is no definitive reason to why this happens, it seems to be a problem with FNA\nHow to solve: The easiest way to solve this without any drawbacks is to just tab out and in after launching the game. If you do want a permanent solution, please follow [this](https://docs.google.com/document/d/183SkwLm14uItpn4wku-tEQJ_vGewj5O4wox5mBRMx1g/edit#bookmark=id.nc5ghga040mu)"
            });
            exceptionFields.Add(new EmbedFieldBuilder()
            {
                Name = "Side note",
                Value = "Thanks for reading my guide, if you find anything wrong with it or you want to improve anything just ping me(TheStachelfisch#0395) or dm me"
            });
            
            var exceptions = new EmbedBuilder()
            {
                Title = "Possible Exceptions/Errors on Startup",
                Footer = new EmbedFooterBuilder().WithText("Guide made by TheStachelfisch#0395"),
                Fields = exceptionFields,
                Color = Color.DarkRed
            };


            await ReplyAsync(embed: before.Build());
            await ReplyAsync(embed: install.Build());
            await ReplyAsync(embed: steamAdd.Build());
            await ReplyAsync(embed: vulkanInstall.Build());
            await ReplyAsync(embed: exceptions.Build());
        }
    }

    [Group("purge")]
    public class PurgeModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("purges a specified amount of messages")]
        public async Task PurgeMessageAsync(int amount)
        {
            //Embeds are pretty you know
            EmbedBuilder amountErrorEmbed = new EmbedBuilder();
            EmbedBuilder successEmbed = new EmbedBuilder();

            if (amount <= 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription("The amount of messages cant be zero or negative");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                await ReplyAsync("", false, amountErrorEmbed.Build());
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

            var filteredMessages = messages.Where(x => (DateTimeOffset.Now - x.Timestamp).TotalDays <= 14);

            var count = filteredMessages.Count();

            if (count == 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription("No messages found to delete");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                await ReplyAsync("", false, amountErrorEmbed.Build());
            }
            else
            {
                await (Context?.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                successEmbed.WithTitle($"Successfully deleted {filteredMessages.Count()} messages");
                successEmbed.WithColor(Color.DarkGreen);
                successEmbed.WithCurrentTimestamp();

                var embedMessage = await ReplyAsync("", false, successEmbed.Build());
                await Task.Delay(5000);

                await embedMessage.DeleteAsync();
                await Context.Message.DeleteAsync();
            }
        }

        [Command("user")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("purges a specified amount of messages from a specific user")]
        public async Task PurgeUserAsync(IGuildUser user, int messageCount)
        {
            EmbedBuilder amountErrorEmbed = new EmbedBuilder();
            EmbedBuilder successEmbed = new EmbedBuilder();

            if (messageCount <= 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription("The amount of messages cant be zero or negative");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                var embedMessage = await ReplyAsync("", false, amountErrorEmbed.Build());

                await Task.Delay(2500);

                await embedMessage.DeleteAsync();
                await Context.Message.DeleteAsync();
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, messageCount).FlattenAsync();

            var filteredMessages = messages.Where(x => (DateTimeOffset.Now - x.Timestamp).TotalDays <= 14).Where(u => u.Author.Id.Equals(user.Id));

            var count = filteredMessages.Count();

            if (count == 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription("No messages found to delete");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                var embedMessage = await ReplyAsync("", false, amountErrorEmbed.Build());

                await Task.Delay(2500);

                await embedMessage.DeleteAsync();
                await Context.Message.DeleteAsync();
            }
            else
            {
                await (Context?.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                successEmbed.WithTitle($"Successfully deleted {filteredMessages.Count()} messages from");
                successEmbed.WithDescription(MentionUtils.MentionUser(user.Id));
                successEmbed.WithColor(Color.DarkGreen);
                successEmbed.WithCurrentTimestamp();

                var embedMessage = await ReplyAsync("", false, successEmbed.Build());
                await Task.Delay(2500);

                await embedMessage.DeleteAsync();
                await Context.Message.DeleteAsync();
            }
        }

        [Command("contains"), Alias("contain")]
        [Summary("Deletes a specified amount of message that contain a certain string")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeContainAsync(int amount, string contain)
        {
            contain = contain.ToLower();

            EmbedBuilder amountErrorEmbed = new EmbedBuilder();
            EmbedBuilder successEmbed = new EmbedBuilder();

            if (amount <= 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription("The amount of messages cant be zero or negative");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                await ReplyAsync("", false, amountErrorEmbed.Build());
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

            var filteredMessages = messages.Where(x => (DateTimeOffset.Now - x.Timestamp).TotalDays <= 14).Where(m => m.Content.ToLower().Contains(contain));

            var count = filteredMessages.Count();

            if (count == 0)
            {
                amountErrorEmbed.WithTitle("Error!");
                amountErrorEmbed.WithDescription($"No messages found that contained **'{contain}'***");
                amountErrorEmbed.WithColor(Color.Red);
                amountErrorEmbed.WithCurrentTimestamp();
                await ReplyAsync("", false, amountErrorEmbed.Build());
            }
            else
            {
                await (Context?.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                successEmbed.WithTitle($"Successfully deleted {filteredMessages.Count()} messages that contained '**{contain}**'");
                successEmbed.WithColor(Color.DarkGreen);
                successEmbed.WithCurrentTimestamp();

                var embedMessage = await ReplyAsync("", false, successEmbed.Build());
                await Task.Delay(2500);

                await embedMessage.DeleteAsync();
                await Context.Message.DeleteAsync();
            }
        }
    }
}