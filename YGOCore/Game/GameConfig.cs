using System;

namespace YGOCore.Game
{
    public class GameConfig
    {
        public int LfList { get; private set; }
        public int Rule { get; private set; }
        public int Mode { get; private set; }
        public bool EnablePriority { get; private set; }
        public bool NoCheckDeck { get; private set; }
        public bool NoShuffleDeck { get; private set; }
        public int StartLp { get; private set; }
        public int StartHand { get; private set; }
        public int DrawCount { get; private set; }
        public int GameTimer { get; private set; }
        public string Name { get; private set; }

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

                Name = list[4];
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