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
using System.Net;
using System.Drawing;

namespace DankBot
{
    class Program
    {

        public static DiscordSocketClient client = new DiscordSocketClient();

        static void Main(string[] args)
        {
            BotMain().Wait();
        }

        static async Task BotMain()
        {
            // Uncomment next two lines to generate config file
            //ConfigUtils.SetDefaults();
            //ConfigUtils.Save(@"resources\config\config.json");

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
                await client.LoginAsync(TokenType.User, ConfigUtils.Configuration.BotToken);
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

        public static List<string> coolDowns = new List<string>();

        static async Task MessageReceived(SocketMessage message)
        {
            if (message.Channel.Name != "bot-commands" && message.Author.Id != 274976585650536449 && message.Author.Id != 186310020365811721) // Little override for @xX_WhatsTheGeek_Xx and @GJ
            {
                return;
            }
            if (message.Author.Username == "DankBot")
            {
                return;
            }
            if (message.Content.ToUpper().Contains("TRAPS AREN'T GAY"))
            {
                await message.Channel.SendMessageAsync($"STFU {message.Author.Mention}");
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
                        await message.Channel.SendMessageAsync($"https://www.google.fr/search?q={msg.Substring(7).Replace(' ', '+')}");
                        break;
                    case "D0G3":
                        await message.Channel.SendMessageAsync("░░░░░░░█▐▓▓░████▄▄▄█▀▄▓▓▓▌█\n░░░░░▄█▌▀▄▓▓▄▄▄▄▀▀▀▄▓▓▓▓▓▌█\n░░░▄█▀▀▄▓█▓▓▓▓▓▓▓▓▓▓▓▓▀░▓▌█\n░░█▀▄▓▓▓███▓▓▓███▓▓▓▄░░▄▓▐█▌\n░█▌▓▓▓▀▀▓▓▓▓███▓▓▓▓▓▓▓▄▀▓▓▐█\n▐█▐██▐░▄▓▓▓▓▓▀▄░▀▓▓▓▓▓▓▓▓▓▌█▌\n█▌███▓▓▓▓▓▓▓▓▐░░▄▓▓███▓▓▓▄▀▐█\n█▐█▓▀░░▀▓▓▓▓▓▓▓▓▓██████▓▓▓▓▐█\n▌▓▄▌▀░▀░▐▀█▄▓▓██████████▓▓▓▌█▌\n▌▓▓▓▄▄▀▀▓▓▓▀▓▓▓▓▓▓▓▓█▓█▓█▓▓▌█▌\n█▐▓▓▓▓▓▓▄▄▄▓▓▓▓▓▓█▓█▓█▓█▓▓▓▐█");
                        break;
                    case "GUYMASTURBATINGONWHAMEN":
                        await message.Channel.SendMessageAsync(":point_up:️             :man:\n     :bug::zzz::necktie: :bug:\n                    :fuelpump:️     :boot:\n                :zap:️ 8==:punch: =D:sweat_drops:\n             :trumpet:   :eggplant:                      :sweat_drops:\n            :boot:      :boot:                       :ok_woman::skin-tone-1:");
                        break;
                    case "THANKING":
                        await message.Channel.SendMessageAsync("⠰⡿⠿⠛⠛⠻⠿⣷\n      ⣀⣄⡀⠀⠀⠀⠀⢀⣀⣀⣤⣄⣀⡀\n     ⢸⣿⣿⣷⠀⠀⠀⠀⠛⠛⣿⣿⣿⡛⠿⠷\n     ⠘⠿⠿⠋⠀⠀⠀⠀⠀⠀⣿⣿⣿⠇\n               ⠈⠉⠁\n \n    ⣿⣷⣄⠀⢶⣶⣷⣶⣶⣤⣀\n    ⣿⣿⣿⠀⠀⠀⠀⠀⠈⠙⠻⠗\n   ⣰⣿⣿⣿⠀⠀⠀⠀⢀⣀⣠⣤⣴⣶⡄\n ⣠⣾⣿⣿⣿⣥⣶⣶⣿⣿⣿⣿⣿⠿⠿⠛⠃\n⢰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡄\n⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡁\n⠈⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠁\n  ⠛⢿⣿⣿⣿⣿⣿⣿⡿⠟\n     ⠉⠉⠉");
                        break;
                    case "PENIS":
                        await message.Channel.SendMessageAsync("8================================>");
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
                                foreach (PlayListItem item in Playlist.files)
                                {
                                    if (item.Url == video.Url)
                                    {
                                        await message.Channel.SendMessageAsync($":no_entry: `Sorry, this video is already in the playlist !`");
                                        return;
                                    }
                                }
                                Playlist.Add(video);
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
                        if (!IsUserBotAdmin(message.Author.Id))
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
                            return;
                        }
                        await message.Channel.SendMessageAsync($":white_check_mark: `Have a dank day m8 !`");
                        Disconnect().Start();
                        break;
                    case "SETGAME":
                        if (!IsUserBotAdmin(message.Author.Id))
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
                            return;
                        }
                        try
                        {
                            if (arg.Count() > 1)
                            {
                                string game = msg.Substring(8);
                                await client.SetGameAsync(game);
                                ConfigUtils.Configuration.Playing = game;
                                ConfigUtils.Save(@"resources\config\config.json");
                                await message.Channel.SendMessageAsync($":white_check_mark: `The game is now '{game}'`");
                            }
                            else
                            {
                                await client.SetGameAsync("");
                                ConfigUtils.Configuration.Playing = "";
                                ConfigUtils.Save(@"resources\config\config.json");
                                await message.Channel.SendMessageAsync($":white_check_mark: `The game has been reset !`");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLine($"Error: {ex.Message}", ConsoleColor.Red);
                        }
                        break;
                    case "SETPREFIX":
                        if (!IsUserBotAdmin(message.Author.Id))
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
                            return;
                        }
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
                        if (!IsUserBotAdmin(message.Author.Id))
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
                            return;
                        }
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
                        if (!IsUserBotAdmin(message.Author.Id))
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
                            return;
                        }
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
                        if (Playlist.Loop)
                        {
                            response += "(LOOP MODE ENABLED)\n";
                        }
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
                    case "LOOP":
                        Playlist.Loop = !Playlist.Loop;
                        if (Playlist.Loop)
                        {
                            await message.Channel.SendMessageAsync($":white_check_mark: `Loop mode enabled`");
                            if (Playlist.files.Count > 0)
                            {
                                await client.SetGameAsync($"(Loop) {Playlist.files[0].Title}");
                            }
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync($":white_check_mark: `Loop mode disabled`");
                            if (Playlist.files.Count > 0)
                            {
                                await client.SetGameAsync(Playlist.files[0].Title);
                            }
                        }
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
                            await message.Channel.SendMessageAsync($":no_entry: `Invalid song id` :thinking:");
                        }
                        break;
                    case "RNG":
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
                                            file = @"resources\sounds\airhorn.wav";
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
                                        case "EOOF":
                                            file = @"resources\sounds\eoof.wav";
                                            break;
                                        case "MISSIONFAILED":
                                            file = @"resources\sounds\missionfailed.wav";
                                            break;
                                        case "TRIGGERED":
                                            file = @"resources\sounds\triggered.wav";
                                            break;
                                        case "JEFF":
                                            file = @"resources\sounds\jeff.wav";
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
                                                    try
                                                    {
                                                        Playlist.JoinChannel(achannel).Wait();
                                                    }
                                                    catch
                                                    {

                                                    }
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
                                    await message.Channel.SendMessageAsync($"https://github.com/AlexandreRouma/DankBot/wiki/Sound-Effect-List");
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
                        
                        await message.Channel.SendMessageAsync("https://github.com/AlexandreRouma/DankBot/wiki/Command-List");
                        break;
                    case "PI":
                        await message.Channel.SendMessageAsync("`PI = 3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679821480865132823066470938446095505822317253594081284811174502841027019385211055596446229489549303819644288109756659334461284756482337867831652712019091456485669234603486104543266482133936072602491412737245870066063155881748815209209628292540917153643678925903600113305305488204665213841469519415116094330572703657595919530921861173819326117931051185480744623799627495673518857527248912279381830119491298336733624406566430860213949463952247371907021798609437027705392171762931767523846748184676694051320005681271452635608277857713427577896091736371787214684409012249534301465495853710507922796892589235420199561121290219608640344181598136297747713099605187072113499999983729780499510597317328160963185950244594553469083026425223082533446850352619311881710100031378387528865875332083814206171776691473035982534904287554687311595628638823537875937519577818577805321712268066130019278766111959092164201989`");
                        break;
                    case "AVATAR":
                        if (message.MentionedUsers.Count() > 0)
                        {

                            await message.Channel.SendMessageAsync(message.MentionedUsers.FirstOrDefault().GetAvatarUrl(ImageFormat.Png, 1024).TrimEnd("?size=1024".ToCharArray()));
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Please tell me which user you want the avatar from...`");
                        }
                        break;
                    case "USERINFO":
                        if (message.MentionedUsers.Count() > 0)
                        {
                            message.MentionedUsers.FirstOrDefault();
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Please tell me which user you want the info from...`");
                        }
                        break;
                    case "UNDO":
                        var messages = await message.Channel.GetMessagesAsync(16).Flatten();
                        foreach (IMessage ms in messages)
                        {
                            if (ms.Author.Mention == client.CurrentUser.Mention)
                            {
                                await ms.DeleteAsync();
                                return;
                            }
                        }
                        break;
                    case "WTF":
                        await message.Channel.SendFileAsync(@"resources\images\wtf.png");
                        break;
                    case "HSGTF":
                        string imgLink = "";
                        if (arg.Count() > 1)
                        {
                            imgLink = msg.Substring(6);
                        }
                        else
                        {
                            imgLink = await ImageHelper.GetLastImageAsync(message);
                        }
                        if (imgLink == "")
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Sorry, I can't find any image :/`");
                            return;
                        }
                        new Thread(() => HsgtfTask(message, imgLink)).Start();
                        break;
                    case "WTH":
                        if (arg.Count() > 1)
                        {
                            imgLink = msg.Substring(6);
                        }
                        else
                        {
                            imgLink = await ImageHelper.GetLastImageAsync(message);
                        }
                        if (imgLink == "")
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Sorry, I can't find any image :/`");
                            return;
                        }
                        new Thread(() => WthTask(message, imgLink)).Start();
                        break;
                    case "USOAB":
                        if (arg.Count() > 1)
                        {
                            imgLink = msg.Substring(6);
                        }
                        else
                        {
                            imgLink = await ImageHelper.GetLastImageAsync(message);
                        }
                        if (imgLink == "")
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Sorry, I can't find any image :/`");
                            return;
                        }
                        new Thread(() => UsoabTask(message, imgLink)).Start();
                        break;
                    case "DUMPROLES":
                        string roleList = "Role ID            | Role Name\n------------------------------\n";
                        foreach (SocketRole role in client.GetGuild(ConfigUtils.Configuration.ServerID).Roles)
                        {
                            roleList += $"{role.Id} | {role.Name}\n";
                        }
                        await message.Channel.SendMessageAsync($"```{roleList}```");
                        break;
                    case "AMIBOTADMIN":
                        if (!IsUserBotAdmin(message.Author.Id))
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `You are not bot admin`");
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync($":white_check_mark: `You are bot admin`");
                        }
                        break;
                    case "DEBUG":
                        if (!IsUserBotAdmin(message.Author.Id))
                        {
                            await message.Channel.SendMessageAsync($":no_entry: `Your KD is too low to execute this command...`");
                            return;
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync($":white_check_mark: `You are bot admin`");
                        }
                        break;
                    default:
                        await message.Channel.SendMessageAsync($":no_entry: `The command '{cmd}' is as legit as an OpticGaming player on this server :(`");
                        break;
                }
            }
        }

        static void HsgtfTask(SocketMessage message, string imgLink)
        {
            Bitmap img = (Bitmap)System.Drawing.Image.FromStream(new MemoryStream(new WebClient().DownloadData(imgLink)));
            string filename = $@"cache\{Convert.ToBase64String(Encoding.Default.GetBytes(GetSalt() + imgLink.Substring(0, 16))).Replace("/", "_")}.png";
            ImageHelper.HSGTF(img).Save(filename);
            message.Channel.SendFileAsync(filename).Wait();
            File.Delete(filename);
        }

        static void WthTask(SocketMessage message, string imgLink)
        {
            Bitmap img = (Bitmap)System.Drawing.Image.FromStream(new MemoryStream(new WebClient().DownloadData(imgLink)));
            string filename = $@"cache\{Convert.ToBase64String(Encoding.Default.GetBytes(GetSalt() + imgLink.Substring(0, 16))).Replace("/", "_")}.png";
            ImageHelper.WTH(img).Save(filename);
            message.Channel.SendFileAsync(filename).Wait();
            File.Delete(filename);
        }

        static void UsoabTask(SocketMessage message, string imgLink)
        {
            byte[] file = new WebClient().DownloadData(imgLink);
            Bitmap img = (Bitmap)System.Drawing.Image.FromStream(new MemoryStream(file));
            string filename = $@"cache\{Convert.ToBase64String(Encoding.Default.GetBytes(GetSalt() + imgLink.Substring(0, 16))).Replace("/", "_")}.png";
            ImageHelper.USOAB(img).Save(filename);
            message.Channel.SendFileAsync(filename).Wait();
            File.Delete(filename);
        }

        static void removeCooldown(string user)
        {
            Thread.Sleep(2000);
            coolDowns.Remove(user);
        }

        static bool IsUserBotAdmin(ulong id)
        {
            if (id == 274976585650536449 || id == 186310020365811721) // Little override for @xX_WhatsTheGeek_Xx and @GJ
            {
                return true;
            }
            foreach (SocketRole role in client.GetGuild(ConfigUtils.Configuration.ServerID).GetUser(id).Roles)
            {
                if (ConfigUtils.Configuration.AdminRoles.Contains(role.Id))
                {
                    return true;
                }
            }
            return false;
        }

        static string GetSalt()
        {
            Random rng = new Random();
            string str = "";
            for (int i = 0; i < 10; i++)
            {
                str += (char)rng.Next(65, 128);
            }
            return str;
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
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}
