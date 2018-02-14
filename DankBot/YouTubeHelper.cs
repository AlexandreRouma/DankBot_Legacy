using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Json;

namespace DankBot
{
    class YouTubeHelper
    {
        public static YouTubeVideo Search(string search)
        {
            YouTubeVideo video = new YouTubeVideo();
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = ConfigUtils.Configuration.YoutubeApiKey,
                ApplicationName = "DankBot"
            });
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = search; // Replace with your search term.
            searchListRequest.MaxResults = 10;
            var searchListResponse = searchListRequest.Execute();
            foreach (SearchResult r in searchListResponse.Items)
            {
                if (r.Id.Kind == "youtube#video")
                {
                    video.Title = r.Snippet.Title;
                    video.Url = $"https://www.youtube.com/watch?v={r.Id.VideoId}";
                    return video;
                }
            }
            throw new Exception("No results");
        }

        public static CommentThreadListResponse GetComments(string video)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = ConfigUtils.Configuration.YoutubeApiKey,
                ApplicationName = "DankBot"
            });
            var playlistItem = new PlaylistItem();
            var req = youtubeService.CommentThreads.List("snippet");
            req.VideoId = video;
            req.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;
            req.MaxResults = 100;
            return req.Execute();
        }
    }

    class YouTubeVideo
    {
        public string Title;
        public string Url;
    }
}
