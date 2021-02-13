using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord_Simple_Embed_Bot
{
    public static class CommandHandler
    {
        public static DiscordSocketClient Client { get; set; }

        public static readonly Dictionary<string, Command> CommandList = new()
        {
            { "help", new Command { Fun = Commands.Help, Desc = "Lists all commands", Usage = "<command>" } },
            { "prefix", new Command { Fun = Commands.ChangePrefix, Desc = "Changes prefix", Usage = "<prefix>" } },
            {
                "new",
                new Command
                {
                    Fun = Commands.CreateEmbed,  
                    Desc = "Creats new embed",
                    Usage = "<title>\n<hex color>\n<description>\n<fields>",
                    Example = "Title"
                + "\n#EEEEEE"
                + "\nDescription"
                + "\n++Field name"
                + "\nField content"
                + "\n--Inline field name"
                + "\nInline field content"
                }
            },
        };



        public static async Task HandleCommand(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message is null) return;

            int argPos = 0;
            string prefix = PrefixFromMessage(message);
            if (message.MentionedUsers.Any(x => x.Discriminator == Client.CurrentUser.Discriminator))
            {
                EmbedBuilder eb = new EmbedBuilder
                {
                    Color = Color.Blue,
                    Description = $"Use `{prefix} help` to list all commands."
                };
                await message.Channel.SendMessageAsync("", false, eb.Build());
                return;
            }

            if (!(message.HasStringPrefix(prefix, ref argPos)) || message.HasMentionPrefix(Client.CurrentUser, ref argPos) || message.Author.IsBot)
            {
                return;
            }


            string command = message.Content[prefix.Length..].Trim().ToLower();
            if (command.Contains(" "))
                command = command.Substring(0, command.IndexOf(" "));

            if (CommandList.ContainsKey(command))
            {
                SocketGuildUser socketGuildUser = message.Author as SocketGuildUser;
                if (socketGuildUser.GuildPermissions.Administrator)
                {
                    await CommandList[command].Fun(message);
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($"Command not found! Use `{PrefixFromMessage(message)} help` to list all commands.");
            }
        }

        public static string PrefixFromMessage(SocketUserMessage message)
        {

            string prefix = SqlManager.GetData((message.Channel as SocketGuildChannel).Guild.Id, 'p').Result;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return prefix;

        }

        public static string[] CheckCommandArgs(string content, int min, int max, string prefix)
        {
            content = content.Trim();
            content = content.Remove(0, prefix.Length).Trim();
            if (!content.Contains(" "))
            {
                return null;
            }

            List<string> args = content.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
            args.RemoveAt(0);
            if (args.Count >= min && args.Count <= max)
            {
                return args.ToArray();
            }
            return null;
        }

        public class Command
        {
            public string Desc { get; set; }
            public string Usage { get; set; }
            public string Example { get; set; }
            public Func<SocketUserMessage, Task> Fun { get; set; }
        }
    }
}