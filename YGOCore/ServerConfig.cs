namespace YGOCore
{
    public class ServerConfig
    {
        public int ServerPort { get; set; }
        public string Path { get; set; }
        public string ScriptFolder { get; set; }
        public string CardCDB { get; set; }
        public string BanlistFile { get; set; }

        public ServerConfig()
        {
            ServerPort = 8911;
            Path = ".";
            ScriptFolder = "script";
            CardCDB = "cards.cdb";
            BanlistFile = "lflist.conf";
        }

    }
}
