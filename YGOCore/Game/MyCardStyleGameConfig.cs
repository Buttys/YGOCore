using System;

namespace YGOCore.Game
{
	public class MyCardStyleGameConfig: IGameConfig
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

		public MyCardStyleGameConfig (String info)
		{
			Load (info);
		}

		public void Load (string info)
		{
			try
			{
				if (info.StartsWith ("M#")) {
					Mode = 1;
					LfList = 0;
					Rule = 2;
					EnablePriority = false;
					NoCheckDeck = false;
					NoShuffleDeck = false;
					StartLp = 8000;
					StartHand = 5;
					DrawCount = 1;
					GameTimer = 120;
					Name = info.Substring (2, info.Length - 2);
				} else if (info.StartsWith ("T#")) {
					Mode = 2;
					LfList = 0;
					Rule = 2;
					EnablePriority = false;
					NoCheckDeck = false;
					NoShuffleDeck = false;
					StartLp = 8000;
					StartHand = 5;
					DrawCount = 1;
					GameTimer = 120;
					Name = info.Substring (2, info.Length - 2);
				} else {
					string[] ParamSeg = info.Split (',');
					if (ParamSeg.Length == 4) {
						if (ParamSeg[0].Length >= 6) {
							string ConfigStr = ParamSeg[0];
							Rule = int.Parse (ConfigStr[0].ToString());
							Mode = int.Parse (ConfigStr[1].ToString());
							EnablePriority = ConfigStr[2].Equals('T');
							NoCheckDeck =  ConfigStr[3].Equals('T');
							NoShuffleDeck = ConfigStr[4].Equals('T');
							StartLp = int.Parse (ConfigStr.Substring(5, ConfigStr.Length - 5));
							StartHand = int.Parse(ParamSeg[1]);
							DrawCount = int.Parse(ParamSeg[2]);
							Name = ParamSeg[3];
							GameTimer = 120;
							LfList = 0;
						} else {
							throw new Exception();
						}
					} else {
						Mode = 0;
						LfList = 0;
						Rule = 2;
						EnablePriority = false;
						NoCheckDeck = false;
						NoShuffleDeck = false;
						StartLp = 8000;
						StartHand = 5;
						DrawCount = 1;
						GameTimer = 120;
						Name = info; 
					}
				}
			} catch (Exception) {
				Mode = 0;
				LfList = 0;
				Rule = 2;
				EnablePriority = false;
				NoCheckDeck = false;
				NoShuffleDeck = false;
				StartLp = 8000;
				StartHand = 5;
				DrawCount = 1;
				GameTimer = 120;
				Name = info; 
				return;
			}
		}
	}
}

