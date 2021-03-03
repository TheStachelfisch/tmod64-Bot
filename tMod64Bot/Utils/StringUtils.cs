using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace tMod64Bot
{
    public static class StringUtils
    {
        public static bool IsNullOrWhitespace(this string str) => string.IsNullOrWhiteSpace(str);

        /// <summary>
        /// Item1: Guild id, 0 if Dm channel
        /// Item2: Channel id
        /// Item3: Message id
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static Tuple<ulong, ulong, ulong>? DeconstructMessageLink(this string link)
        {
            Regex rx = new Regex(@"[\d]+",RegexOptions.Multiline);
            var matches = rx.Matches(link);

            if (matches.Count == 2)
                return new Tuple<ulong, ulong, ulong>(0, ulong.Parse(matches[0].Value), ulong.Parse(matches[1].Value));
            else if (matches.Count == 3)
                return new Tuple<ulong, ulong, ulong>(ulong.Parse(matches[0].Value), ulong.Parse(matches[1].Value), ulong.Parse(matches[2].Value));
            else
                return null;
        }
        public static bool ContainsAny(this string haystack, out string word, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle))
                {
                    word = needle;
                    return true;
                }
            }

            word = "";
            return false;
        }

        public static bool ContainsAny(this string haystack, params string[] needles) => ContainsAny(haystack, out string _, needles);
    }
}
