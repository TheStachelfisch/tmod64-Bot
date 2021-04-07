using System;
using System.Text;

namespace tMod64Bot.Utils
{
    public static class TimeSpanUtils
    {
        public static string FormatString(this TimeSpan span, bool shortFormat)
        {
            StringBuilder builder = new();

            if (span == TimeSpan.Zero)
                throw new ArgumentNullException(nameof(span), "Timespan may not be Zero");

            if (span.Days != 0)
                builder.Append($"{(span.Days == 1 ? $"{span.Days}{(shortFormat ? "d" : " Day")}" : $"{span.Days}{(shortFormat ? "d" : " Days")}")} ");
            if (span.Hours != 0)
                builder.Append($"{(span.Hours == 1 ? $"{span.Hours}{(shortFormat ? "h" : " Hour")}" : $"{span.Hours}{(shortFormat ? "h" : " Hours")}")} ");
            if (span.Minutes != 0)
                builder.Append($"{(span.Minutes == 1 ? $"{span.Minutes}{(shortFormat ? "m" : " Minute")}" : $"{span.Minutes}{(shortFormat ? "m" : " Minutes")}")} ");
            if (span.Seconds != 0)
                builder.Append($"{(span.Seconds == 1 ? $"{span.Seconds}{(shortFormat ? "s" : " Second")}" : $"{span.Seconds}{(shortFormat ? "s" : " Seconds")}")} ");

            return builder.ToString();
        }
    }
}