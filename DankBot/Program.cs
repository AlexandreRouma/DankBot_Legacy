using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Threading;
using System.Configuration;
using System.IO;

namespace DankBot
{
    class Program
    {

        static DiscordSocketClient client = new DiscordSocketClient();

        static void Main(string[] args)
        {
            GoogleHelper.Search("wikipedia");
            BotMain().Wait();
        }

        static async Task BotMain()
        {
            Logger.WriteLine("Welcome to DankBot !");

            Logger.Write("Loading configuration...    ");
            try
            {
                ConfigUtils.Load(@"resources\config\config.json");
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }

            Logger.Write("Starting discord client...  ");
            try
            {
                await client.LoginAsync(TokenType.Bot, ConfigUtils.Configuration.BotToken);
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
            if (message.Content.StartsWith("!") && message.Author.Username != "xX_WhatsTheGeek_Xx")
            {
                //await message.Channel.SendMessageAsync("Nique ta race !");
                //return;
            }
            if (message.Content.StartsWith(ConfigUtils.Configuration.Prefix))
            {
                string msg = message.Content.Substring(ConfigUtils.Configuration.Prefix.Length);
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
                    case "PLAY":
                        string url = msg.Substring(5);
                        IAudioChannel channel = null;
                        channel = channel ?? (message.Author as IGuildUser)?.VoiceChannel;
                        try
                        {
                            if (!Playlist.Enabled)
                            {
                                Playlist.CurrentChannel = channel;
                                if (channel != null)
                                {
                                    new Thread(() => {
                                        Playlist.JoinChannel(channel).Wait();
                                    }).Start();
                                    Playlist.Enable();
                                }
                                else
                                {
                                    await message.Channel.SendMessageAsync($":no_entry: `Sorry, u aren't in a fucking audio channel m8`");
                                    break;
                                }
                            }
                            if (Playlist.CurrentChannel == channel)
                            {
                                YouTubeVideo video = YouTubeHelper.Search(url);
                                YoutubeAudio.Download(video);
                                await message.Channel.SendMessageAsync($":white_check_mark: `'{video.Title}' has been added to the playlist !`");
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync($":no_entry: `Sorry, you are't in the same audio channel as the bot !`");
                            }
                        }
                        catch
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `That video doesn't exist nigga`:joy:");
                        }
                        break;
                    case "SKIP":
                        Playlist.Skip();
                        break;
                    case "STOP":
                        //await message.Channel.SendMessageAsync($":white_check_mark: `Have a dank day m8 !`");
                        //Disconnect().Start();
                        await message.Channel.SendMessageAsync($":white_check_mark: `I DON'T THINK SO...`");
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
                    case "SETPREFIX":
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
                        catch (Exception ex)
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Could not change the dank prefix :/`");
                        }
                        break;
                    case "RELOAD":
                        try
                        {
                            ConfigUtils.Load(@"resources\config\config.json");
                            await message.Channel.SendMessageAsync($":white_check_mark: `The configuration has been reloaded`");
                        }
                        catch (Exception ex)
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Could not reload the configuration :/`");
                        }
                        break;
                    case "RESET":
                        try
                        {
                            ConfigUtils.SetDefaults();
                            ConfigUtils.Save(@"resources\config\config.json");
                            await message.Channel.SendMessageAsync($":white_check_mark: `The configuration has been reset`");
                        }
                        catch (Exception ex)
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Could not reload the configuration :/`");
                        }
                        break;
                    case "YT":
                        try
                        {
                            await message.Channel.SendMessageAsync(YouTubeHelper.Search(msg.Substring(3)).Url);
                        }
                        catch
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `That video doesn't exist nigga`:joy:");
                        }
                        break;
                    case "PLAYLIST":
                        string response = "```";
                        int i = 1;
                        if (Playlist.files.Count == 0)
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `There are currently no songs in the playlist :/`");
                            break;
                        }
                        foreach (PlayListItem video in Playlist.files)
                        {
                            response += $"{i}) {video.Title}\n";
                            i++;
                        }
                        await message.Channel.SendMessageAsync($"{response}```");
                        break;
                    case "REMOVE":
                        try
                        {
                            int id = int.Parse(msg.Substring(6));
                            string name = Playlist.files[id - 1].Title;
                            if (id == 1)
                                Playlist.Skip();
                            else
                                Playlist.files.RemoveAt(id - 1);
                            await message.Channel.SendMessageAsync($":white_check_mark: `Removed {name} from the playlist`");
                        }
                        catch
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Invalid song id`:thinking:");
                        }
                        break;
                    case "SE":
                        try
                        {
                            if (Playlist.files.Count == 0)
                            {
                                if (msg.Split(' ').Count() > 1)
                                {
                                    string file = "";
                                    switch (msg.Split(' ')[1].ToUpper())
                                    {
                                        case "AIRHORN":
                                            file = @"resources\sounds\playlist_skip.wav";
                                            break;
                                        case "TRIPLE":
                                            file = @"resources\sounds\triple.wav";
                                            break;
                                        case "WRONGNUMBER":
                                            file = @"resources\sounds\wrongnumber.wav";
                                            break;
                                        case "OOOHHH":
                                            file = @"resources\sounds\oooh.wav";
                                            break;
                                        case "GUNSHOT":
                                            file = @"resources\sounds\gunshot.wav";
                                            break;
                                        case "FAIL":
                                            file = @"resources\sounds\fail.wav";
                                            break;
                                        case "FUCKTHISSHIT":
                                            file = @"resources\sounds\fuckthisshit.wav";
                                            break;
                                        case "OOF":
                                            file = @"resources\sounds\oof.wav";
                                            break;
                                        default:
                                            await message.Channel.SendMessageAsync($":no_entry: `I don't know this sound effect m8...`");
                                            break;
                                    }
                                    if (file != "")
                                    {
                                        IAudioChannel achannel = null;
                                        achannel = achannel ?? (message.Author as IGuildUser)?.VoiceChannel;
                                        if (!Playlist.Enabled)
                                        {
                                            Playlist.CurrentChannel = achannel;
                                            if (achannel != null)
                                            {
                                                Playlist.client = null;
                                                Playlist.Enable();
                                                new Thread(() =>
                                                {
                                                    Playlist.JoinChannel(achannel).Wait();
                                                }).Start();
                                            }
                                            else
                                            {
                                                await message.Channel.SendMessageAsync($":no_entry: `Sorry, u aren't in a fucking audio channel m8`");
                                                break;
                                            }
                                        }
                                        if (Playlist.CurrentChannel == achannel)
                                        {

                                            new Thread(() =>
                                            {
                                                Playlist.SendFileAsync(file);
                                                Playlist.Disable();
                                            }).Start();

                                        }
                                        else
                                        {
                                            await message.Channel.SendMessageAsync($":no_entry: `Sorry, you are't in the same audio channel as the bot !`");
                                        }
                                    }
                                }
                                else
                                {
                                    await message.Channel.SendMessageAsync($":no_entry: `You didn't tell me which sound effect to play xD`");
                                }
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync($":no_entry: `Sorry, I'm already playing music...`:musical_note:");
                            }
                        }
                        catch { }
                        break;
                    case "PLZHALP":
                        await message.Channel.SendFileAsync(@"resources\config\help.txt");
                        break;
                    case "DEBUG":
                        await message.Channel.SendMessageAsync(YouTubeHelper.Search(msg.Substring(6)).Url);
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
