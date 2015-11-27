using System;

namespace YGOCore.Game
{
	public interface IGameConfig
	{
		int LfList { get; set; }
		int Rule { get; set; }
		int Mode { get; set; }
		bool EnablePriority { get; set; }
		bool NoCheckDeck { get; set; }
		bool NoShuffleDeck { get; set; }
		int StartLp { get; set; }
		int StartHand { get; set; }
		int DrawCount { get; set; }
		int GameTimer { get; set; }
		string Name { get; set; }
		void Load(String info);
	}
}

