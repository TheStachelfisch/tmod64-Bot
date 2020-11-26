using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace tMod64Bot.Modules
{
    public sealed class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Alias("h")]
        [Summary("Help command with all commands")]
        public async Task Help()
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("The Command Documentation can be found here")
                .WithDescription($"[Command Documentation](https://github.com/TheStachelfisch/tmod64-Bot/tree/master/CommandDocumentation/)\n\nIf you experience any problems with the bot, please ping {MentionUtils.MentionUser(442639987180306432)}")
                .WithColor(Color.DarkGreen)
                .WithCurrentTimestamp()
                .WithFooter("This message was sent by a bot");

            await Context.User.SendMessageAsync("", false, embedBuilder.Build());

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }
    }
}