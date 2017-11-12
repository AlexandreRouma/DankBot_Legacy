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
            searchListRequest.MaxResults = 1;
            var searchListResponse = searchListRequest.Execute();
            video.Title = searchListResponse.Items.First().Snippet.Title;
            video.Url = $"https://www.youtube.com/watch?v={searchListResponse.Items.First().Id.VideoId}";
            return video;
        }
    }

    class YouTubeVideo
    {
        public string Title;
        public string Url;
    }
}
