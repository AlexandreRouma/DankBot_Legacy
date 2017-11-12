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
    class YoutubeAudio
    {

        /// <summary>
        /// Download the audio from a youtube video
        /// </summary>
        /// <param name="url">Url of the video to download</param>
        /// <param name="DownloadDone">Task that executes when the video is done</param>
        public static void Download(string url)
        {
            new Thread(() => {
                string videoid = url.Substring(url.LastIndexOf("v=") + 2, 11);
                Process youtubedl = new Process();
                youtubedl.StartInfo.FileName = @"resources\utils\youtube-dl\youtube-dl.exe";
                youtubedl.StartInfo.Arguments = $@"-o resources\utils\youtube-dl\temp\{videoid}.mp4 https://www.youtube.com/watch?v={videoid}";
                youtubedl.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                youtubedl.Start();
                youtubedl.WaitForExit();
                Playlist.files.Add($@"resources\utils\youtube-dl\temp\{videoid}.mp4");//$@"resources\sounds\downloaded\{temp_filename}.mp3");
            }).Start();
        }
    }
}
