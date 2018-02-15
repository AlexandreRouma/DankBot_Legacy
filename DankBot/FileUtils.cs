using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DankBot
{
    class FileUtils
    {
        public static string RandomFilename(string salt)
        {
            return Convert.ToBase64String(Encoding.Default.GetBytes(GetSalt() + salt.Substring(0, Math.Min(16, salt.Length - 1)))).Replace("/", "_");
        }

        public static string GetSalt()
        {
            Random rng = new Random();
            string str = "";
            for (int i = 0; i < 10; i++)
            {
                str += (char)rng.Next(65, 128);
            }
            return str;
        }
    }
}
