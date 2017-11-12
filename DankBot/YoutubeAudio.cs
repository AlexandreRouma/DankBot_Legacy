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
        public static void Download(YouTubeVideo video)
        {
            string videoid = video.Url.Substring(video.Url.LastIndexOf("v=") + 2, 11);
            PlayListItem item = new PlayListItem();
            item.Title = video.Title;
            item.Url = video.Url;
            Playlist.files.Add(item);
        }
    }
}
