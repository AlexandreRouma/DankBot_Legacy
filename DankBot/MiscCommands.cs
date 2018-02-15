using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DankBot
{
    class MiscCommands
    {
        public static async Task Say(SocketMessage message, string[] arg, string msg)
        {
            await message.Channel.SendMessageAsync($"```{msg.Substring(4)}```");
        }

        public static async Task Wide(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                string str = "";
                foreach (char c in msg.Substring(5))
                {
                    str += $"{c} ";
                }
                await message.Channel.SendMessageAsync(str);
            }
            else
            {
                await message.Channel.SendMessageAsync($":white_check_mark: `Please enter some text...`");
            }
        }

        public static async Task Random(SocketMessage message, string[] arg, string msg)
        {
            try
            {
                int max = int.Parse(msg.Substring(3));
                Random rng = new Random();
                await message.Channel.SendMessageAsync($":white_check_mark: `{rng.Next(max + 1)}`");
            }
            catch
            {
                await message.Channel.SendMessageAsync($":no_entry: `Invalid number` :joy:");
            }
        }

        public static async Task Undo(SocketMessage message, string[] arg, string msg)
        {
            var messages = await message.Channel.GetMessagesAsync(16).Flatten();
            foreach (Discord.IMessage ms in messages)
            {
                if (ms.Author.Mention == Program.client.CurrentUser.Mention)
                {
                    await ms.DeleteAsync();
                    return;
                }
            }
        }

        public static async Task Plzhalp(SocketMessage message, string[] arg, string msg)
        {
            await message.Channel.SendMessageAsync("https://github.com/AlexandreRouma/DankBot/wiki/Command-List");
        }

        public static async Task Aesthetic(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Length > 1)
            {
                var originalAbc = "abcdefghijklmnopqrstuvwxyz1234567890 ";
                var abc = "ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ１２３４５６７８９０\t";
                var fancyString = "";
                foreach (var character in arg[1])
                {
                    var index = originalAbc.IndexOf(character);
                    if (index >= 0)
                    {
                        fancyString += abc[index] + " ";
                    }
                    else
                    {
                        fancyString += character + " ";
                    }
                }
                fancyString = fancyString.Substring(0, fancyString.Length - 1);
                await message.Channel.SendMessageAsync(fancyString);

            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Tell me what to say...`");
            }
        }

        public static async Task B64Encode(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                await message.Channel.SendMessageAsync($"`{Convert.ToBase64String(Encoding.Default.GetBytes(msg.Substring(10)))}`");
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please say what you want me to encode...`");
            }
        }

        public static async Task B64Decode(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                await message.Channel.SendMessageAsync($"`{Encoding.Default.GetString(Convert.FromBase64String(msg.Substring(10)))}`");
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please say what you want me to encode...`");
            }
        }
    }
}
