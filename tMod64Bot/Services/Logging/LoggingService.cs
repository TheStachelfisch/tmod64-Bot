using System;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace tMod64Bot.Services.Logging
{
    public sealed class LoggingService : ServiceBase
    {
        private readonly string _path = $"{ServiceConstants.DATA_DIR}log.txt";

        public LoggingService(IServiceProvider services) : base(services)
        {
            if (File.Exists(_path))
                File.Delete(_path);

            Client.Log += Log;
        }

        /// <summary>
        /// Logs a message to the Console
        /// </summary>
        /// <param name="severity">Severity of the Message</param>
        /// <param name="source">Source of the Message</param>
        /// <param name="msg">The Message</param>
        /// <param name="e">Exception attached to Log message</param>
        /// <returns></returns>
        public Task Log(LogSeverity severity, LogSource source, string msg, Exception e = null)
        {
            LogInternal(severity, source, msg, e);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Logs a message to the Console
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Task Log(LogMessage m)
        {
            LogInternal(m.Severity, GetLogSrc(m), m.Message, m.Exception);
            return Task.CompletedTask;

            static LogSource GetLogSrc(LogMessage msg) => msg.Source switch
            {
                "Rest" => LogSource.Rest,
                "Discord" => LogSource.Discord,
                "Gateway" => LogSource.Gateway,
                _ => LogSource.Unknown,
            };
        }
        
        /// <summary>
        /// Standard Log message with Severity as Info and Source as Self.
        /// </summary>
        /// <param name="m">The message</param>
        /// <returns></returns>
        public Task Log(string m) => Log(LogSeverity.Info, LogSource.Self, m);

        private void LogInternal(LogSeverity severity, LogSource source, string msg, Exception e)
        {
            using var writer = File.AppendText(_path);
            
            var color = VerifySeverity(severity);
            AppendText(severity.ToString(), color);
            AppendText("->", ConsoleColor.White);

            color = VerifySource(source);
            AppendText(source.ToString(), color);
            AppendText(":", ConsoleColor.White);

            writer.Write($"{severity} -> {source} : ");

            if (!msg.IsNullOrWhitespace())
            {
                AppendText(msg, ConsoleColor.White);
                writer.Write(msg);
            }

            if (e != null)
            {
                AppendText($"{e.Message}\n{e.StackTrace}", VerifySeverity(severity));
                writer.Write($"{e.Message}\n{e.StackTrace}");
            }
            
            Console.WriteLine();
            Console.ResetColor();
            writer.WriteLine();

            static void AppendText(string text, ConsoleColor color)
            {
                Console.ForegroundColor = color;
                Console.Write(text + " ");
            }
        }

        private static ConsoleColor VerifySeverity(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    return ConsoleColor.DarkRed;

                case LogSeverity.Warning:
                    return ConsoleColor.Yellow;

                case LogSeverity.Info:
                    return ConsoleColor.White;

                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                default:
                    return ConsoleColor.Gray;
            }
        }

        private static ConsoleColor VerifySource(LogSource source)
        {
            switch (source)
            {
                case LogSource.Module:
                case LogSource.Service:
                    return ConsoleColor.Yellow;

                case LogSource.Discord:
                case LogSource.Rest:
                case LogSource.Gateway:
                    return ConsoleColor.Blue;

                case LogSource.Self:
                    return ConsoleColor.Green;

                case LogSource.Unknown:
                default:
                    return ConsoleColor.Gray;
            }
        }
    }

    public enum LogSource : byte
    {
        Module,
        Service,
        Discord,
        Rest,
        Gateway,
        Self,
        Unknown,
    }
}
