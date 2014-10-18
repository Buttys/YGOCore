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
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Config = new ServerConfig();
            bool loaded = args.Length > 1 ? Config.Load(args[1]): Config.Load();


            if (Config.SplashScreen == true)
            {

                Logger.WriteLine(" __     _______  ____   _____", false);
                Logger.WriteLine(" \\ \\   / / ____|/ __ \\ / ____|", false);
                Logger.WriteLine("  \\ \\_/ / |  __| |  | | |     ___  _ __ ___", false);
                Logger.WriteLine("   \\   /| | |_ | |  | | |    / _ \\| '__/ _ \\", false);
                Logger.WriteLine("    | | | |__| | |__| | |___| (_) | | |  __/", false);
                Logger.WriteLine("    |_|  \\_____|\\____/ \\_____\\___/|_|  \\___|               Version: " + Version, false);
                Logger.WriteLine(string.Empty, false);

            }
            Logger.WriteLine("Accepting client version 0x" + Config.ClientVersion.ToString("x") + " or higher.");


            if (loaded)
                Console.WriteLine("Config loaded.");
            else
                Console.WriteLine("Unable to load config.txt, using default settings.");


            int coreport = 0;

            if (args.Length > 0)
                int.TryParse(args[0], out coreport);

            Random = new Random();
            
            Server server = new Server();
            if (!server.Start(coreport))
                Thread.Sleep(5000);

            if (server.IsListening == true && Config.Ready == true)
            {
                Logger.WriteLine("::::network-ready");

            }
            while (server.IsListening)
            {
                server.Process();
                Thread.Sleep(1);
          
                
            }
            Logger.WriteLine("::::network-end");
            Process.GetCurrentProcess().Kill();

        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception ?? new Exception();

            File.WriteAllText("crash_" + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt", exception.ToString());

            Process.GetCurrentProcess().Kill();
        }
    }
}
