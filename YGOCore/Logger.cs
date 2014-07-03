using System;

namespace YGOCore
{
    public static class Logger
    {

        public static bool EnableLog { get; set; }

        public static void WriteLine(object text)
        {
            Console.WriteLine("[Log] " + text);
            WriteLog("[Log] " + text);
        }

        public static void WriteError(object error)
        {
            Console.WriteLine("[Error] " + error);
            WriteLog("[Error] " + error);
        }

        public static void WriteLine(string type, object text)
        {
            Console.WriteLine("[" + type + "] " + text);
            WriteLog("[" + type + "] " + text);
        }

        private static void WriteLog(string text)
        {
            if (EnableLog)
            {
                //write to file
            }
        }
    }
}
