using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.mariuszgromada.math.mxparser;

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

        public static async Task EmojiPrank(SocketMessage message, string[] arg, string msg)
        {
            await message.Channel.SendMessageAsync($"HA ! You just got ppprrrraaaannnkkkeeeedddd XDDDDDD");
        }

        public static async Task Aesthetic(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Length > 1)
            {
                var originalAbc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890 ";
                var abc = "ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ１２３４５６７８９０\t";
                var fancyString = "";
                foreach (var character in msg.Substring(10).ToUpper())
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

        public static Dictionary<char, char> leet_speak = new Dictionary<char, char>
        {
            { 'a', '4' },
            { 'e', '3' },
            { 'i', '1' },
            { 'l', '1' },
            { 'o', '0' },
            { 's', '5' },
            { 't', '7' },
            { 'z', '2' },
        };

        public static async Task Leet(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                string str = "";
                foreach (char c in msg.Substring(5).ToLower())
                {
                    char r = (char)0;
                    leet_speak.TryGetValue(c, out r);
                    if (r == 0)
                    {
                        r = c;
                    }
                    str += r;
                }
                await message.Channel.SendMessageAsync($"```{str}```");
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please say what you want me to encode...`");
            }
        }

        public static string[] because = { "Because of china.",
                                           "Because you're gay.",
                                           "Because you have cancer.",
                                           "Because of jake paul.",
                                           "Because of hitler.",
                                           "Because jetfuel can't melt steel beams",
                                           "Because mans not hot",
                                           "because GJ got banned"};

        public static async Task Why(SocketMessage message, string[] arg, string msg)
        {
            await message.Channel.SendMessageAsync($"`{because[new Random().Next(0, because.Count())]}`");
        }

        public static async Task Mock(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                string str = "";
                Random rng = new Random();
                foreach (char c in msg.Substring(5).ToLower())
                {
                    if (rng.Next(2) == 0)
                    {
                        str += $"{c}".ToUpper();
                    }
                    else
                    {
                        str += $"{c}".ToLower();
                    }
                    
                }
                await message.Channel.SendMessageAsync($"```{str}```");
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please enter a string`");
            }
        }

        public static async Task Run(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 2)
            {
                int id = 0;
                RextesterHelper.languages.TryGetValue(arg[1].ToLower(), out id);
                if (id != 0)
                {
                    RextesterResponse result = await RextesterHelper.runCodeAsync(msg.Substring(5 + arg[1].Length), id);
                    if (result.Result != "" && result.Result != null)
                    {
                        if (result.Result.Length > 1994)
                        {
                            result.Result = $"{result.Result.Substring(0, 1991)}...";
                        }
                        await message.Channel.SendMessageAsync($"```{result.Result}```");
                    }
                    await message.Channel.SendMessageAsync("", false, genCodeResult(result, message.Author));
                }
                else
                {
                    await message.Channel.SendMessageAsync($":no_entry: `Unknown language: {arg[1]}`");
                }
            }
            else if (arg.Count() > 1 && arg[1].ToUpper() == "LIST")
            {
                await message.Channel.SendMessageAsync("", false, genLangList());
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please include language name and code`");
            }

        }

        public static async Task Calculate(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                try
                {
                    Expression e = new Expression(msg.Substring(10).ToLower());
                    await message.Channel.SendMessageAsync($"```{msg.Substring(10)} = {e.calculate()}```");
                }
                catch
                {
                    await message.Channel.SendMessageAsync($":no_entry: `Could not evaluate expression` :computer:");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please enter math expression to evaluate`");
            }

        }



        static Embed genCodeResult(RextesterResponse result, SocketUser author)
        {
            EmbedBuilder em = new EmbedBuilder();
            em.Color = Color.Blue;
            EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
            eab.Name = $"{author.Username}#{author.Discriminator}'s Code Result";
            eab.IconUrl = author.GetAvatarUrl();
            em.Author = eab;
            if (result.Errors != null)
            {
                em.AddField("Errors:", $"```{result.Errors}```");
            }
            if (result.Warnings != null)
            {
                em.AddField("Warnings:", $"```{result.Warnings}```");
            }
            if (result.Stats != "")
            {
                em.AddField("Stats:", result.Stats);
            }
            return em.Build();
        }

        static Embed genLangList()
        {
            EmbedBuilder em = new Discord.EmbedBuilder();
            em.Color = Discord.Color.Blue;
            em.Title = "Supported Languages:";
            string str = "";
            foreach (KeyValuePair<string, int> key in RextesterHelper.languages)
            {
                str += $"{key.Key}\n";
            }
            em.Description = str;
            return em.Build();
        }
    }
}
