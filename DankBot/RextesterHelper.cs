using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DankBot
{
    class RextesterHelper
    {
        public static async Task<RextesterResponse> runCodeAsync(string prog, int language)
        {
            var values = new Dictionary<string, string>
            {
                { "LanguageChoiceWrapper", language.ToString() }, // 1 C#, 5 Python
                { "EditorChoiceWrapper", "1" },
                { "LayoutChoiceWrapper", "1" },
                { "Program", prog },
                { "ShowWarnings", "false" },
                { "IsInEditMode", "false" },
                { "IsLive", "false" }
            };

            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);

            var content = new FormUrlEncodedContent(values);

            try
            {
                var httpresponse = await (await client.PostAsync("http://rextester.com/rundotnet/Run", content)).Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RextesterResponse>(httpresponse);
            }
            catch
            {
                RextesterResponse response = new RextesterResponse();
                response.Errors = "Code ran longer than 10 seconds !";
                response.Stats = "";
                response.Result = "";
                return response;
            }
        }


    }

    class RextesterResponse
    {
        public string Warnings;
        public string Errors;
        public string Result;
        public string Stats;
        public string Files;
    }
}
