using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Json;

namespace DankBot
{
    class GoogleHelper
    {
        public static Result Search(string search)
        {
            var googleService = new CustomsearchService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyAz8XwVBS15-YpKBZd_r38HGX2BYzGBNT4",
                ApplicationName = "DankBot"
            });

            var listRequest = googleService.Cse.List(search);
            listRequest.Cx = "009590618107170545812:chu7p8sc1ie";
            var src = listRequest.Execute();

            return src.Items.FirstOrDefault();
        }

        public static Result SearchImage(string search)
        {
            var googleService = new CustomsearchService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyAz8XwVBS15-YpKBZd_r38HGX2BYzGBNT4",
                ApplicationName = "DankBot"
            });

            var listRequest = googleService.Cse.List(search);
            listRequest.FileType = "png";
            listRequest.Cx = "009590618107170545812:vkajuihze6m";
            var src = listRequest.Execute();

            return src.Items.FirstOrDefault();
        }
    }
}
