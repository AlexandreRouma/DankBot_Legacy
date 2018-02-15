using Discord.WebSocket;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
    }
}
