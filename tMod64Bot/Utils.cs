using Discord.WebSocket;
using System;
using System.IO;

namespace tMod64Bot
{
    public static class Utils
    {
        

        // Retrieves a file from the actual top level source
        public static string SourceFileName(string file) => Path.GetFullPath($@"{Environment.CurrentDirectory}\..\..\..\{file}");

        public static bool IsNullOrWhitespace(this string str) => string.IsNullOrWhiteSpace(str);
    }
}
