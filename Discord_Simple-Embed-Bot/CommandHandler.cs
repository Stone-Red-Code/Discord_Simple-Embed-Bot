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
        public static DiscordSocketClient Client;
        private static readonly SqlManager sqlManager = new SqlManager();

        static readonly Dictionary<string, Command> _commands = new()
        {
            { "help", new Command { Fun = Help, Desc = "Lists all commands", Usage = "<command>" } },
            { "prefix", new Command { Fun = ChangePrefix, Desc = "Changes prefix", Usage = "<prefix>" } },
            {
                "new",
                new Command
                {
                    Fun = CreateEmbed,
                    Desc = "Creats new embed",
                    Usage = "<title>\n<hex color>\n<description>\n<field>",
                    Example = "Title"
                + "\n#FFFFFF"
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
            if (message == null) return;

            int argPos = 0;
            string prefix = PrefixFromMessage(message);
            if (message.MentionedUsers.Any(x => x.Discriminator == Client.CurrentUser.Discriminator))
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.Color = Color.Blue;
                eb.Description = $"Use `{prefix} help` to list all commands.";
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

            if (_commands.ContainsKey(command))
            {
                SocketGuildUser socketGuildUser = message.Author as SocketGuildUser;
                if (socketGuildUser.GuildPermissions.Administrator)
                {
                    await _commands[command].Fun(message);
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($"Command not found! Use `{PrefixFromMessage(message)} help` to list all commands.");
            }
        }

        static string PrefixFromMessage(SocketUserMessage message)
        {

            string prefix = sqlManager.GetData((message.Channel as SocketGuildChannel).Guild.Id, 'p').Result;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return prefix;

        }

        static string[] CheckCommandArgs(string content, int min, int max, string prefix)
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

        static public async Task Help(SocketUserMessage message)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.Color = Color.Blue;
            string[] args = CheckCommandArgs(message.Content, 0, 1, PrefixFromMessage(message));
            if (args == null)
            {
                eb.Title = "Commands:";

                foreach (var item in _commands)
                {
                    eb.AddField(item.Key, item.Value.Desc);
                }
            }
            else if (args.Length > 0)
            {
                if (_commands.ContainsKey(args[0].ToLower()))
                {
                    eb.Title = args[0].ToLower();
                    eb.AddField("Parameters", _commands[args[0].ToLower()].Usage);
                    if (!string.IsNullOrWhiteSpace(_commands[args[0].ToLower()].Example))
                    {
                        eb.AddField("Example", $"```{PrefixFromMessage(message)} {args[0].ToLower()} {_commands[args[0].ToLower()].Example}```");
                    }
                }
                else
                {
                    eb.WithDescription("Command not found!");
                }
            }
            await message.Channel.SendMessageAsync("", false, eb.Build());
        }

        static public async Task ChangePrefix(SocketUserMessage message)
        {
            string prefix = "";
            if (message.Content.Contains(" "))
                prefix = message.Content[message.Content.LastIndexOf(" ")..].Trim();
            if (prefix.Length <= 0)
            {
                await message.Channel.SendMessageAsync($"Prefix not valid!");
                return;
            }

            await sqlManager.SetData((message.Channel as SocketGuildChannel).Guild.Id, prefix, 'p');
            await message.Channel.SendMessageAsync($"Prefix changed to: `{prefix}`");
        }

        static public async Task CreateEmbed(SocketUserMessage message)
        {
            string prefix = PrefixFromMessage(message);
            string content = message.Content[prefix.Length..].Trim()[3..];
            string[] lines = content.Split("\n");

            await message.DeleteAsync();

            EmbedBuilder eb = new EmbedBuilder();


            eb.Title = lines[0];

            if (lines.Length > 1)
            {
                try
                {
                    if (!lines[1].Contains("#"))
                        lines[1] = "#" + lines[1];
                    System.Drawing.Color col = (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(lines[1]);
                    eb.Color = new Color(col.R, col.G, col.B);
                }
                catch { }
            }
            if (lines.Length > 2)
            {
                eb.Description = lines[2];
            }

            EmbedFieldBuilder efb = null;
            foreach (string line in lines.Skip(3))
            {
                if (line.StartsWith("++"))
                {
                    if (efb is not null)
                        eb.AddField(efb);
                    efb = new EmbedFieldBuilder();
                    efb.IsInline = false;
                    efb.Name = line.Remove(0, 2);
                }
                else if (line.StartsWith("--"))
                {
                    if (efb is not null)
                        eb.AddField(efb);
                    efb = new EmbedFieldBuilder();
                    efb.IsInline = true;
                    efb.Name = line.Remove(0, 2);
                }
                else if (efb is not null && !string.IsNullOrWhiteSpace(line))
                {
                    efb.Value += line + "\n";
                }
            }

            if (eb is not null)
                eb.AddField(efb);

            for (int i = 0; i < eb.Fields.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(eb.Fields[i].Value as string))
                {
                    eb.Fields[i].Value = "-";
                }
            }

            try
            {
                await message.Channel.SendMessageAsync("", false, eb.Build());
            }
            catch (Exception ex)
            {
                await Logging.Log(new LogMessage(LogSeverity.Debug, "CMD Handler", "", ex));
            }
        }

        class Command
        {
            public string Desc { get; set; }
            public string Usage { get; set; }
            public string Example { get; set; }
            public Func<SocketUserMessage, Task> Fun { get; set; }
        }
    }
}