using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Discord;

namespace DankBot
{
    class ImageHelper
    {
        public static async Task<string> GetLastImageAsync(IMessage message)
        {
            var messages = await message.Channel.GetMessagesAsync(16).Flatten();
            foreach (IMessage ms in messages)
            {
                if (ms.Attachments.Count > 0)
                {
                    foreach (IAttachment a in ms.Attachments)
                    {
                        if (a.Filename.EndsWith(".png") || a.Filename.EndsWith(".jpg") || a.Filename.EndsWith(".bmp"))
                        {
                            return a.Url;
                        }
                    }
                }
                if (ms.Embeds.Count > 0)
                {
                    foreach (IEmbed a in ms.Embeds)
                    {
                        if (a.Url.Contains(".png") || a.Url.Contains(".jpg") || a.Url.Contains(".bmp"))
                        {
                            return a.Url;
                        }
                    }
                }
            }
            return null;
        }

        public static Size GetNewSize(int maxWidth, int maxHeight, Bitmap bmp)
        {
            float nw = bmp.Width;
            float nh = bmp.Height;

            if (bmp.Width > maxWidth && bmp.Height > maxHeight)
            {
                if (bmp.Width > bmp.Height)
                {
                    nw = maxWidth;
                    nh = (maxWidth * bmp.Height) / bmp.Width;
                }
                else
                {
                    nh = maxHeight;
                    nw = (float)maxHeight * (float)((float)bmp.Width / (float)bmp.Height);
                }
            }
            else if (bmp.Width > maxWidth)
            {
                nw = maxWidth;
                nh = (maxWidth * bmp.Height) / bmp.Width;
            }
            else if (bmp.Height > maxHeight)
            {
                nh = maxHeight;
                nw = (float)maxHeight * (float)((float)bmp.Width / (float)bmp.Height);
            }
            return new Size((int)nw, (int)nh);
        }

        public static Bitmap HSGTF(Bitmap bmp)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(@"resources\images\hsgtf.png");
            Graphics g = Graphics.FromImage(img);

            Size newSize = GetNewSize(600, 392, bmp);

            g.DrawImage(bmp, 300 - (newSize.Width / 2), 96 + 196 - (newSize.Height / 2), newSize.Width, newSize.Height);
            return (Bitmap)img;
        }

        public static Bitmap WTH(Bitmap bmp)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(@"resources\images\wth.png");
            Graphics g = Graphics.FromImage(img);

            Size newSize = GetNewSize(158, 177, bmp);

            g.DrawImage(bmp, 79 - (newSize.Width / 2) + 42, 29 +  88 - (newSize.Height / 2), newSize.Width, newSize.Height);
            return (Bitmap)img;
        }

        public static Bitmap USOAB(Bitmap bmp)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(@"resources\images\usoab.png");
            Graphics g = Graphics.FromImage(img);

            Size newSize = GetNewSize(285, 416, bmp);

            g.DrawImage(bmp, 142 - (newSize.Width / 2) + 26, 208 - (newSize.Height / 2) + 14, newSize.Width, newSize.Height);
            return (Bitmap)img;
        }
    }
}
