using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Simple_Embed_Bot
{
    static class Commands
    {
        static public async Task Help(SocketUserMessage message)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.Color = Color.Blue;
            string[] args = CommandHandler.CheckCommandArgs(message.Content, 0, 1, CommandHandler.PrefixFromMessage(message));
            if (args == null)
            {
                eb.Title = "Commands:";

                foreach (var item in CommandHandler.CommandList)
                {
                    eb.AddField(item.Key, item.Value.Desc);
                }
            }
            else if (args.Length > 0)
            {
                if (CommandHandler.CommandList.ContainsKey(args[0].ToLower()))
                {
                    eb.Title = args[0].ToLower();
                    eb.AddField("Parameters", CommandHandler.CommandList[args[0].ToLower()].Usage);
                    if (!string.IsNullOrWhiteSpace(CommandHandler.CommandList[args[0].ToLower()].Example))
                    {
                        eb.AddField("Example", $"```{CommandHandler.PrefixFromMessage(message)} {args[0].ToLower()} {CommandHandler.CommandList[args[0].ToLower()].Example}```");
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

            if (Architecture.Arm != RuntimeInformation.OSArchitecture)
            {
                await SqlManager.SetData((message.Channel as SocketGuildChannel).Guild.Id, prefix, 'p');
                await message.Channel.SendMessageAsync($"Prefix changed to: `{prefix}`");
            }
            else
            {
                await message.Channel.SendMessageAsync($"Command does not support ARM architecture!");
            }
        }

        static public async Task CreateEmbed(SocketUserMessage message)
        {
            string prefix = CommandHandler.PrefixFromMessage(message);
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
    }
}
