using System.Diagnostics;

namespace tMod64Bot.Utils
{
    public static class ShellHelper
    {
        public static string ExecuteShell(this string cmd)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}