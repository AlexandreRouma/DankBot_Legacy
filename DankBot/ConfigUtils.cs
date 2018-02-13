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
        public static string DEFAULT_YOUTUBEAPIKEY = "[PLEASE YOUTUBE API KEY YOUR TOKEN HERE]";
        public static string DEFAULT_GOOGLEAPIKEY = "[PLEASE YOUTUBE API KEY YOUR TOKEN HERE]";
        public static ulong DEFAULT_SERVERID = 0;
        public static ulong[] DEFAULT_ADMINROLES = { 0 };

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
            Configuration.ServerID = DEFAULT_SERVERID;
            Configuration.AdminRoles = DEFAULT_ADMINROLES;
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
        public ulong ServerID;
        public ulong[] AdminRoles;
    }
}
