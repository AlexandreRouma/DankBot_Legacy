using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Threading;
using System.IO;
using System.Net;

namespace DankBot
{
    class Program
    {

        public static DiscordSocketClient client = new DiscordSocketClient();

        static void Main(string[] args)
        {
            BotMain().Wait();
        }

        static async Task BotMain()
        {
            Logger.WriteLine("Welcome to DankBot !");

            if (!Directory.Exists(@"resources\config\"))
                Directory.CreateDirectory(@"resources\config\");
            if (!Directory.Exists(@"cache"))
                Directory.CreateDirectory(@"cache");

            if (!File.Exists(@"resources\config\config.json"))
            {
                Logger.Write("Generating configuration... ");
                try
                {
                    ConfigUtils.SetDefaults();
                    ConfigUtils.Save(@"resources\config\config.json");
                    Logger.OK();
                }
                catch
                {
                    Logger.FAILED();
                    Panic("Could not generate configuration file !");
                }
                
            }
            Logger.Write("Loading configuration...    ");
            try
            {
                ConfigUtils.Load(@"resources\config\config.json");
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }

            Logger.Write("Starting discord client...  ");
            try
            {
                await client.LoginAsync(ConfigUtils.Configuration.TokenType, ConfigUtils.Configuration.BotToken);
                await client.StartAsync();
                await client.SetStatusAsync(UserStatus.Online);
                await client.SetGameAsync(ConfigUtils.Configuration.Playing);
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }
            Logger.Write("Loading resources...        ");
            try
            {
                ResourceCache.Load();
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }

            Logger.Write("Starting message handler... ");
            try
            {
                CommandManager.LoadCommands();
                client.MessageReceived += CommandManager.MessageReceived;
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }

            Logger.Write("Enabling audio player...    ");
            try
            {
                Playlist.Enable();
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }

            Logger.WriteLine("Ready.", ConsoleColor.Green);

            while (true)
                Thread.Sleep(1000);
        }

        public static async Task Disconnect()
        {
            Logger.Write("Disabling audio player...   ");
            try
            {
                Playlist.Disable();
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }

            Logger.Write("Disconnecting...            ");
            try
            {
                await client.SetStatusAsync(UserStatus.Offline);
                await client.LogoutAsync();
                await client.StopAsync();
                client.Dispose();
                Logger.OK();
            }
            catch (Exception ex)
            {
                Logger.FAILED();
                Panic(ex.Message);
            }
            Environment.Exit(0);
        }

        static void Panic(string error)
        {
            Logger.WriteLine($"Error: {error}", ConsoleColor.Red);
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}
