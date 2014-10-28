using YGOCore.Game.Enums;

namespace YGOCore.Game
{
    public class Player
    {
        public Game Game { get; private set; }
        public string Name { get; private set; }
        public bool IsAuthentified { get; private set; }
        public int Type { get; set; }
        public int TurnSkip { get; set; }
        public Deck Deck { get; private set; }
        public PlayerState State { get; set; }
        private GameClient m_client;

        public Player(GameClient client)
        {
            Game = client.Game;
            Type = (int)PlayerType.Undefined;
            State = PlayerState.None;
            m_client = client;
            TurnSkip = 0;
        }

        public void Send(GameServerPacket packet)
        {
            m_client.Send(packet.GetContent());
        }

        public void Disconnect()
        {
            m_client.Close();
        }

        public void OnDisconnected()
        {
            if (IsAuthentified)
                Game.RemovePlayer(this);
        }

        public void SendTypeChange()
        {
            GameServerPacket packet = new GameServerPacket(StocMessage.TypeChange);
            packet.Write((byte)(Type + (Game.HostPlayer.Equals(this) ? (int)PlayerType.Host : 0)));
            Send(packet);
        }

        public bool Equals(Player player)
        {
            return ReferenceEquals(this, player);
        }

        public void Parse(GameClientPacket packet)
        {
            CtosMessage msg = packet.ReadCtos();
            switch (msg)
            {
                case CtosMessage.PlayerInfo:
                    OnPlayerInfo(packet);
                    break;
				case CtosMessage.JoinGame:
					if (Program.Config.MyCard)
					{
						onJoinMyCardStyleGame(packet);
					}
					else
					{
						OnJoinGame(packet);
					}
					break;
				case CtosMessage.CreateGame:
                    OnCreateGame(packet);
                    break;
            }
            if (!IsAuthentified)
                return;
            switch (msg)
            {
                case CtosMessage.Chat:
                    OnChat(packet);
                    break;
                case CtosMessage.HsToDuelist:
                    Game.MoveToDuelist(this);
                    break;
                case CtosMessage.HsToObserver:
                    Game.MoveToObserver(this);
                    break;
                case CtosMessage.LeaveGame:
                    Game.RemovePlayer(this);
                    break;
                case CtosMessage.HsReady:
                    Game.SetReady(this, true);
                    break;
                case CtosMessage.HsNotReady:
                    Game.SetReady(this, false);
                    break;
                case CtosMessage.HsKick:
                    OnKick(packet);
                    break;
                case CtosMessage.HsStart:
                    Game.StartDuel(this);
                    break;
                case CtosMessage.HandResult:
                    OnHandResult(packet);
                    break;
                case CtosMessage.TpResult:
                    OnTpResult(packet);
                    break;
                case CtosMessage.UpdateDeck:
                    OnUpdateDeck(packet);
                    break;
                case CtosMessage.Response:
                    OnResponse(packet);
                    break;
                case CtosMessage.Surrender:
                    Game.Surrender(this, 0);
                    break;
            }
        }

        private void OnPlayerInfo(GameClientPacket packet)
        {
            if (Name != null)
                return;
            Name = packet.ReadUnicode(20);

            if (string.IsNullOrEmpty(Name))
                LobbyError("Username Required");
        }

        private void OnCreateGame(GameClientPacket packet)
        {
            if (string.IsNullOrEmpty(Name) || Type != (int)PlayerType.Undefined)
                return;

            GameRoom room = null;

            room = GameManager.CreateOrGetGame(new GameConfig(packet));

            if (room == null)
            {
                LobbyError("Server Full");
                return;
            }

            Game = room.Game;
            Game.AddPlayer(this);
            IsAuthentified = true;
            
        }

		private void onJoinMyCardStyleGame(GameClientPacket packet)
		{
			if (string.IsNullOrEmpty(Name) || Type != (int)PlayerType.Undefined)
				return;
			int version = packet.ReadInt16();
			if (version < Program.Config.ClientVersion)
			{
				LobbyError("Version too low");
				return;
			}
			else if (version > Program.Config.ClientVersion)
				ServerMessage("Warning: client version is higher than servers.");
			packet.ReadInt32();//gameid
			packet.ReadInt16();

			string joinCommand = packet.ReadUnicode(60);

			GameRoom room = null;

			if (GameManager.GameExists(joinCommand))
				room = GameManager.GetGame(joinCommand);
			else
				room = GameManager.CreateOrGetGame(new MyCardStyleGameConfig(joinCommand));

			if (room == null)
			{
				LobbyError("Server Full");
				return;
			}
			if (!room.IsOpen)
			{
				LobbyError("Game Finished");
				return;
			}

			Game = room.Game;
			Game.AddPlayer(this);
			IsAuthentified = true;

		}

        private void OnJoinGame(GameClientPacket packet)
        {
            if (string.IsNullOrEmpty(Name) || Type != (int)PlayerType.Undefined)
                return;

            int version = packet.ReadInt16();
            if (version < Program.Config.ClientVersion)
            {
                LobbyError("Version too low");
                return;
            }
            else if (version > Program.Config.ClientVersion)
                ServerMessage("Warning: client version is higher than servers.");
                

            packet.ReadInt32();//gameid
            packet.ReadInt16();

            string joinCommand = packet.ReadUnicode(60);

            GameRoom room = null;

            if (GameManager.GameExists(joinCommand))
                room = GameManager.GetGame(joinCommand);
            else if (joinCommand.ToLower() == "random")
            {
                room = GameManager.GetRandomGame();

                if (room == null)
                {
                    LobbyError("No Games");
                    return;
                }

            }
            else if (joinCommand.ToLower() == "spectate")
            {
                room = GameManager.SpectateRandomGame();

                if (room == null)
                {
                    LobbyError("No Games");
                    return;
                }
            }
            else if (string.IsNullOrEmpty(joinCommand) || joinCommand.ToLower() == "tcg" || joinCommand.ToLower() == "ocg"
                || joinCommand.ToLower() == "ocg/tcg" || joinCommand.ToLower() == "tcg/ocg")
            {
                int filter = string.IsNullOrEmpty(joinCommand) ? -1 : 
                    (joinCommand.ToLower() == "ocg/tcg" || joinCommand.ToLower() == "tcg/ocg") ? 2 : 
                    joinCommand.ToLower() == "tcg" ? 1 : 0;

                room = GameManager.GetRandomGame(filter);
                if (room == null) //if no games just make a new one!!
                    room = GameManager.CreateOrGetGame(new GameConfig(joinCommand));
            }
            else
                room = GameManager.CreateOrGetGame(new GameConfig(joinCommand));

            if (room == null)
            {
                LobbyError("Server Full");
                return;
            }
            if (!room.IsOpen)
            {
                LobbyError("Game Finished");
                return;
            }

            Game = room.Game;
            Game.AddPlayer(this);
            IsAuthentified = true;
        }

        private void OnChat(GameClientPacket packet)
        {
            string msg = packet.ReadUnicode(256);
            Game.Chat(this, msg);
        }

        private void OnKick(GameClientPacket packet)
        {
            int pos = packet.ReadByte();
            Game.KickPlayer(this, pos);
        }

        private void OnHandResult(GameClientPacket packet)
        {
            int res = packet.ReadByte();
            Game.HandResult(this, res);
        }

        private void OnTpResult(GameClientPacket packet)
        {
            bool tp = packet.ReadByte() != 0;
            Game.TpResult(this, tp);
        }

        private void OnUpdateDeck(GameClientPacket packet)
        {
            if (Type == (int)PlayerType.Observer)
                return;
            Deck deck = new Deck();
            int main = packet.ReadInt32();
            int side = packet.ReadInt32();
            for (int i = 0; i < main; i++)
                deck.AddMain(packet.ReadInt32());
            for (int i = 0; i < side; i++)
                deck.AddSide(packet.ReadInt32());
            if (Game.State == GameState.Lobby)
            {
                Deck = deck;
                Game.IsReady[Type] = false;
            }
            else if (Game.State == GameState.Side)
            {
                if (Game.IsReady[Type])
                    return;
                if (!Deck.Check(deck))
                {
                    GameServerPacket error = new GameServerPacket(StocMessage.ErrorMsg);
                    error.Write((byte)3);
                    error.Write(0);
                    Send(error);
                    return;
                }
                Deck = deck;
                Game.IsReady[Type] = true;
                Game.ServerMessage(Name + " is ready.");
                Send(new GameServerPacket(StocMessage.DuelStart));
                Game.MatchSide();
            }
        }

        private void OnResponse(GameClientPacket packet)
        {
            if (Game.State != GameState.Duel)
                return;
            if (State != PlayerState.Response)
                return;
            byte[] resp = packet.ReadToEnd();
            if (resp.Length > 64)
                return;
            State = PlayerState.None;
            Game.SetResponse(resp);
        }

        private void LobbyError(string message)
        {
            GameServerPacket join = new GameServerPacket(StocMessage.JoinGame);
            join.Write(0U);
            join.Write((byte)0);
            join.Write((byte)0);
            join.Write(0);
            join.Write(0);
            join.Write(0);
            // C++ padding: 5 bytes + 3 bytes = 8 bytes
            for (int i = 0; i < 3; i++)
                join.Write((byte)0);
            join.Write(0);
            join.Write((byte)0);
            join.Write((byte)0);
            join.Write((short)0);
            Send(join);

            GameServerPacket enter = new GameServerPacket(StocMessage.HsPlayerEnter);
            enter.Write("[" + message + "]", 20);
            enter.Write((byte)0);
            Send(enter);
        }

        private void ServerMessage(string msg)
        {
            string finalmsg = "[Server] " + msg;
            GameServerPacket packet = new GameServerPacket(StocMessage.Chat);
            packet.Write((short)PlayerType.Yellow);
            packet.Write(finalmsg, finalmsg.Length + 1);
            Send(packet);
        }
    }
}