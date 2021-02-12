using Discord;
using Discord.WebSocket;
using Discord.Net;
using System;
using System.Threading.Tasks;

namespace Discord_Simple_Embed_Bot
{
    public class Program
    {
        public static void Main()
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            DiscordSocketClient _client = new DiscordSocketClient();
            _client.Log += Logging.Log;
            _client.MessageReceived += Client_MessageReceived;
            await _client.LoginAsync(TokenType.Bot, "ODA4ODMwNzQxNTQ2MDA4NTc3.YCMQVA.kRe6ZL45NYS8DiuoQv0yx0NEHnE", false);
            await _client.StartAsync();
            CommandHandler.Client = _client;

            //Block this task until the program is closed.
            await Task.Delay(-1);

            
        }

        private async Task Client_MessageReceived(SocketMessage messageParam)
        {
            await CommandHandler.HandleCommand(messageParam);
        }
    }

    static class Logging
    {
        public static Task Log(LogMessage msg)
        {
            Console.ForegroundColor = GetColor(msg.Severity);
            Console.WriteLine(msg.ToString());
            Console.ResetColor();
            return Task.CompletedTask;
        }

        private static ConsoleColor GetColor(LogSeverity logSeverity)
        {
            switch (logSeverity)
            {
                case LogSeverity.Critical: return ConsoleColor.DarkRed;
                case LogSeverity.Error: return ConsoleColor.Red;
                case LogSeverity.Warning: return ConsoleColor.DarkYellow;
                case LogSeverity.Info: return ConsoleColor.Green;
                default: return ConsoleColor.White;
            }
        }
    }
}
