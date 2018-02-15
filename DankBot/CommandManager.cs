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

        public static void LoadCommands()
        {
            // Debug Commands
            commands.Add("DEBUG", DEBUG);

            // Search Commands
            commands.Add("GOOGLE", SearchCommands.Google);
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
            commands.Add("LOOP", AudioCommands.Loop);
            commands.Add("SOUNDEFFECT", AudioCommands.Soundeffect);

            // Admin Commands
            commands.Add("STOP", AdminCommands.Stop);
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
            commands.Add("WTH", ImageCommands.Hsgtf);
            commands.Add("USOAB", ImageCommands.Hsgtf);
            commands.Add("QR", ImageCommands.QR);

            // Misc Commands
            commands.Add("SAY", MiscCommands.Say);
            commands.Add("RANDOM", MiscCommands.Random);
            commands.Add("UNDO", MiscCommands.Undo);
            commands.Add("PLZHALP", MiscCommands.Say);
            commands.Add("AESTHETIC", MiscCommands.Say);
            commands.Add("B64ENCODE", MiscCommands.Say);
            commands.Add("B64DECODE", MiscCommands.Say);
        }

        public static async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.Id == Program.client.CurrentUser.Id)
            {
                return;
            }

            if (message.Content.StartsWith(ConfigUtils.Configuration.Prefix))
            {
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
                string msg = message.Content.Substring(ConfigUtils.Configuration.Prefix.Length);
                string[] arg = msg.Split(' ');
                string cmd = arg[0];

                Func<SocketMessage, string[], string, Task> command;
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
            await message.Channel.SendMessageAsync("", false, genCodeResult(await RextesterHelper.runCodeAsync(msg.Substring(6), 5), message.Author));
        }

        static Embed genCodeResult(RextesterResponse result, SocketUser author)
        {
            EmbedBuilder em = new Discord.EmbedBuilder();
            em.Color = Discord.Color.Blue;
            EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
            eab.Name = $"{author.Username}#{author.Discriminator}'s Code Result";
            eab.IconUrl = author.GetAvatarUrl();
            em.Author = eab;
            if (result.Result != "")
            {
                if (result.Result.Length > 2042)
                {
                    result.Result = $"{result.Result.Substring(0, 2039)}...";
                }
                em.Description = $"```{result.Result}```";
            }
            if (result.Errors != null)
            {
                em.AddField("Errors:", $"```{result.Errors}```");
            }
            if (result.Warnings != null)
            {
                em.AddField("Warnings:", $"```{result.Warnings}```");
            }
            if(result.Stats != "")
            {
                em.AddField("Stats:", result.Stats);
            }
            return em.Build();
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
