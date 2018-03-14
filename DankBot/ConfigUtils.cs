using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Discord;

namespace DankBot
{
    class ConfigUtils
    {
        public static string DEFAULT_BOTTOKEN = "[PLEASE ENTER YOUR TOKEN HERE]";
        public static TokenType DEFAULT_TOKENTYPE = TokenType.Bot;
        public static string DEFAULT_PREFIX = "!";
        public static string DEFAULT_PLAYING = "DankBot";
        public static bool DEFAULT_SOUNDEFFECTS = true;
        public static string DEFAULT_YOUTUBEAPIKEY = "[ENTER YOUTUBE API KEY HERE]";
        public static string DEFAULT_GOOGLEAPIKEY = "[ENTER GOOGLE API KEY HERE]";
        public static string DEFAULT_TWITTERCKEY = "[ENTER TWITTER CONSUMER KEY HERE]";
        public static string DEFAULT_TWITTERCSECRET = "[ENTER TWITTER CONSUMER SECRET HERE]";
        public static string DEFAULT_TWITTERATOKEN = "[ENTER TWITTER ACCESS TOKEN HERE]";
        public static string DEFAULT_TWITTERATOKENSECRET = "[ENTER TWITTER ACCESS TOKEN SECRET HERE]";
        public static ulong DEFAULT_SERVERID = 0;
        public static ulong[] DEFAULT_ADMINROLES = { 0 };
        public static bool DEFAULT_RESOURCECACHING = true;

        public static Config Configuration = new Config();

        public static void Load(string path)
        {
            Configuration = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }

        public static void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(Configuration, Formatting.Indented));
        }

        public static void SetDefaults()
        {
            Configuration.BotToken = DEFAULT_BOTTOKEN;
            Configuration.TokenType = DEFAULT_TOKENTYPE;
            Configuration.Prefix = DEFAULT_PREFIX;
            Configuration.Playing = DEFAULT_PLAYING;
            Configuration.SoundEffects = DEFAULT_SOUNDEFFECTS;
            Configuration.YoutubeApiKey = DEFAULT_YOUTUBEAPIKEY;
            Configuration.GoogleApiKey = DEFAULT_GOOGLEAPIKEY;
            Configuration.TwitterConsumerKey = DEFAULT_TWITTERCKEY;
            Configuration.TwitterConsumerSecret = DEFAULT_TWITTERCSECRET;
            Configuration.TwitterAccessToken = DEFAULT_TWITTERATOKEN;
            Configuration.TwitterAccessTokenSecret = DEFAULT_TWITTERATOKENSECRET;
            Configuration.ServerID = DEFAULT_SERVERID;
            Configuration.AdminRoles = DEFAULT_ADMINROLES;
            Configuration.ResourceCaching = DEFAULT_RESOURCECACHING;
        }
    }

    class Config
    {
        public string BotToken;
        public TokenType TokenType;
        public string Prefix;
        public string Playing;
        public bool SoundEffects;
        public string YoutubeApiKey;
        public string GoogleApiKey;
        public string TwitterConsumerKey;
        public string TwitterConsumerSecret;
        public string TwitterAccessToken;
        public string TwitterAccessTokenSecret;
        public ulong ServerID;
        public ulong[] AdminRoles;
        public bool ResourceCaching;
    }
}
