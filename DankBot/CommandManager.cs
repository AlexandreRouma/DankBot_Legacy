using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DankBot
{
    class CommandManager
    {
        public static Dictionary<string, Func<SocketMessage, string[], string, Task>> commands = new Dictionary<string, Func<SocketMessage, string[], string, Task>>();
        public static Dictionary<string, string> shortCommands = new Dictionary<string, string>();

        public static void LoadCommands()
        {
            // Debug Commands
            commands.Add("DEBUG", DEBUG);

            // Search Commands
            commands.Add("GOOGLE", SearchCommands.Google);
            commands.Add("IMAGE", SearchCommands.Image);
            commands.Add("YOUTUBE", SearchCommands.Youtube);
            commands.Add("URBAN", SearchCommands.Urban);
            commands.Add("COMMENT", SearchCommands.Comment);
            commands.Add("LASTTWEET", SearchCommands.LastTweet);
            commands.Add("RANDOMTWEET", SearchCommands.RandomTweet);

            // Playlist Commands
            commands.Add("PLAY", AudioCommands.Play);
            commands.Add("SKIP", AudioCommands.Skip);
            commands.Add("PLAYLIST", AudioCommands.PlayList);
            commands.Add("REMOVE", AudioCommands.Remove);
            commands.Add("STOP", AudioCommands.Stop);
            commands.Add("LOOP", AudioCommands.Loop);
            commands.Add("SOUNDEFFECT", AudioCommands.Soundeffect);

            // Admin Commands
            commands.Add("TURNOFF", AdminCommands.Stop);
            commands.Add("SETGAME", AdminCommands.Setgame);
            commands.Add("SETPREFIX", AdminCommands.Setprefix);
            commands.Add("RELOAD", AdminCommands.Reload);
            commands.Add("RESET", AdminCommands.Reset);
            commands.Add("AVATAR", AdminCommands.Avatar);
            commands.Add("USERINFO", AdminCommands.Userinfo);
            commands.Add("DUMPROLES", AdminCommands.Dumproles);
            commands.Add("AMIBOTADMIN", AdminCommands.Amibotadmin);
            commands.Add("PING", AdminCommands.Ping);

            // Image Commands
            commands.Add("WTF", ImageCommands.WTF);
            commands.Add("WINK", ImageCommands.Wink);
            commands.Add("HSGTF", ImageCommands.Hsgtf);
            commands.Add("WTH", ImageCommands.Wth);
            commands.Add("USOAB", ImageCommands.Usoab);
            commands.Add("QR", ImageCommands.QR);
            commands.Add("ELIANS", ImageCommands.Elians);

            // Misc Commands
            commands.Add("SAY", MiscCommands.Say);
            commands.Add("RANDOM", MiscCommands.Random);
            commands.Add("UNDO", MiscCommands.Undo);
            commands.Add("PLZHALP", MiscCommands.Plzhalp);
            commands.Add("-;", MiscCommands.EmojiPrank);
            commands.Add("AESTHETIC", MiscCommands.Aesthetic);
            commands.Add("B64ENCODE", MiscCommands.B64Encode);
            commands.Add("B64DECODE", MiscCommands.B64Decode);
            commands.Add("RUN", MiscCommands.Run);
            commands.Add("LEET", MiscCommands.Leet);
            commands.Add("MOCK", MiscCommands.Mock);
            commands.Add("WHY", MiscCommands.Why);
            commands.Add("CALCULATE", MiscCommands.Calculate);

            // ============================ SHORT COMMANDS ============================

            shortCommands.Add("G", "GOOGLE");
            shortCommands.Add("YT", "YOUTUBE");
            shortCommands.Add("SE", "SOUNDEFFECT");
            shortCommands.Add("PL", "PLAYLIST");
            shortCommands.Add("B64E", "B64ENCODE");
            shortCommands.Add("B64D", "B64DECODE");
            shortCommands.Add("CALC", "CALCULATE");
        }

        public static async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.Id == Program.client.CurrentUser.Id)
            {
                return;
            }

            if (message.Content.StartsWith(ConfigUtils.Configuration.Prefix))
            {
                if (message.Author.Id == 186310020365811721)
                {
                    await message.Channel.SendMessageAsync(":wave: :joy: :ok_hand: :b: OI");
                    return;
                }
                if (coolDowns.Contains(message.Author.Mention))
                {
                    await message.Channel.SendMessageAsync($"Calm the fuck down {message.Author.Mention} !");
                    return;
                }
                else
                {
                    coolDowns.Add(message.Author.Mention);
                    new Task(() => removeCooldown(message.Author.Mention)).Start();
                }

                Logger.WriteLine($"{message.Author.Username}#{message.Author.Discriminator} issued: \"{message.Content}\"");

                string msg = message.Content.Substring(ConfigUtils.Configuration.Prefix.Length);
                string[] arg = msg.Split(' ');
                string cmd = arg[0];

                Func<SocketMessage, string[], string, Task> command;

                string fullCommand = "";
                shortCommands.TryGetValue(cmd.ToUpper(), out fullCommand);
                if (fullCommand != null)
                {
                    msg = fullCommand + msg.Substring(cmd.Length);
                    arg[0] = fullCommand;
                    cmd = fullCommand;
                }

                commands.TryGetValue(cmd.ToUpper(), out command);
                var type = message.Channel.EnterTypingState();
                if (command != null)
                {
                    Task awaiter = command(message, arg, msg);
                    awaiter.GetAwaiter().OnCompleted(() => { type.Dispose(); });
                    awaiter.Start();
                }
                else
                {
                    type.Dispose();
                    await message.Channel.SendMessageAsync($":no_entry: `The command '{cmd}' is as legit as an OpticGaming player on this server :(`");
                }
            }
        }


        // >-------------------------- FOR TESTING PURPOUSES ONLY --------------------------<

        public static async Task DEBUG(SocketMessage message, string[] arg, string msg)
        {
            await message.Channel.SendMessageAsync($":no_entry: `NO IMPLEMENTED YET`");
        }

        static Embed generateTest()
        {
            EmbedBuilder em = new Discord.EmbedBuilder();
            em.Color = Discord.Color.Blue;
            em.Title = "Title";
            EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
            eab.Url = "http://www.google.com/";
            eab.Name = "Embed Author Name";
            eab.IconUrl = "https://i.imgur.com/2HXCkSR.png";
            em.Author = eab;
            em.Description = "Description";
            EmbedFooterBuilder emfb = new EmbedFooterBuilder();
            emfb.IconUrl = "https://i.imgur.com/Pieqv0h.png";
            emfb.Text = "Footer text";
            em.Footer = emfb;
            em.ImageUrl = "https://i.imgur.com/Wks5VLA.png";
            em.ThumbnailUrl = "https://i.imgur.com/asc2NGx.png";
            em.Timestamp = new DateTimeOffset(new DateTime(2042, 4, 20, 4, 20, 42));
            return em.Build();
        }

        // >-------------------------- FOR TESTING PURPOUSES ONLY --------------------------<


        public static List<string> coolDowns = new List<string>();

        public static void removeCooldown(string user)
        {
            Thread.Sleep(1000);
            coolDowns.Remove(user);
        }
    }
}
