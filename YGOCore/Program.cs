using System;
using System.Threading;

namespace YGOCore
{
    class Program
    {
        public const int ProVersion = 0x1330;
        const string Version = "0.1 Beta";

        public static Random Random;
        
        static void Main(string[] args)
        {

            Console.WriteLine(" __     _______  ____   _____");            
            Console.WriteLine(" \\ \\   / / ____|/ __ \\ / ____|");              
            Console.WriteLine("  \\ \\_/ / |  __| |  | | |     ___  _ __ ___");
            Console.WriteLine("   \\   /| | |_ | |  | | |    / _ \\| '__/ _ \\");
            Console.WriteLine("    | | | |__| | |__| | |___| (_) | | |  __/");
            Console.WriteLine("    |_|  \\_____|\\____/ \\_____\\___/|_|  \\___|               Version: " + Version);
            Console.WriteLine("");

            int coreport = 0;

            if (args.Length > 0)
                int.TryParse(args[0], out coreport);

            Random = new Random();
            Server server = new Server();
            if (!server.Start(coreport))
                Thread.Sleep(5000);

            while (server.IsListening)
            {
                server.Process();
                Thread.Sleep(1);
            }

        }
    }
}
