using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Threading;

namespace DankBot
{
    class Program
    {

        static DiscordSocketClient client = new DiscordSocketClient();

        static void Main(string[] args)
        {
            BotMain().Wait();
        }

        static async Task BotMain()
        {
            Logger.WriteLine("Welcome to DankBot !");

            Logger.Write("Starting discord client...  ");
            try
            {
                await client.LoginAsync(TokenType.Bot, "MzQ4MTc0NzIzMjY1NTkzMzY0.DHjQGQ.PRF_EOfSAmVd3S1lpTCXy9ThLAE");
                await client.StartAsync();
                await client.SetStatusAsync(UserStatus.Online);
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }

            Logger.Write("Starting message handler... ");
            try
            {
                client.MessageReceived += MessageReceived;
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }

            Logger.Write("Enabling audio player...    ");
            try
            {
                Playlist.Enable();
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }

            Logger.WriteLine("Ready.", ConsoleColor.Green);

            while (true)
                Thread.Sleep(1000);
        }

        

        static async Task MessageReceived(SocketMessage message)
        {
            if (message.Content.StartsWith("!"))
            {
                string msg = message.Content.Substring(1);
                string[] arg = msg.Split(' ');
                string cmd = arg[0];

                switch (cmd.ToUpper())
                {
                    case "":
                        await message.Channel.SendMessageAsync("U wot m8 ?!");
                        break;
                    case "PING":
                        await message.Channel.SendMessageAsync("Ur a n00b !");
                        break;
                    case "SAY":
                        await message.Channel.SendMessageAsync(msg.Substring(4));
                        break;
                    case "CALC":
                        await message.Channel.SendMessageAsync("I'm not ur dank calculator asshole !");
                        break;
                    case "GOOGLE":
                        await message.Channel.SendMessageAsync($"https://www.google.fr/search?q={msg.Substring(7).Replace(' ','+')}");
                        break;
                    case "PENIS":
                        await message.Channel.SendMessageAsync("8================================-");
                        break;
                    case "SUICIDE":
                        await message.Channel.SendFileAsync("resources/images/nooseman.png");
                        break;
                    case "GETOVERHERE":
                        IAudioChannel channel = null;
                        channel = channel ?? (message.Author as IGuildUser)?.VoiceChannel;
                        if (channel != null)
                        {
                            new Thread(() => {
                                Playlist.JoinChannel(channel).Wait();
                            }).Start();
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Sorry, u aren't in a fucking audio channel m8`");
                        }
                        break;
                    case "PLAY":
                        string url = msg.Substring(5);
                        YoutubeAudio.Download(url);
                        await message.Channel.SendMessageAsync($":white_check_mark: `{url} has been added to the playlist !`");
                        break;
                    case "SKIP":
                        Playlist.Skip();
                        break;
                    case "STOP":
                        await message.Channel.SendMessageAsync($":white_check_mark: `Have a dank day m8 !`");
                        Disconnect().Start();
                        break;
                    case "SETGAME":
                        try
                        {
                            if (arg.Count() > 1)
                            {
                                string game = msg.Substring(8);
                                await client.SetGameAsync(game);
                                Logger.WriteLine($"Game set to '{game}' successfully !", ConsoleColor.Green);
                            }
                            else
                            {
                                await client.SetGameAsync("");
                                Logger.WriteLine("Cleared game successfully !", ConsoleColor.Green);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLine($"Error: {ex.Message}", ConsoleColor.Red);
                        }
                        break;
                    default:
                        await message.Channel.SendMessageAsync($":no_entry: `The command '{cmd}' is as legit as an OpticGaming player on this server :(`");
                        break;
                }
            }
        }

        static async Task Disconnect()
        {
            Logger.Write("Disabling audio player...   ");
            try
            {
                Playlist.Disable();
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }

            Logger.Write("Disconnecting...            ");
            try
            {
                await client.SetStatusAsync(UserStatus.Offline);
                await client.LogoutAsync();
                await client.StopAsync();
                client.Dispose();
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }
            Environment.Exit(0);
        }

        static void Panic(string error)
        {
            Logger.WriteLine($"Error: {error}", ConsoleColor.Red);
            Environment.Exit(1);
        }
    }
}
