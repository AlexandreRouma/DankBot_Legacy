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
        private static IAudioClient client;
        private static Thread playerThread = new Thread(player);
        public static List<string> files = new List<string>();
        private static bool playing = false;

        public static async Task JoinChannel(IAudioChannel channel)
        {
            client = await channel.ConnectAsync();
        }

        public static void Skip()
        {
            playing = false;
        }

        public static void Enable()
        {
            playerThread = new Thread(player);
            playerThread.Start();
        }

        public static void Disable()
        {
            if (client != null)
                client.Dispose();
            playerThread.Abort();
        }

        private static void player()
        {
            while (true)
            {
                while (files.Count() > 0)
                {
                    SendAsync(files[0]);
                    if (!ffmpeg.HasExited)
                        ffmpeg.Kill();
                    ffmpeg.WaitForExit();
                    File.Delete(files[0]);
                    files.RemoveAt(0);
                }
                Thread.Sleep(100);
            }
        }

        private static Process CreateStream(string path)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "resources/utils/ffmpeg/ffmpeg.exe",
                Arguments = $"-i {path} -ac 2 -f s16le -ar 48000 pipe:1 -loglevel panic",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            return Process.Start(ffmpeg);
        }

        private static Process ffmpeg;

        public static void SendAsync(string path)
        {
            ffmpeg = CreateStream(path);
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = client.CreatePCMStream(AudioApplication.Mixed);
            playing = true;
            output.CopyToAsync(discord).Wait();
            discord.FlushAsync().Wait();
            playing = false;
        }
    }
}