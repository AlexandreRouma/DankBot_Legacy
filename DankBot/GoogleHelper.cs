using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DankBot
{
    class GoogleHelper
    {
        public static GoogleResult Search(string search)
        {
            //string page = new WebClient().DownloadString($"https://www.google.co.uk/search?q={search}");
            //string descriptorRaw = page.Substring(page.IndexOf("<div class=\"g\">") + 17);
            //int urlStart = descriptorRaw.IndexOf(";url=") + 10;
            //string urlRaw = descriptorRaw.Substring(urlStart);
            //string url = urlRaw.Substring(0, urlRaw.IndexOf("&amp;"));
            string uriString = "http://www.google.com/search";
            string keywordString = "Test Keyword";

            WebClient webClient = new WebClient();

            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection.Add("q", keywordString);

            webClient.QueryString.Add(nameValueCollection);
            string str = webClient.DownloadString(uriString);
            return new GoogleResult();
        }
    }

    class GoogleResult
    {
        public string Url;
    }
}
