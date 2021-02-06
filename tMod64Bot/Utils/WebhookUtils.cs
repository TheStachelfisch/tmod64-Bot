using Discord;

namespace tMod64Bot.Utils
{
    public static class WebhookUtils
    {
        public static string GetUrl(this IWebhook webhook)
        {
            return $"https://discord.com/api/webhooks/{webhook.Id}/{webhook.Token}";
        }
    }
}