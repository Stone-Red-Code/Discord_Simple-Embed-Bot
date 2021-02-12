using Discord;
using Discord.WebSocket;
using Discord.Net;
using System;
using System.Threading.Tasks;
using System.IO;

namespace Discord_Simple_Embed_Bot
{
    public class Program
    {
        public static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.txt");
            if (!File.Exists(tokenPath))
                File.Create(tokenPath).Close();

            string token = File.ReadAllText(tokenPath);
            if (string.IsNullOrWhiteSpace(token))
            {
                await Logging.Log(new LogMessage(LogSeverity.Critical, "Main", "", new ArgumentNullException("Token", $"Bot-Token not found! Put the Bot-Token into: '{tokenPath}'")));
                return;
            }

            DiscordSocketClient _client = new DiscordSocketClient();
            _client.Log += Logging.Log;
            _client.MessageReceived += Client_MessageReceived;

            try
            {
                await _client.LoginAsync(TokenType.Bot, token, false);
                await _client.StartAsync();
            }
            catch (Exception ex)
            {
                await Logging.Log(new LogMessage(LogSeverity.Critical, "Main", "", ex));
                return;
            }
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
            lock (Console.Out)
            {
                Console.ForegroundColor = GetColor(msg.Severity);
                Console.WriteLine(msg.ToString());
                Console.ResetColor();
            }
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
