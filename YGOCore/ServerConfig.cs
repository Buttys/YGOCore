using System.IO;
using System;
using System.Globalization;
namespace YGOCore
{
    public class ServerConfig
    {
        public int ServerPort { get; private set; }
        public string Path { get; private set; }
        public string ScriptFolder { get; private set; }
        public string CardCDB { get; private set; }
        public string BanlistFile { get; private set; }
        public bool Log { get; private set; }
        public bool ConsoleLog { get; private set; }
        public bool HandShuffle { get; private set; }
        public bool AutoEndTurn { get; private set; }
        public int ClientVersion { get; private set; }
	public bool SplashScreen { get; private set; }
	public bool Ready { get; private set; }
        public bool STDOUT { get; private set; }


			 
			   
			 
        public ServerConfig()
        {
            ClientVersion = 0x1332;
            ServerPort = 8911;
            Path = ".";
            ScriptFolder = "script";
            CardCDB = "cards.cdb";
            BanlistFile = "lflist.conf";
            Log = true;
            ConsoleLog = true;
            HandShuffle = false;
            AutoEndTurn = true;
			SplashScreen = true;
			Ready = false;
            STDOUT = false;	
        }

        public bool Load(string file = "config.txt")
        {
            if (File.Exists(file))
            {
                StreamReader reader = null;
                try
                {
                    reader = new StreamReader(File.OpenRead(file));
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line == null) continue;
                        line = line.Trim();
                        if (line.Equals(string.Empty)) continue;
                        if (!line.Contains("=")) continue;
                        if (line.StartsWith("#")) continue;

                        string[] data = line.Split(new[] { '=' }, 2);
                        string variable = data[0].Trim().ToLower();
                        string value = data[1].Trim();
                        switch (variable)
                        {
                            case "serverport":
                                ServerPort = Convert.ToInt32(value);
                                break;
                            case "path":
                                Path = value;
                                break;
                            case "scriptfolder":
                                ScriptFolder = value;
                                break;
                            case "cardcdb":
                                CardCDB = value;
                                break;
                            case "banlist":
                                BanlistFile = value;
                                break;
                            case "errorlog":
                                Log = Convert.ToBoolean(value);
                                break;
                            case "consolelog":
                                ConsoleLog = Convert.ToBoolean(value);
                                break;
                            case "handshuffle":
                                HandShuffle = Convert.ToBoolean(value);
                                break;
                            case "autoendturn":
                                AutoEndTurn = Convert.ToBoolean(value);
                                break;
                            case "clientversion":
                                ClientVersion = Convert.ToInt32(value, 16);
                                break;
							case "splashscreen":
                                SplashScreen = Convert.ToBoolean(value);
                                break;
							case "stdoutsupport":
                                STDOUT = Convert.ToBoolean(value);
                                break;    
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex);
                    reader.Close();
                    return false;
                }
                reader.Close();
                if (HandShuffle)
                    Logger.WriteLine("Warning: Hand shuffle requires a custom ocgcore to work.");
                return true;
            }

            return false;
        }

    }
}
