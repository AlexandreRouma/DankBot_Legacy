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
    class AudioCommands
    {
        public static async Task Play(SocketMessage message, string[] arg, string msg)
        {
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
                        return;
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
                    string tUpper = video.Title.ToUpper();
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
        }

        public static async Task Skip(SocketMessage message, string[] arg, string msg)
        {
            IAudioChannel schannel = null;
            schannel = schannel ?? (message.Author as IGuildUser)?.VoiceChannel;
            if (schannel == null)
            {
                await message.Channel.SendMessageAsync($":no_entry: `Sorry, u aren't in a fucking audio channel m8`");
                return;
            }
            if (!Playlist.SkipVotes.Contains(message.Author.Id))
            {
                Playlist.SkipVotes.Add(message.Author.Id);
                int c = (int)Math.Ceiling(((double)await schannel.GetUsersAsync().Count() / 2));
                if (Playlist.SkipVotes.Count() >= c)
                {
                    Playlist.Skip();
                }
                else
                {
                    await message.Channel.SendMessageAsync($":white_check_mark: `'Your vote has been added. {c - Playlist.SkipVotes.Count()} more votes needed`");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `You already voted...`");
            }
        }

        public static async Task PlayList(SocketMessage message, string[] arg, string msg)
        {
            string response = "```";
            int i = 1;
            if (Playlist.Loop)
            {
                response += "(LOOP MODE ENABLED)\n";
            }
            if (Playlist.files.Count == 0)
            {
                await message.Channel.SendMessageAsync($":no_entry: `There are currently no songs in the playlist :/`");
                return;
            }
            foreach (PlayListItem video in Playlist.files)
            {
                response += $"{i}) {video.Title}\n";
                i++;
            }
            await message.Channel.SendMessageAsync($"{response}```");
        }

        public static async Task Remove(SocketMessage message, string[] arg, string msg)
        {
            try
            {
                int id = int.Parse(msg.Substring(6));
                string name = Playlist.files[id - 1].Title;
                if (id == 1)
                {
                    await message.Channel.SendMessageAsync($":no_entry: `You can't remove the song currently playing...` :thinking:");
                    return;
                }
                else
                {
                    Playlist.files.RemoveAt(id - 1);
                }
                await message.Channel.SendMessageAsync($":white_check_mark: `Removed {name} from the playlist`");
            }
            catch
            {
                await message.Channel.SendMessageAsync($":no_entry: `Invalid song id` :thinking:");
            }
        }

        public static async Task Stop(SocketMessage message, string[] arg, string msg)
        {
            try
            {
                Playlist.files.Clear();
                Playlist.Skip();
            }
            catch
            {
                await message.Channel.SendMessageAsync($":no_entry: `Could not quit singing` :thinking:");
            }
        }

        public static async Task Loop(SocketMessage message, string[] arg, string msg)
        {
            Playlist.Loop = !Playlist.Loop;
            if (Playlist.Loop)
            {
                await message.Channel.SendMessageAsync($":white_check_mark: `Loop mode enabled`");
                if (Playlist.files.Count > 0)
                {
                    await Program.client.SetGameAsync($"(Loop) {Playlist.files[0].Title}");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":white_check_mark: `Loop mode disabled`");
                if (Playlist.files.Count > 0)
                {
                    await Program.client.SetGameAsync(Playlist.files[0].Title);
                }
            }
        }

        public static async Task Soundeffect(SocketMessage message, string[] arg, string msg)
        {
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
                                    return;
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
        }
    }
}
