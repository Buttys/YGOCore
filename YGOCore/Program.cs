using System;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace YGOCore
{
    class Program
    {
        const string Version = "0.2 Beta";

        public static ServerConfig Config { get; private set; }
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

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            int coreport = 0;

            if (args.Length > 0)
                int.TryParse(args[0], out coreport);

            Random = new Random();
            Config = new ServerConfig();
            Server server = new Server();
            if (!server.Start(coreport))
                Thread.Sleep(5000);

            while (server.IsListening)
            {
                server.Process();
                Thread.Sleep(1);
            }

        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception ?? new Exception();

            File.WriteAllText("crash_" + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt", exception.ToString());

            Process.GetCurrentProcess().Kill();
        }
    }
}
