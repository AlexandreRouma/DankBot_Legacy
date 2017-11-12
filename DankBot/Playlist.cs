using Discord;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DankBot
{
    class Playlist
    {
        public static IAudioClient client;
        public static IAudioChannel CurrentChannel;
        private static Thread playerThread = new Thread(player);
        public static List<PlayListItem> files = new List<PlayListItem>();
        private static bool playing = false;
        public static bool Enabled = false;
        public static bool Skipped = false;

        public static async Task JoinChannel(IAudioChannel channel)
        {
            client = await channel.ConnectAsync();
            Enabled = true;
            CurrentChannel = channel;
        }

        public static void Skip()
        {
            Skipped = true;
            playing = false;
            youtubedlThread.Abort();
            sendingThread.Abort();
            if (!ffmpeg.HasExited)
                ffmpeg.Kill();
            if (!youtube_dl.HasExited)
                youtube_dl.Kill();
        }

        public static void Enable()
        {
            if (!playerThread.IsAlive)
            {
                playerThread = new Thread(player);
                playerThread.Start();
            }
        }

        public static void Disable()
        {
            playing = false;
            if (youtubedlThread != null)
                youtubedlThread.Abort();
            sendingThread.Abort();
            client.StopAsync().Wait();
            if (client != null)
                client.Dispose();
            Enabled = false;
        }

        static Thread senderThread;

        private static void player()
        {
            while (true)
            {
                while (files.Count() > 0 && Enabled)
                {
                    SendAsync(files[0]);
                    playing = true;
                    if (!ffmpeg.HasExited)
                        ffmpeg.Kill();
                    ffmpeg.WaitForExit();
                    files.RemoveAt(0);
                    if (files.Count == 0)
                    {
                        SendFileAsync(@"resources\sounds\playlist_done.wav");
                        Disable();
                    }
                    else if (Skipped)
                    {
                        Skipped = false;
                        SendFileAsync(@"resources\sounds\playlist_skip.wav");
                    }
                }
                Thread.Sleep(100);
            }
        }

        private static Process CreateStream()
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = @"resources\utils\ffmpeg\ffmpeg.exe",
                Arguments = $"-i - -ac 2 -f s16le -ar 48000 pipe:1 -loglevel panic",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };
            return Process.Start(ffmpeg);
        }

        private static Process CreateFileStream(string path)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = @"resources\utils\ffmpeg\ffmpeg.exe",
                Arguments = $"-i {path} -ac 2 -f s16le -ar 48000 pipe:1 -loglevel panic",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            return Process.Start(ffmpeg);
        }

        private static Process CreateYoutubeDL(string url)
        {
            var youtubedl = new ProcessStartInfo
            {
                FileName = @"resources\utils\youtube-dl\youtube-dl.exe",
                Arguments = $@"{url} -q --no-warnings -o -",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            return Process.Start(youtubedl);
        }
        private static Process ffmpeg;
        private static Process youtube_dl;
        public static Thread sendingThread;
        public static Thread youtubedlThread;

        public static void SendAsync(PlayListItem item)
        {
            ffmpeg = CreateStream();
            youtube_dl = CreateYoutubeDL(item.Url);
            var output = ffmpeg.StandardOutput.BaseStream;
            var input = youtube_dl.StandardOutput.BaseStream;
            var discord = client.CreatePCMStream(AudioApplication.Mixed);
            youtubedlThread = new Thread(() =>
            {
                try
                {
                    input.CopyTo(ffmpeg.StandardInput.BaseStream);
                }
                catch { }
                playing = false;
            });
            sendingThread = new Thread(() =>
            {
                try
                {
                    output.CopyTo(discord);
                }
                catch { }
                playing = false;
            });
            youtubedlThread.Start();
            sendingThread.Start();
            playing = true;
            while (playing)
                Thread.Sleep(100);
            
            youtubedlThread.Abort();
            sendingThread.Abort();
            discord.FlushAsync().Wait();
            playing = false;
        }

        public static void SendFileAsync(string path)
        {
            while (client == null)
                Thread.Sleep(100);
            ffmpeg = CreateFileStream(path);
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = client.CreatePCMStream(AudioApplication.Mixed);
            sendingThread = new Thread(() =>
            {
                try
                {
                    output.CopyTo(discord);
                }
                catch { }
                playing = false;
            });
            sendingThread.Start();
            playing = true;
            while (playing)
                Thread.Sleep(100);
            sendingThread.Abort();
            discord.FlushAsync().Wait();
            playing = false;
        }
    }

    class PlayListItem
    {
        public string Title;
        public string Url;
    }
}