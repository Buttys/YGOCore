using System;
using System.Collections.Generic;
using System.IO;
using OcgWrapper;
using OcgWrapper.Enums;
using YGOCore.Game.Enums;
using YGOCore;

namespace YGOCore.Game
{
    public class Game
    {
        public IGameConfig Config { get; private set; }
        public Banlist Banlist { get; private set; }
        public bool IsMatch { get; private set; }
        public bool IsTag { get; private set; }
        public bool IsTpSelect { get; private set; }

        public GameState State { get; private set; }
        public DateTime SideTimer { get; private set; }
        public DateTime TpTimer { get; private set; }
        public DateTime RPSTimer { get; private set; }
        public int TurnCount { get; set; }
        public int CurrentPlayer { get; set; }
        public int[] LifePoints { get; set; }

        public Player[] Players { get; private set; }
        public Player[] CurPlayers { get; private set; }
        public bool[] IsReady { get; private set; }
        public List<Player> Observers { get; private set; }
        public Player HostPlayer { get; private set; }

        public Replay Replay { get; private set; }

        private GameRoom m_room;
        private Duel m_duel;
        private GameAnalyser m_analyser;
        private int[] m_handResult;
        private int m_startplayer;
        private int m_lastresponse;

        private int[] m_timelimit;
        private int[] m_bonustime;
        private DateTime? m_time;

        private int[] m_matchResult;
        private int m_duelCount;
        private bool m_matchKill;
        private bool m_swapped;

        public Game(GameRoom room, IGameConfig config)
        {
            Config = config;
            State = GameState.Lobby;
            IsMatch = config.Mode == 1;
            IsTag = config.Mode == 2;
            CurrentPlayer = 0;
            LifePoints = new int[2];
            Players = new Player[IsTag ? 4 : 2];
            CurPlayers = new Player[2];
            IsReady = new bool[IsTag ? 4 : 2];
            m_handResult = new int[2];
            m_timelimit = new int[2];
            m_bonustime = new int[2];
            m_matchResult = new int[6];
            Observers = new List<Player>();
            if (config.LfList >= 0 && config.LfList < BanlistManager.Banlists.Count)
                Banlist = BanlistManager.Banlists[config.LfList];
            m_room = room;
            m_analyser = new GameAnalyser(this);
        }

        public void ReloadGameConfig(string gameinfo)
        {
            Config.Load(gameinfo);
            IsMatch = Config.Mode == 1;
            IsTag = Config.Mode == 2;
            Players = new Player[IsTag ? 4 : 2];
            IsReady = new bool[IsTag ? 4 : 2];
            if (Config.LfList >= 0 && Config.LfList < BanlistManager.Banlists.Count)
                Banlist = BanlistManager.Banlists[Config.LfList];
            LifePoints[0] = Config.StartLp;
            LifePoints[1] = Config.StartLp;

        }

        public void SendToAll(GameServerPacket packet)
        {
            SendToPlayers(packet);
            SendToObservers(packet);
        }

        public void SendToAllBut(GameServerPacket packet, Player except)
        {
            foreach (Player player in Players)
                if (player != null && !player.Equals(except))
                    player.Send(packet);
            foreach (Player player in Observers)
                if (!player.Equals(except))
                    player.Send(packet);
        }

        public void SendToAllBut(GameServerPacket packet, int except)
        {
            if (except < CurPlayers.Length)
                SendToAllBut(packet, CurPlayers[except]);
            else
                SendToAll(packet);
        }

        public void SendToPlayers(GameServerPacket packet)
        {
            foreach (Player player in Players)
                if (player != null)
                    player.Send(packet);
        }

        public void SendToObservers(GameServerPacket packet)
        {
            foreach (Player player in Observers)
                player.Send(packet);
        }

        public void SendToTeam(GameServerPacket packet, int team)
        {
            if (!IsTag)
                Players[team].Send(packet);
            else if (team == 0)
            {
                Players[0].Send(packet);
                Players[1].Send(packet);
            }
            else
            {
                Players[2].Send(packet);
                Players[3].Send(packet);
            }
        }

        public void AddPlayer(Player player)
        {
            int pos = GetAvailablePlayerPos();
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::join-slot|{2}|{1}|{0}", player.Name, pos, Config.Name);


            if (State != GameState.Lobby)
            {
                player.Type = (int)PlayerType.Observer;
                if (State == GameState.End)
                    return;
                SendJoinGame(player);
                player.SendTypeChange();
                player.Send(new GameServerPacket(StocMessage.DuelStart));
                Observers.Add(player);


                if (State == GameState.Duel)
                    InitNewSpectator(player);
                return;
            }

            if (HostPlayer == null)
                HostPlayer = player;

           
            if (pos != -1)
            {
                GameServerPacket enter = new GameServerPacket(StocMessage.HsPlayerEnter);
                enter.Write(player.Name, 20);
                enter.Write((byte)pos);
                SendToAll(enter);

                Players[pos] = player;
                IsReady[pos] = false;
                player.Type = pos;

                
            }
            else
            {
                GameServerPacket watch = new GameServerPacket(StocMessage.HsWatchChange);
                watch.Write((short)(Observers.Count + 1));
                if (Program.Config.STDOUT == true)
                Console.WriteLine("::::spectator|{1}|{0}", Observers.Count,Config.Name);
                SendToAll(watch);

                player.Type = (int)PlayerType.Observer;
                Observers.Add(player);
               
            }

            SendJoinGame(player);
            player.SendTypeChange();

            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i] != null)
                {
                    GameServerPacket enter = new GameServerPacket(StocMessage.HsPlayerEnter);
                    enter.Write(Players[i].Name, 20);
                    enter.Write((byte)i);
                    player.Send(enter);

                    if (IsReady[i])
                    {
                        GameServerPacket change = new GameServerPacket(StocMessage.HsPlayerChange);
                        change.Write((byte)((i << 4) + (int)PlayerChange.Ready));
                        player.Send(change);
                    }
                }
            }

            if (Observers.Count > 0)
            {
                GameServerPacket nwatch = new GameServerPacket(StocMessage.HsWatchChange);
                nwatch.Write((short)Observers.Count);
                player.Send(nwatch);
                if (Program.Config.STDOUT == true)
                    Console.WriteLine("::::spectator|{1}|{0}", Observers.Count, Config.Name);
            }
        }

        public void RemovePlayer(Player player)
        {
            if (player.Equals(HostPlayer) && State == GameState.Lobby)
                m_room.Close();
            else if (player.Type == (int)PlayerType.Observer)
            {
                Observers.Remove(player);
                if (State == GameState.Lobby)
                {
                    GameServerPacket nwatch = new GameServerPacket(StocMessage.HsWatchChange);
                    nwatch.Write((short)Observers.Count);
                    SendToAll(nwatch);
                    if (Program.Config.STDOUT == true)
                        Console.WriteLine("::::spectator|{1}|{0}", Observers.Count,Config.Name);
                }
             //   if (Program.Config.STDOUT == true)        
             //       Console.WriteLine("{0} - disconnected", player.Name); //Need API
                player.Disconnect();
            }
            else if (State == GameState.Lobby)
            {
                Players[player.Type] = null;
                IsReady[player.Type] = false;

                GameServerPacket change = new GameServerPacket(StocMessage.HsPlayerChange);
                change.Write((byte)((player.Type << 4) + (int)PlayerChange.Leave));
                if (Program.Config.STDOUT == true)
                    Console.WriteLine("::::left-slot|{2}|{1}|{0}", player.Name, player.Type, Config.Name);

                SendToAll(change);
                player.Disconnect();
            }
            else
                Surrender(player, 4, true);
        }

        public void MoveToDuelist(Player player)
        {
            if (State != GameState.Lobby)
                return;
            int pos = GetAvailablePlayerPos();
            
            if (pos == -1)
                return;
            if (player.Type != (int)PlayerType.Observer)
            {
                if (!IsTag || IsReady[player.Type])
                    return;

                pos = (player.Type + 1) % 4;
                while (Players[pos] != null)
                    pos = (pos + 1) % 4;

                GameServerPacket change = new GameServerPacket(StocMessage.HsPlayerChange);
                if (Program.Config.STDOUT == true)
                    Console.WriteLine("::::left-slot|{2}|{1}|{0}", player.Name, player.Type, Config.Name);
                change.Write((byte)((player.Type << 4) + pos));
                SendToAll(change);

                Players[player.Type] = null;
                Players[pos] = player;
                player.Type = pos;
                player.SendTypeChange();
            }
            else
            {
                Observers.Remove(player);
                Players[pos] = player;
                player.Type = pos;

                GameServerPacket enter = new GameServerPacket(StocMessage.HsPlayerEnter);
                enter.Write(player.Name, 20);
                enter.Write((byte)pos);
                SendToAll(enter);

                GameServerPacket nwatch = new GameServerPacket(StocMessage.HsWatchChange);
                nwatch.Write((short)Observers.Count);
                
                SendToAll(nwatch);

                player.SendTypeChange();
            }
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::join-slot|{2}|{1}|{0}", player.Name, pos, Config.Name);
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::spectator|{1}|{0}", Observers.Count, Config.Name);
        }

        public void MoveToObserver(Player player)
        {
            if (State != GameState.Lobby)
                return;
            if (player.Type == (int)PlayerType.Observer)
                return;
            if (IsReady[player.Type])
                return;
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::left-slot|{2}|{1}|{0}", player.Name, player.Type, Config.Name);
            Players[player.Type] = null;
            IsReady[player.Type] = false;
            Observers.Add(player);

            GameServerPacket change = new GameServerPacket(StocMessage.HsPlayerChange);
            change.Write((byte)((player.Type << 4) + (int)PlayerChange.Observe));
            SendToAll(change);

            player.Type = (int)PlayerType.Observer;
            player.SendTypeChange();
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::spectator|{1}|{0}", Observers.Count, Config.Name);
        }

        public void Chat(Player player, string msg)
        {
            GameServerPacket packet = new GameServerPacket(StocMessage.Chat);
            packet.Write((short)player.Type);
            if (player.Type == (int)PlayerType.Observer)
                msg = player.Name + ": " + msg;
            packet.Write(msg, msg.Length + 1);
            SendToAllBut(packet, player);
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::chat|{2}|{0}|{1}", player.Name, msg, Config.Name);
        }

        public void ServerMessage(string msg)
        {
            string finalmsg = "[Server] " + msg;
            GameServerPacket packet = new GameServerPacket(StocMessage.Chat);
            packet.Write((short)PlayerType.Yellow);
            packet.Write(finalmsg, finalmsg.Length + 1);
            SendToAll(packet);
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::chat|{1}|[Server]|{0}", msg, Config.Name);
        }

        public void SetReady(Player player, bool ready)
        {
            if (State != GameState.Lobby)
                return;
            if (player.Type == (int)PlayerType.Observer)
                return;
            if (IsReady[player.Type] == ready)
                return;

            if (ready)
            {
                bool ocg = Config.Rule == 0 || Config.Rule == 2;
                bool tcg = Config.Rule == 1 || Config.Rule == 2;
                int result = 1;
                if (Config.NoCheckDeck)
                    result = 0;
                else if (player.Deck != null)
                    result = player.Deck.Check(Banlist, ocg, tcg);
                if (result != 0)
                {
                    GameServerPacket rechange = new GameServerPacket(StocMessage.HsPlayerChange);
                    rechange.Write((byte)((player.Type << 4) + (int)(PlayerChange.NotReady)));
                    player.Send(rechange);
                    GameServerPacket error = new GameServerPacket(StocMessage.ErrorMsg);
                    error.Write((byte)2); // ErrorMsg.DeckError
                    // C++ padding: 1 byte + 3 bytes = 4 bytes
                    for (int i = 0; i < 3; i++)
                        error.Write((byte)0);
                    error.Write(result);
                    player.Send(error);
                    return;
                }
            }

            IsReady[player.Type] = ready;

            GameServerPacket change = new GameServerPacket(StocMessage.HsPlayerChange);
            change.Write((byte)((player.Type << 4) + (int)(ready ? PlayerChange.Ready : PlayerChange.NotReady)));
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::lock-slot|{2}|{1}|{0}", ready, player.Type, Config.Name);
            SendToAll(change);
        }

        public void KickPlayer(Player player, int pos)
        {
            if (State != GameState.Lobby)
                return;
            if (pos >= Players.Length || !player.Equals(HostPlayer) || player.Equals(Players[pos]) || Players[pos] == null)
                return;
            RemovePlayer(Players[pos]);
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::left-slot|{2}|{1}|{0}", player.Name, pos, Config.Name);
        }

        public void StartDuel(Player player)
        {
            if (State != GameState.Lobby)
                return;
            if (!player.Equals(HostPlayer))
                return;
            for (int i = 0; i < Players.Length; i++)
            {
                if (!IsReady[i])
                    return;
                if (Players[i] == null)
                    return;
            }

            State = GameState.Hand;
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::startduel|{0}", Config.Name);

            SendToAll(new GameServerPacket(StocMessage.DuelStart));

            SendHand();
        }

        public void HandResult(Player player, int result)
        {
            if (State != GameState.Hand)
                return;
            if (player.Type == (int)PlayerType.Observer)
                return;
            if (result < 1 || result > 3)
                return;
            if (IsTag && player.Type != 0 && player.Type != 2)
                return;
            int type = player.Type;
            if (IsTag && player.Type == 2)
                type = 1;
            if (m_handResult[type] != 0)
                return;
            m_handResult[type] = result;
            if (m_handResult[0] != 0 && m_handResult[1] != 0)
            {
                GameServerPacket packet = new GameServerPacket(StocMessage.HandResult);
                packet.Write((byte)m_handResult[0]);
                packet.Write((byte)m_handResult[1]);
                SendToTeam(packet, 0);
                SendToObservers(packet);

                packet = new GameServerPacket(StocMessage.HandResult);
                packet.Write((byte)m_handResult[1]);
                packet.Write((byte)m_handResult[0]);
                SendToTeam(packet, 1);

                if (m_handResult[0] == m_handResult[1])
                {
                    m_handResult[0] = 0;
                    m_handResult[1] = 0;
                    SendHand();
                    return;
                }
                if ((m_handResult[0] == 1 && m_handResult[1] == 2) ||
                    (m_handResult[0] == 2 && m_handResult[1] == 3) ||
                    (m_handResult[0] == 3 && m_handResult[1] == 1))
                    m_startplayer = IsTag ? 2 : 1;
                else
                    m_startplayer = 0;
                State = GameState.Starting;
                Players[m_startplayer].Send(new GameServerPacket(StocMessage.SelectTp));
            }
        }

        public void TpResult(Player player, bool result)
        {
            if (State != GameState.Starting)
                return;
            if (player.Type != m_startplayer)
                return;

            m_swapped = false;
            if (result && player.Type == (IsTag ? 2 : 1) || !result && player.Type == 0)
            {
                m_swapped = true;
                if (IsTag)
                {
                    Player temp = Players[0];
                    Players[0] = Players[2];
                    Players[2] = temp;

                    temp = Players[1];
                    Players[1] = Players[3];
                    Players[3] = temp;

                    Players[0].Type = 0;
                    Players[1].Type = 1;
                    Players[2].Type = 2;
                    Players[3].Type = 3;
                }
                else
                {
                    Player temp = Players[0];
                    Players[0] = Players[1];
                    Players[1] = temp;
                    Players[0].Type = 0;
                    Players[1].Type = 1;
                }
            }
            CurPlayers[0] = Players[0];
            CurPlayers[1] = Players[IsTag ? 3 : 1];

            State = GameState.Duel;
            int seed = Environment.TickCount;
            m_duel = Duel.Create((uint)seed);
            Random rand = new Random(seed);

            m_duel.SetAnalyzer(m_analyser.Analyse);
            m_duel.SetErrorHandler(HandleError);

            m_duel.InitPlayers(Config.StartLp, Config.StartHand, Config.DrawCount);

            int opt = 0;
            if (Config.EnablePriority)
                opt += 0x08;
            if (Config.NoShuffleDeck)
                opt += 0x10;
            if (IsTag)
                opt += 0x20;

            Replay = new Replay((uint)seed, IsTag);
            Replay.Writer.WriteUnicode(Players[0].Name, 20);
            Replay.Writer.WriteUnicode(Players[1].Name, 20);
            if (IsTag)
            {
                Replay.Writer.WriteUnicode(Players[2].Name, 20);
                Replay.Writer.WriteUnicode(Players[3].Name, 20);
            }
            Replay.Writer.Write(Config.StartLp);
            Replay.Writer.Write(Config.StartHand);
            Replay.Writer.Write(Config.DrawCount);
            Replay.Writer.Write(opt);

            for (int i = 0; i < Players.Length; i++)
            {
                Player dplayer = Players[i == 2 ? 3 : (i == 3 ? 2 : i)];
                int pid = i;
                if (IsTag)
                    pid = i >= 2 ? 1 : 0;
                if (!Config.NoShuffleDeck)
                {
                    IList<int> cards = ShuffleCards(rand, dplayer.Deck.Main);
                    Replay.Writer.Write(cards.Count);
                    foreach (int id in cards)
                    {
                        if (IsTag && (i == 1 || i == 3))
                            m_duel.AddTagCard(id, pid, CardLocation.Deck);
                        else
                            m_duel.AddCard(id, pid, CardLocation.Deck);
                        Replay.Writer.Write(id);
                    }
                }
                else
                {
                    Replay.Writer.Write(dplayer.Deck.Main.Count);
                    for (int j = dplayer.Deck.Main.Count - 1; j >= 0; j--)
                    {
                        int id = dplayer.Deck.Main[j];
                        if (IsTag && (i == 1 || i == 3))
                            m_duel.AddTagCard(id, pid, CardLocation.Deck);
                        else
                            m_duel.AddCard(id, pid, CardLocation.Deck);
                        Replay.Writer.Write(id);
                    }
                }
                Replay.Writer.Write(dplayer.Deck.Extra.Count);
                foreach (int id in dplayer.Deck.Extra)
                {
                    if (IsTag && (i == 1 || i == 3))
                        m_duel.AddTagCard(id, pid, CardLocation.Extra);
                    else
                        m_duel.AddCard(id, pid, CardLocation.Extra);
                    Replay.Writer.Write(id);
                }
            }

            GameServerPacket packet = new GameServerPacket(GameMessage.Start);
            packet.Write((byte)0);
            packet.Write(Config.StartLp);
            packet.Write(Config.StartLp);
            packet.Write((short)m_duel.QueryFieldCount(0, CardLocation.Deck));
            packet.Write((short)m_duel.QueryFieldCount(0, CardLocation.Extra));
            packet.Write((short)m_duel.QueryFieldCount(1, CardLocation.Deck));
            packet.Write((short)m_duel.QueryFieldCount(1, CardLocation.Extra));
            SendToTeam(packet, 0);

            packet.SetPosition(2);
            packet.Write((byte)1);
            SendToTeam(packet, 1);

            packet.SetPosition(2);
            if (m_swapped)
                packet.Write((byte)0x11);
            else
                packet.Write((byte)0x10);
            SendToObservers(packet);

            RefreshExtra(0);
            RefreshExtra(1);

            m_duel.Start(opt);

            TurnCount = 0;
            LifePoints[0] = Config.StartLp;
            LifePoints[1] = Config.StartLp;
            Process();
        }

        public void Surrender(Player player, int reason, bool force = false)
        {
            if (!force)
                if (State != GameState.Duel)
                    return;
            if (player.Type == (int)PlayerType.Observer)
                return;
            GameServerPacket win = new GameServerPacket(GameMessage.Win);
         //   if (Program.Config.STDOUT == true)
         //       Console.WriteLine("Surrender - {0}", player.Name);  //Need API

            int team = player.Type;
            if (IsTag)
                team = player.Type >= 2 ? 1 : 0;
            win.Write((byte)(1 - team));
            win.Write((byte)reason);
            SendToAll(win);

            //MatchSaveResult(1 - team);

            RecordWin(1 - team, reason, force);

            EndDuel(reason == 4);
        }

        public void RecordWin(int team, int reason, bool force = false)
        {
            //Record user win here
            if (Program.Config.STDOUT == true)
                Console.WriteLine("::::endduel|{1}|{0}|{1}", team, reason, Config.Name);
        }

        public void RefreshAll()
        {
            RefreshMonsters(0);
            RefreshMonsters(1);
            RefreshSpells(0);
            RefreshSpells(1);
            RefreshHand(0);
            RefreshHand(1);
        }

        public void RefreshMonsters(int player, int flag = 0x81fff, bool useCache = true)
        {
            byte[] result = m_duel.QueryFieldCard(player, CardLocation.MonsterZone, flag, useCache);
            GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.MonsterZone);
            update.Write(result);
            SendToTeam(update, player);

            update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.MonsterZone);

            MemoryStream ms = new MemoryStream(result);
            BinaryReader reader = new BinaryReader(ms);
            BinaryWriter writer = new BinaryWriter(ms);
            for (int i = 0; i < 5; i++)
            {
                int len = reader.ReadInt32();
                if (len == 4)
                    continue;
                long pos = ms.Position;
                byte[] raw = reader.ReadBytes(len - 4);
                if ((raw[11] & (int)CardPosition.FaceDown) != 0)
                {
                    ms.Position = pos;
                    writer.Write(new byte[len - 4]);
                }
            }
            update.Write(result);

            SendToTeam(update, 1 - player);
            SendToObservers(update);
        }

        public void RefreshSpells(int player, int flag = 0x681fff, bool useCache = true)
        {
            byte[] result = m_duel.QueryFieldCard(player, CardLocation.SpellZone, flag, useCache);
            GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.SpellZone);
            update.Write(result);
            SendToTeam(update, player);

            update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.SpellZone);

            MemoryStream ms = new MemoryStream(result);
            BinaryReader reader = new BinaryReader(ms);
            BinaryWriter writer = new BinaryWriter(ms);
            for (int i = 0; i < 8; i++)
            {
                int len = reader.ReadInt32();
                if (len == 4)
                    continue;
                long pos = ms.Position;
                byte[] raw = reader.ReadBytes(len - 4);
                if ((raw[11] & (int)CardPosition.FaceDown) != 0)
                {
                    ms.Position = pos;
                    writer.Write(new byte[len - 4]);
                }
            }
            update.Write(result);

            SendToTeam(update, 1 - player);
            SendToObservers(update);
        }

        public void RefreshHand(int player, int flag = 0x181fff, bool useCache = true)
        {
            byte[] result = m_duel.QueryFieldCard(player, CardLocation.Hand, flag | 0x100000, useCache);
            GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.Hand);
            update.Write(result);
            CurPlayers[player].Send(update);

            update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.Hand);

            MemoryStream ms = new MemoryStream(result);
            BinaryReader reader = new BinaryReader(ms);
            BinaryWriter writer = new BinaryWriter(ms);
            while (ms.Position < ms.Length)
            {
                int len = reader.ReadInt32();
                if (len == 4)
                    continue;
                long pos = ms.Position;
                byte[] raw = reader.ReadBytes(len - 4);
                if (raw[len - 8] == 0)
                {
                    ms.Position = pos;
                    writer.Write(new byte[len - 4]);
                }
            }
            update.Write(result);

            SendToAllBut(update, player);
        }

        public void RefreshGrave(int player, int flag = 0x81fff, bool useCache = true)
        {
            byte[] result = m_duel.QueryFieldCard(player, CardLocation.Grave, flag, useCache);
            GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.Grave);
            update.Write(result);
            SendToAll(update);
        }

        public void RefreshExtra(int player, int flag = 0x81fff, bool useCache = true)
        {
            byte[] result = m_duel.QueryFieldCard(player, CardLocation.Extra, flag, useCache);
            GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.Extra);
            update.Write(result);
            CurPlayers[player].Send(update);
        }

        public void RefreshSingle(int player, int location, int sequence, int flag = 0x781fff)
        {
            byte[] result = m_duel.QueryCard(player, location, sequence, flag);

            if (location == (int)CardLocation.Removed && (result[15] & (int)CardPosition.FaceDown) != 0)
                return;

            GameServerPacket update = new GameServerPacket(GameMessage.UpdateCard);
            update.Write((byte)player);
            update.Write((byte)location);
            update.Write((byte)sequence);
            update.Write(result);
            CurPlayers[player].Send(update);

            if (IsTag)
            {
                if ((location & (int)CardLocation.Onfield) != 0)
                {
                    SendToTeam(update, player);
                    if ((result[15] & (int)CardPosition.FaceUp) != 0)
                        SendToTeam(update, 1 - player);
                }
                else
                {
                    CurPlayers[player].Send(update);
                    if ((location & 0x90) != 0)
                        SendToAllBut(update, player);
                }
            }
            else
            {
                if ((location & 0x90) != 0 || ((location & 0x2c) != 0 && (result[15] & (int)CardPosition.FaceUp) != 0))
                    SendToAllBut(update, player);
            }
        }

        public int WaitForResponse()
        {
            WaitForResponse(m_lastresponse);
            return m_lastresponse;
        }

        public void WaitForResponse(int player)
        {
            m_lastresponse = player;
            CurPlayers[player].State = PlayerState.Response;
            SendToAllBut(new GameServerPacket(GameMessage.Waiting), player);
            TimeStart();
            GameServerPacket packet = new GameServerPacket(StocMessage.TimeLimit);
            packet.Write((byte)player);
            packet.Write((byte)0); // C++ padding
            packet.Write((short)m_timelimit[player]);
            SendToPlayers(packet);
        }

        public void SetResponse(int resp)
        {
            if (!Replay.Disabled)
            {
                Replay.Writer.Write((byte)4);
                Replay.Writer.Write(BitConverter.GetBytes(resp));
                Replay.Check();
            }

            TimeStop();
            m_duel.SetResponse(resp);
        }

        public void SetResponse(byte[] resp)
        {
            if (!Replay.Disabled)
            {
                Replay.Writer.Write((byte)resp.Length);
                Replay.Writer.Write(resp);
                Replay.Check();
            }

            TimeStop();
            m_duel.SetResponse(resp);
            Process();
        }

        public void EndDuel(bool force)
        {
            if (State == GameState.Duel)
            {
                if (!Replay.Disabled)
                {
                    Replay.End();
                    byte[] replayData = Replay.GetFile();
                    GameServerPacket packet = new GameServerPacket(StocMessage.Replay);
                    packet.Write(replayData);
                    SendToAll(packet);
                }

                State = GameState.End;
                m_duel.End();
            }

            if (m_swapped)
            {
                m_swapped = false;
                Player temp = Players[0];
                Players[0] = Players[1];
                Players[1] = temp;
                Players[0].Type = 0;
                Players[1].Type = 1;
            }

            if (IsMatch && !force && !MatchIsEnd())
            {
                IsReady[0] = false;
                IsReady[1] = false;
                ServerMessage("You have 120 seconds to side!");
                SideTimer = DateTime.UtcNow;
                State = GameState.Side;
                SendToPlayers(new GameServerPacket(StocMessage.ChangeSide));
                SendToObservers(new GameServerPacket(StocMessage.WaitingSide));
            }
            else
                End();
        }

        public void End()
        {
            SendToAll(new GameServerPacket(StocMessage.DuelEnd));
            m_room.CloseDelayed();
        }

        public void TimeReset()
        {
            m_timelimit[0] = Config.GameTimer;
            m_timelimit[1] = Config.GameTimer;
            m_bonustime[0] = 0;
            m_bonustime[1] = 0;
        }

        public void TimeStart()
        {
            m_time = DateTime.UtcNow;
        }

        public void TimeStop()
        {
            if (m_time != null)
            {
                TimeSpan elapsed = DateTime.UtcNow - m_time.Value;
                m_timelimit[m_lastresponse] -= (int)elapsed.TotalSeconds;
                if (m_timelimit[m_lastresponse] < 0)
                    m_timelimit[m_lastresponse] = 0;
                m_time = null;
            }
        }

        private int m_lasttick;
        public void TimeTick()
        {
            if (State == GameState.Duel)
            {
                if (m_time != null)
                {
                    TimeSpan elapsed = DateTime.UtcNow - m_time.Value;
                    if ((int)elapsed.TotalSeconds > m_timelimit[m_lastresponse])
                    {
                        if (m_analyser.LastMessage == GameMessage.SelectIdleCmd ||
                            m_analyser.LastMessage == GameMessage.SelectBattleCmd)
                        {
                            if (Program.Config.AutoEndTurn)
                            {
                                if (Players[m_lastresponse].TurnSkip == 2)
                                {
                                    Surrender(Players[m_lastresponse], 3);
                                }
                                else
                                {
                                    Players[m_lastresponse].State = PlayerState.None;
                                    Players[m_lastresponse].TurnSkip++;
                                    SetResponse(m_analyser.LastMessage == GameMessage.SelectIdleCmd ? 7 : 3);
                                    Process();
                                }
                            }
                            else
                                Surrender(Players[m_lastresponse], 3);
                        }
                        else if (elapsed.TotalSeconds > m_timelimit[m_lastresponse] + 30)
                            Surrender(Players[m_lastresponse], 3);
                    }
                }
            }

            if (State == GameState.Side)
            {
                TimeSpan elapsed = DateTime.UtcNow - SideTimer;
                int currentTick = (int)(120 - elapsed.TotalSeconds);
                if (currentTick == 60 || currentTick == 30 || currentTick == 10 || currentTick < 6)
                {
                    if (m_lasttick != currentTick)
                    {
                        ServerMessage("You have " + currentTick + " seconds left.");
                        m_lasttick = currentTick;
                    }
                }

                if (elapsed.TotalMilliseconds >= 120000)
                {
                    if (!IsReady[0] && !IsReady[1])
                    {
                        State = GameState.End;
                        End();
                        return;
                    }

                    Surrender(!IsReady[0] ? Players[0] : Players[1], 3, true);
                    State = GameState.End;
                    End();
                }
            }

            if (State == GameState.Starting)
            {
                if (IsTpSelect)
                {
                    TimeSpan elapsed = DateTime.UtcNow - TpTimer;

                    int currentTick = 30 - elapsed.Seconds;

                    if (currentTick == 15 || currentTick < 6)
                    {
                        if (m_lasttick != currentTick)
                        {
                            ServerMessage("You have " + currentTick + " seconds left.");
                            m_lasttick = currentTick;
                        }
                    }

                    if (elapsed.TotalMilliseconds >= 30000)
                    {
                        Surrender(Players[m_startplayer], 3, true);
                        State = GameState.End;
                        End();
                    }

                }
            }
            if (State == GameState.Hand)
            {
                TimeSpan elapsed = DateTime.UtcNow - RPSTimer;
                int currentTick = (60 - elapsed.Seconds);

                if (currentTick == 30 || currentTick == 15 || currentTick < 6)
                {
                    if (m_lasttick != currentTick)
                    {
                        ServerMessage("You have " + currentTick + " seconds left.");
                        m_lasttick = currentTick;
                    }
                }

                if ((int)elapsed.TotalMilliseconds >= 60000)
                {
                    if (m_handResult[0] != 0)
                        Surrender(Players[1], 3, true);
                    else if (m_handResult[1] != 0)
                        Surrender(Players[0], 3, true);
                    else
                    {
                        State = GameState.End;
                        End();
                    }

                    if (m_handResult[0] == 0 && m_handResult[1] == 0)
                    {
                        State = GameState.End;
                        End();
                    }
                    else
                        Surrender(Players[1 - m_lastresponse], 3, true);
                }
            }
        }

        public void MatchSaveResult(int player)
        {
            if (!IsMatch)
                return;
            if (player < 2 && m_swapped)
                player = 1 - player;
            if (player < 2)
                m_startplayer = 1 - player;
            else
                m_startplayer = 1 - m_startplayer;
            m_matchResult[m_duelCount++] = player;
        }

        public void MatchKill()
        {
            m_matchKill = true;
        }

        public bool MatchIsEnd()
        {
            if (m_matchKill)
                return true;
            int[] wins = new int[3];
            for (int i = 0; i < m_duelCount; i++)
                wins[m_matchResult[i]]++;
            return wins[0] == 2 || wins[1] == 2 || wins[0] + wins[1] + wins[2] == 3;
        }

        public int MatchWinner()
        {
            int[] wins = new int[3];
            for (int i = 0; i < m_duelCount; i++)
                wins[m_matchResult[i]]++;

            bool draw = wins[0] == wins[1];

            if (draw)
                return 2;

            return wins[0] > wins[1] ? 0 : 1;
        }

        public void MatchSide()
        {
            if (IsReady[0] && IsReady[1])
            {
                State = GameState.Starting;
                IsTpSelect = true;
                TpTimer = DateTime.UtcNow;
                Players[m_startplayer].Send(new GameServerPacket(StocMessage.SelectTp));
            }
        }

        private int GetAvailablePlayerPos()
        {
            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i] == null)
                    return i;
            }
            return -1;
        }

        private void SendHand()
        {
            RPSTimer = DateTime.UtcNow;
            GameServerPacket hand = new GameServerPacket(StocMessage.SelectHand);
            if (IsTag)
            {
                Players[0].Send(hand);
                Players[2].Send(hand);
            }
            else
                SendToPlayers(hand);
        }

        private void Process()
        {
            int result = m_duel.Process();
            switch (result)
            {
                case -1:
                    m_room.Close();
                    break;
                case 2: // Game finished
                    EndDuel(false);
                    break;
            }
        }

        private void SendJoinGame(Player player)
        {
            GameServerPacket join = new GameServerPacket(StocMessage.JoinGame);
            join.Write(Banlist == null ? 0U : Banlist.Hash);
            join.Write((byte)Config.Rule);
            join.Write((byte)Config.Mode);
            join.Write(Config.EnablePriority);
            join.Write(Config.NoCheckDeck);
            join.Write(Config.NoShuffleDeck);
            // C++ padding: 5 bytes + 3 bytes = 8 bytes
            for (int i = 0; i < 3; i++)
                join.Write((byte)0);
            join.Write(Config.StartLp);
            join.Write((byte)Config.StartHand);
            join.Write((byte)Config.DrawCount);
            join.Write((short)Config.GameTimer);
            player.Send(join);

            if (State != GameState.Lobby)
                SendDuelingPlayers(player);
        }

        private void SendDuelingPlayers(Player player)
        {
            for (int i = 0; i < Players.Length; i++)
            {
                GameServerPacket enter = new GameServerPacket(StocMessage.HsPlayerEnter);
                int id = i;
                if (m_swapped)
                {
                    if (IsTag)
                    {
                        if (i == 0 || id == 1)
                            id = i + 2;
                        else
                            id = i - 2;
                    }
                    else
                        id = 1 - i;
                }
                enter.Write(Players[id].Name, 20);
                enter.Write((byte)i);
                player.Send(enter);
            }
        }

        private void InitNewSpectator(Player player)
        {
            int deck1 = m_duel.QueryFieldCount(0, CardLocation.Deck);
            int deck2 = m_duel.QueryFieldCount(1, CardLocation.Deck);

            int hand1 = m_duel.QueryFieldCount(0, CardLocation.Hand);
            int hand2 = m_duel.QueryFieldCount(1, CardLocation.Hand);

            GameServerPacket packet = new GameServerPacket(GameMessage.Start);
            packet.Write((byte)(m_swapped ? 0x11 : 0x10));
            packet.Write(LifePoints[0]);
            packet.Write(LifePoints[1]);
            packet.Write((short)(deck1 + hand1));
            packet.Write((short)m_duel.QueryFieldCount(0, CardLocation.Extra));
            packet.Write((short)(deck2 + hand2));
            packet.Write((short)m_duel.QueryFieldCount(1, CardLocation.Extra));
            player.Send(packet);

            GameServerPacket draw = new GameServerPacket(GameMessage.Draw);
            draw.Write((byte)0);
            draw.Write((byte)hand1);
            for (int i = 0; i < hand1; i++)
                draw.Write(0);
            player.Send(draw);

            draw = new GameServerPacket(GameMessage.Draw);
            draw.Write((byte)1);
            draw.Write((byte)hand2);
            for (int i = 0; i < hand2; i++)
                draw.Write(0);
            player.Send(draw);

            GameServerPacket turn = new GameServerPacket(GameMessage.NewTurn);
            turn.Write((byte)0);
            player.Send(turn);
            if (CurrentPlayer == 1)
            {
                turn = new GameServerPacket(GameMessage.NewTurn);
                turn.Write((byte)0);
                player.Send(turn);
            }

            InitSpectatorLocation(player, CardLocation.MonsterZone);
            InitSpectatorLocation(player, CardLocation.SpellZone);
            InitSpectatorLocation(player, CardLocation.Grave);
            InitSpectatorLocation(player, CardLocation.Removed);
        }

        private void InitSpectatorLocation(Player player, CardLocation loc)
        {
            for (int index = 0; index < 2; index++)
            {
                int flag = loc == CardLocation.MonsterZone ? 0x91fff : 0x81fff;
                byte[] result = m_duel.QueryFieldCard(index, loc, flag, false);

                MemoryStream ms = new MemoryStream(result);
                BinaryReader reader = new BinaryReader(ms);
                BinaryWriter writer = new BinaryWriter(ms);
                while (ms.Position < ms.Length)
                {
                    int len = reader.ReadInt32();
                    if (len == 4)
                        continue;
                    long pos = ms.Position;
                    reader.ReadBytes(len - 4);
                    long endPos = ms.Position;

                    ms.Position = pos;
                    ClientCard card = new ClientCard();
                    card.Update(reader);
                    ms.Position = endPos;

                    bool facedown = ((card.Position & (int)CardPosition.FaceDown) != 0);

                    GameServerPacket move = new GameServerPacket(GameMessage.Move);
                    move.Write(facedown ? 0 : card.Code);
                    move.Write(0);
                    move.Write((byte)card.Controler);
                    move.Write((byte)card.Location);
                    move.Write((byte)card.Sequence);
                    move.Write((byte)card.Position);
                    move.Write(0);
                    player.Send(move);

                    foreach (ClientCard material in card.Overlay)
                    {
                        GameServerPacket xyzcreate = new GameServerPacket(GameMessage.Move);
                        xyzcreate.Write(material.Code);
                        xyzcreate.Write(0);
                        xyzcreate.Write((byte)index);
                        xyzcreate.Write((byte)CardLocation.Grave);
                        xyzcreate.Write((byte)0);
                        xyzcreate.Write((byte)0);
                        xyzcreate.Write(0);
                        player.Send(xyzcreate);

                        GameServerPacket xyzmove = new GameServerPacket(GameMessage.Move);
                        xyzmove.Write(material.Code);
                        xyzmove.Write((byte)index);
                        xyzmove.Write((byte)CardLocation.Grave);
                        xyzmove.Write((byte)0);
                        xyzmove.Write((byte)0);
                        xyzmove.Write((byte)material.Controler);
                        xyzmove.Write((byte)material.Location);
                        xyzmove.Write((byte)material.Sequence);
                        xyzmove.Write((byte)material.Position);
                        xyzmove.Write(0);
                        player.Send(xyzmove);
                    }

                    if (facedown)
                    {
                        ms.Position = pos;
                        writer.Write(new byte[len - 4]);
                    }
                }

                if (loc == CardLocation.MonsterZone)
                {
                    result = m_duel.QueryFieldCard(index, loc, 0x81fff, false);
                    ms = new MemoryStream(result);
                    reader = new BinaryReader(ms);
                    writer = new BinaryWriter(ms);
                    while (ms.Position < ms.Length)
                    {
                        int len = reader.ReadInt32();
                        if (len == 4)
                            continue;
                        long pos = ms.Position;
                        byte[] raw = reader.ReadBytes(len - 4);

                        bool facedown = ((raw[11] & (int)CardPosition.FaceDown) != 0);
                        if (facedown)
                        {
                            ms.Position = pos;
                            writer.Write(new byte[len - 4]);
                        }
                    }
                }

                GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
                update.Write((byte)index);
                update.Write((byte)loc);
                update.Write(result);
                player.Send(update);
            }
        }

        private void HandleError(string error)
        {
            const string log = "LuaErrors.log";
            if (File.Exists(log))
            {
                foreach (string line in File.ReadAllLines(log))
                {
                    if (line == error)
                        return;
                }
            }

            StreamWriter writer = new StreamWriter(log, true);
            writer.WriteLine(error);
            writer.Close();

            GameServerPacket packet = new GameServerPacket(StocMessage.Chat);
            packet.Write((short)PlayerType.Observer);
            packet.Write(error, error.Length + 1);
            SendToAll(packet);
        }

        private static IList<int> ShuffleCards(Random rand, IEnumerable<int> cards)
        {
            List<int> shuffled = new List<int>(cards);
            for (int i = shuffled.Count - 1; i > 0; --i)
            {
                int pos = rand.Next(i + 1);
                int tmp = shuffled[i];
                shuffled[i] = shuffled[pos];
                shuffled[pos] = tmp;
            }
            return shuffled;
        }

        public void BonusTime(GameMessage message)
        {
            switch (message)
            {
                case GameMessage.Summoning:
                case GameMessage.SpSummoning:
                case GameMessage.Set:
                case GameMessage.Chaining:
                case GameMessage.Battle:
                    if (m_bonustime[m_lastresponse] < 300 - Config.GameTimer)
                    {
                        m_bonustime[m_lastresponse] += 10;
                        m_timelimit[m_lastresponse] += 10;
                    }
                    break;
                default:
                    break;
            }

        }
    }
}
