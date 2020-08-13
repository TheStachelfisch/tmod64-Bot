namespace tMod64Bot.Modules.ConfigSystem
{
    //This eliminates the need for multiple TagGet... Methods
    // TagChange and shit methods are still needed...
    public enum ConfigEnum
    {
        BotPrefix,
        GuildId,
        BotManagerRole,
        BotOwner,
        LoggingChannel,
        ModLogChannel,
        AdminChannel,
        AdminRole,
        MutedRole,
        SoftbanRole,
        SupportStaffRole
    }
}