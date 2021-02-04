using System;
using System.IO;

namespace tMod64Bot
{
    public static class StringUtils
    {
#if DEBUG
        public static string SourceFileName(string file) => Path.GetFullPath($@"{Environment.CurrentDirectory}\..\..\..\{file}");
#endif
        public static bool IsNullOrWhitespace(this string str) => string.IsNullOrWhiteSpace(str);
    }
}
