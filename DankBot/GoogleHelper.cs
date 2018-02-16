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
        public static Result Search(string search, bool safe = false)
        {
            var googleService = new CustomsearchService(new BaseClientService.Initializer()
            {
                ApiKey = ConfigUtils.Configuration.GoogleApiKey,
                ApplicationName = "DankBot"
            });

            var listRequest = googleService.Cse.List(search);
            listRequest.Cx = "009590618107170545812:chu7p8sc1ie";
            if (safe) { listRequest.Safe = CseResource.ListRequest.SafeEnum.Medium; };
            var src = listRequest.Execute();

            return src.Items.FirstOrDefault();
        }

        public static Result SearchImage(string search, bool safe = false)
        {
            var googleService = new CustomsearchService(new BaseClientService.Initializer()
            {
                ApiKey = ConfigUtils.Configuration.GoogleApiKey,
                ApplicationName = "DankBot"
            });

            var listRequest = googleService.Cse.List(search);
            listRequest.Cx = "009590618107170545812:vkajuihze6m";
            listRequest.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;
            if (safe) { listRequest.Safe = CseResource.ListRequest.SafeEnum.Medium; };
            var src = listRequest.Execute();

            return src.Items.FirstOrDefault();
        }
    }
}
