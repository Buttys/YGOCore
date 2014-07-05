using System;
using System.IO;

namespace YGOCore
{
    public static class Logger
    {
        public static bool EnableLog { get; set; }

        public static void WriteLine(object text)
        {
            Console.WriteLine("[Log] " + text);
        }

        public static void WriteError(object error)
        {
            Console.WriteLine("[Error] " + error);
            WriteLog("[Error] " + error);
        }

        public static void WriteLine(string type, object text)
        {
            Console.WriteLine("[" + type + "] " + text);
        }

        private static void WriteLog(string text)
        {
            if (EnableLog)
            {
                try
                {
                    StreamWriter writer = new StreamWriter("ErrorLog.txt", true);
                    writer.WriteLine(text);
                    writer.Close();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
