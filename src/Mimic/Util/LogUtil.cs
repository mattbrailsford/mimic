using System;

namespace Mimic.Util
{
    public class LogUtil
    {
        public static void Info(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("[Info] " + msg);
        }

        public static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Error] " + msg);
        }

        public static void Warning(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[Warning] " + msg);
        }

        public static void Success(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Success] " + msg);
        }
    }
}
