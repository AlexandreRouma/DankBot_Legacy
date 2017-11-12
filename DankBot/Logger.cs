using System;

namespace DankBot
{
    class Logger
    {
        /// <summary>
        /// Logs a string
        /// </summary>
        /// <param name="str">Text to write</param>
        /// <param name="lineColor">Color to write the text with (optional)</param>
        public static void Write(string str, ConsoleColor lineColor = new ConsoleColor())
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.Write($"[{DateTime.Now.ToLongTimeString()}] ");
            if (lineColor != new ConsoleColor())
                Console.ForegroundColor = lineColor;
            Console.Write(str);
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Logs a string then goes to the next line
        /// </summary>
        /// <param name="str">Text to write</param>
        /// <param name="lineColor">Color to write the text with (optional)</param>
        public static void WriteLine(string str, ConsoleColor lineColor = new ConsoleColor())
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.Write($"[{DateTime.Now.ToLongTimeString()}] ");
            if (lineColor != new ConsoleColor())
                Console.ForegroundColor = lineColor;
            Console.WriteLine(str);
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Writes "[  OK  ]"
        /// </summary>
        public static void OK()
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.Write("[  ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("OK");
            Console.ForegroundColor = oldColor;
            Console.WriteLine("  ]");
        }

        /// <summary>
        /// Writes "[FAILED]"
        /// </summary>
        public static void FAILED()
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("FAILED");
            Console.ForegroundColor = oldColor;
            Console.WriteLine("]");
        }
    }
}
