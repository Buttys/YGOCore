using System;

namespace YGOCore.Game
{
    public class GameConfig : IGameConfig
    {
        public int LfList { get; set; }
        public int Rule { get; set; }
        public int Mode { get; set; }
        public bool EnablePriority { get; set; }
        public bool NoCheckDeck { get; set; }
        public bool NoShuffleDeck { get; set; }
        public int StartLp { get; set; }
        public int StartHand { get; set; }
        public int DrawCount { get; set; }
        public int GameTimer { get; set; }
        public string Name { get; set; }

        public GameConfig(string info)
        {
            if (info.ToLower() == "tcg" || info.ToLower() == "ocg" || info.ToLower() == "ocg/tcg" || info.ToLower() == "tcg/ocg")
            {
                LfList = info.ToLower() == "ocg/tcg" ? 1: info.ToLower() == "tcg/ocg" ? 0 : info.ToLower() == "ocg" ? 1 : 0;
                Rule = info.ToLower() == "ocg/tcg" || info.ToLower() == "tcg/ocg" ? 2 : info.ToLower() == "tcg" ? 1 : 0;
                Mode = 0;
                EnablePriority = false;
                NoCheckDeck = false;
                NoShuffleDeck = false;
                StartLp = 8000;
                StartHand = 5;
                DrawCount = 1;
                GameTimer = 120;
                Name = GameManager.RandomRoomName();
            }
            else
                Load(info);
        }

        public GameConfig(GameClientPacket packet)
        {
            LfList = BanlistManager.GetIndex(packet.ReadUInt32());
            Rule = packet.ReadByte();
            Mode = packet.ReadByte();
            EnablePriority = Convert.ToBoolean(packet.ReadByte());
            NoCheckDeck = Convert.ToBoolean(packet.ReadByte());
            NoShuffleDeck = Convert.ToBoolean(packet.ReadByte());
            //C++ padding: 5 bytes + 3 bytes = 8 bytes
            for (int i = 0; i < 3; i++)
                packet.ReadByte();
            StartLp = packet.ReadInt32();
            StartHand = packet.ReadByte();
            DrawCount = packet.ReadByte();
            GameTimer = packet.ReadInt16();
            packet.ReadUnicode(20);
            Name = packet.ReadUnicode(30);
            if (string.IsNullOrEmpty(Name))
                Name = GameManager.RandomRoomName();
        }

        public void Load(string gameinfo)
        {
            try
            {
                string rules = gameinfo.Substring(0, 6);

                Rule = int.Parse(rules[0].ToString());
                Mode = int.Parse(rules[1].ToString());
                GameTimer = int.Parse(rules[2].ToString()) == 0 ? 120 : 60;
                EnablePriority = rules[3] == 'T' || rules[3] == '1';
                NoCheckDeck = rules[4] == 'T' || rules[4] == '1';
                NoShuffleDeck = rules[5] == 'T' || rules[5] == '1';

                string data = gameinfo.Substring(6, gameinfo.Length - 6);

                string[] list = data.Split(',');

                StartLp = int.Parse(list[0]);
                LfList = int.Parse(list[1]);

                StartHand = int.Parse(list[2]);
                DrawCount = int.Parse(list[3]);

                Name = gameinfo;
            }
            catch (Exception)
            {
                LfList = 0;
                Rule = 2;
                Mode = 0;
                EnablePriority = false;
                NoCheckDeck = false;
                NoShuffleDeck = false;
                StartLp = 8000;
                StartHand = 5;
                DrawCount = 1;
                GameTimer = 120;
                Name = GameManager.RandomRoomName();
                return;
            }
        }
    }
}