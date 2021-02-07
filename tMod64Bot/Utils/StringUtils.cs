using System;
using System.IO;

namespace tMod64Bot
{
    public static class StringUtils
    {
        public static bool IsNullOrWhitespace(this string str) => string.IsNullOrWhiteSpace(str);
    }
}
