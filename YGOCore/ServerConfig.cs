using System.IO;
using System;
namespace YGOCore
{
    public class ServerConfig
    {
        public int ServerPort { get; set; }
        public string Path { get; set; }
        public string ScriptFolder { get; set; }
        public string CardCDB { get; set; }
        public string BanlistFile { get; set; }
        public bool EnableLog { get; set; }

        public ServerConfig()
        {
            ServerPort = 8911;
            Path = ".";
            ScriptFolder = "script";
            CardCDB = "cards.cdb";
            BanlistFile = "lflist.conf";
            EnableLog = true;
            Logger.EnableLog = EnableLog;
        }

        public bool Load(string file = "config.txt")
        {
            if (File.Exists(file))
            {
                var reader = new StreamReader(File.OpenRead(file));
                try
                {
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
                                EnableLog = Convert.ToBoolean(value);
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                    reader.Close();
                    return false;
                }
                reader.Close();
                Logger.EnableLog = EnableLog;
                return true;
            }

            return false;
        }

    }
}
