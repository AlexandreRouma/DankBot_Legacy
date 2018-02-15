using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DankBot
{
    class AdminCommands
    {
        public static async Task Stop(SocketMessage message, string[] arg, string msg)
        {
            if (RoleUtils.IsUserBotAdmin(message.Author.Id))
            {
                await message.Channel.SendMessageAsync($":white_check_mark: `Have a dank day m8 !`");
                Program.Disconnect().Start();
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
            }
        }

        public static async Task Setgame(SocketMessage message, string[] arg, string msg)
        {
            if (!RoleUtils.IsUserBotAdmin(message.Author.Id))
            {
                try
                {
                    if (arg.Count() > 1)
                    {
                        string game = msg.Substring(8);
                        await Program.client.SetGameAsync(game);
                        ConfigUtils.Configuration.Playing = game;
                        ConfigUtils.Save(@"resources\config\config.json");
                        await message.Channel.SendMessageAsync($":white_check_mark: `The game is now '{game}'`");
                    }
                    else
                    {
                        await Program.client.SetGameAsync("");
                        ConfigUtils.Configuration.Playing = "";
                        ConfigUtils.Save(@"resources\config\config.json");
                        await message.Channel.SendMessageAsync($":white_check_mark: `The game has been reset !`");
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLine($"Error: {ex.Message}", ConsoleColor.Red);
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
            }
        }

        public static async Task Setprefix(SocketMessage message, string[] arg, string msg)
        {
            if (RoleUtils.IsUserBotAdmin(message.Author.Id))
            {
                try
                {
                    if (arg.Count() > 1)
                    {
                        string prefix = msg.Substring(10);
                        ConfigUtils.Configuration.Prefix = prefix;
                        ConfigUtils.Save(@"resources\config\config.json");
                        await message.Channel.SendMessageAsync($":white_check_mark: `The prefix is now '{prefix}'`");
                    }
                    else
                    {
                        ConfigUtils.Configuration.Prefix = ConfigUtils.DEFAULT_PREFIX;
                        ConfigUtils.Save(@"resources\config\config.json");
                        await message.Channel.SendMessageAsync($":white_check_mark: `The prefix has been reset.`");
                    }
                }
                catch
                {
                    await message.Channel.SendMessageAsync($":no_entry: `Could not change the dank prefix :/`");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
            }
            
        }

        public static async Task Reload(SocketMessage message, string[] arg, string msg)
        {
            if (!RoleUtils.IsUserBotAdmin(message.Author.Id))
            {
                try
                {
                    ConfigUtils.Load(@"resources\config\config.json");
                    await message.Channel.SendMessageAsync($":white_check_mark: `The configuration has been reloaded`");
                }
                catch
                {
                    await message.Channel.SendMessageAsync($":no_entry: `Could not reload the configuration :/`");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
            }
        }

        public static async Task Reset(SocketMessage message, string[] arg, string msg)
        {
            if (RoleUtils.IsUserBotAdmin(message.Author.Id))
            {
                try
                {
                    ConfigUtils.SetDefaults();
                    ConfigUtils.Save(@"resources\config\config.json");
                    await message.Channel.SendMessageAsync($":white_check_mark: `The configuration has been reset`");
                }
                catch
                {
                    await message.Channel.SendMessageAsync($":no_entry: `Could not reload the configuration :/`");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
            }
        }

        public static async Task Avatar(SocketMessage message, string[] arg, string msg)
        {
            if (message.MentionedUsers.Count() > 0)
            {

                await message.Channel.SendMessageAsync(message.MentionedUsers.FirstOrDefault().GetAvatarUrl(ImageFormat.Png, 1024).TrimEnd("?size=1024".ToCharArray()));
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please tell me which user you want the avatar from...`");
            }
        }

        public static async Task Userinfo(SocketMessage message, string[] arg, string msg)
        {
            if (message.MentionedUsers.Count() > 0)
            {
                await message.Channel.SendMessageAsync("", false, GenUser(message.MentionedUsers.FirstOrDefault()));
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please tell me which user you want the info from...`");
            }
        }


        public static async Task Dumproles(SocketMessage message, string[] arg, string msg)
        {
            string roleList = "Role ID            | Role Name\n------------------------------\n";
            foreach (SocketRole role in Program.client.GetGuild(ConfigUtils.Configuration.ServerID).Roles)
            {
                roleList += $"{role.Id} | {role.Name}\n";
            }
            await message.Channel.SendMessageAsync($"```{roleList}```");
        }

        public static async Task Amibotadmin(SocketMessage message, string[] arg, string msg)
        {
            if (!RoleUtils.IsUserBotAdmin(message.Author.Id))
            {
                await message.Channel.SendMessageAsync($":no_entry: `You are not bot admin`");
            }
            else
            {
                await message.Channel.SendMessageAsync($":white_check_mark: `You are bot admin`");
            }
        }

        public static async Task Ping(SocketMessage message, string[] arg, string msg)
        {
            await message.Channel.SendMessageAsync("Skidaddle Skadoodle, your dick is now a noodle !");
        }

        static Embed GenUser(SocketUser user)
        {
            EmbedBuilder em = new EmbedBuilder();
            em.Color = Discord.Color.Blue;
            em.ThumbnailUrl = user.GetAvatarUrl();
            EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
            eab.Url = user.GetAvatarUrl();
            eab.Name = $"{user.Username}#{user.Discriminator}";
            eab.IconUrl = user.GetAvatarUrl();
            em.Author = eab;
            var joined = Program.client.GetGuild(ConfigUtils.Configuration.ServerID).GetUser(user.Id).JoinedAt;
            em.AddField("Registered: ", $"{user.CreatedAt.DateTime.ToLongDateString()} at {user.CreatedAt.DateTime.ToLongTimeString()}");
            em.AddField("Joined: ", $"{joined.Value.DateTime.ToLongDateString()} at {joined.Value.DateTime.ToLongTimeString()}");
            string nick = Program.client.GetGuild(ConfigUtils.Configuration.ServerID).GetUser(user.Id).Nickname;
            if (nick == null) { nick = "none"; }
            em.AddField("Nickname: ", nick);
            em.AddField("ID: ", user.Id.ToString());
            string game = "none";
            if (user.Game.HasValue)
            {
                game = user.Game.Value.Name;
            }
            em.AddField("Game: ", game);
            string roles = "";
            int i = 0;
            foreach (SocketRole role in Program.client.GetGuild(ConfigUtils.Configuration.ServerID).GetUser(user.Id).Roles)
            {
                roles += $"`{role.Name}`";
                if (i < Program.client.GetGuild(ConfigUtils.Configuration.ServerID).GetUser(user.Id).Roles.Count - 1)
                {
                    roles += ", ";
                }
                i++;
            }
            if (roles == "") { roles = "none"; }
            em.AddField("Roles: ", roles);
            if (user.IsBot)
            {
                em.AddField("Is a bot: ", "Yes");
            }
            else
            {
                em.AddField("Is a bot: ", "No");
            }
            return em.Build();
        }
    }
}
