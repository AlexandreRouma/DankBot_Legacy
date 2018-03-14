using Discord.WebSocket;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DankBot
{
    class ImageCommands
    {
        public static async Task WTF(SocketMessage message, string[] arg, string msg)
        {
            await message.Channel.SendFileAsync(@"resources\images\wtf.png");
        }

        public static async Task Wink(SocketMessage message, string[] arg, string msg)
        {
            await message.Channel.SendFileAsync(@"resources\images\wink.png");
        }

        public static async Task Hsgtf(SocketMessage message, string[] arg, string msg)
        {
            string imgLink = "";
            if (arg.Count() > 1)
            {
                imgLink = msg.Substring(6);
            }
            else
            {
                imgLink = await ImageHelper.GetLastImageAsync(message);
            }
            if (imgLink != "")
            {
                Bitmap img = (Bitmap)Image.FromStream(new MemoryStream(new WebClient().DownloadData(imgLink)));
                string filename = $@"cache\{FileUtils.RandomFilename(imgLink)}.png";
                ImageHelper.HSGTF(img).Save(filename);
                message.Channel.SendFileAsync(filename).Wait();
                File.Delete(filename);
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Sorry, I can't find any image :/`");
            }
        }

        public static async Task Wth(SocketMessage message, string[] arg, string msg)
        {
            string imgLink = "";
            if (arg.Count() > 1)
            {
                imgLink = msg.Substring(6);
            }
            else
            {
                imgLink = await ImageHelper.GetLastImageAsync(message);
            }
            if (imgLink != "")
            {
                Bitmap img = (Bitmap)Image.FromStream(new MemoryStream(new WebClient().DownloadData(imgLink)));
                string filename = $@"cache\{FileUtils.RandomFilename(imgLink)}.png";
                ImageHelper.WTH(img).Save(filename);
                message.Channel.SendFileAsync(filename).Wait();
                File.Delete(filename);
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Sorry, I can't find any image :/`");
            }
        }

        public static async Task Usoab(SocketMessage message, string[] arg, string msg)
        {
            string imgLink = "";
            if (arg.Count() > 1)
            {
                imgLink = msg.Substring(6);
            }
            else
            {
                imgLink = await ImageHelper.GetLastImageAsync(message);
            }
            if (imgLink != "")
            {
                Bitmap img = (Bitmap)Image.FromStream(new MemoryStream(new WebClient().DownloadData(imgLink)));
                string filename = $@"cache\{FileUtils.RandomFilename(imgLink)}.png";
                ImageHelper.USOAB(img).Save(filename);
                message.Channel.SendFileAsync(filename).Wait();
                File.Delete(filename);
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Sorry, I can't find any image :/`");
            }
        }

        public static async Task QR(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                string data = msg.Substring(3);
                string filename = $@"cache\{FileUtils.RandomFilename(data)}.png";
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                qrCode.GetGraphic(20).Save(filename);
                message.Channel.SendFileAsync(filename).Wait();
                File.Delete(filename);
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please enter text for the QR code`");
            }
        }

        public static async Task Elians(SocketMessage message, string[] arg, string msg)
        {
           
            if (arg.Count() > 1)
            {
                string data = msg.Substring(7);
                foreach (char c in data.ToUpper())
                {
                    if ((c > 0x90 || c < 0x41) && c != 0x20)
                    {
                        await message.Channel.SendMessageAsync($":no_entry: `Invalid character: '{c}'`");
                        return;
                    }
                }
                data = data.ToLower();
                Regex rgx = new Regex("[a-zA-Z -]");
                string sanitized = "";
                foreach (Match m in rgx.Matches(data))
                {
                    sanitized += m.Value;
                }
                sanitized = sanitized.TrimStart(' ').TrimEnd(' ');
                string filename = $@"cache\{FileUtils.RandomFilename(data)}.png";
                ImageHelper.ELIANS(sanitized).Save(filename);
                message.Channel.SendFileAsync(filename).Wait();
                File.Delete(filename);
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please enter text to translate into elians script`");
            }
        }
    }
}
