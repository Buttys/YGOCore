using System;
using YGOCore.Game;
using System.Collections.Concurrent;
using System.Collections.Generic;
using YGOCore;

namespace YGOCore
{
    public static class GameManager
    {

        static Dictionary<string, GameRoom> m_rooms = new Dictionary<string,GameRoom>();

        public static GameRoom CreateOrGetGame(IGameConfig config)
        {
            if (m_rooms.ContainsKey(config.Name))
                return m_rooms[config.Name];
            return CreateRoom(config);
        }

        public static GameRoom GetGame(string name)
        {
            if (m_rooms.ContainsKey(name))
                return m_rooms[name];
            return null;
        }

        public static GameRoom GetRandomGame(int filter = -1)
        {
            List<GameRoom> filteredRooms = new List<GameRoom>();
            GameRoom[] rooms;
            rooms = new GameRoom[m_rooms.Count];
            m_rooms.Values.CopyTo(rooms, 0);

            foreach (GameRoom room in rooms)
            {
                if (room.Game.State == Game.Enums.GameState.Lobby
                    && (filter == -1 ? true : room.Game.Config.Rule == filter))
                    filteredRooms.Add(room);
            }

            if (filteredRooms.Count == 0)
                return null;

            return filteredRooms[Program.Random.Next(0, filteredRooms.Count)];
        }

        public static GameRoom SpectateRandomGame()
        {
            List<GameRoom> filteredRooms = new List<GameRoom>();
            GameRoom[] rooms;
            rooms = new GameRoom[m_rooms.Count];
            m_rooms.Values.CopyTo(rooms, 0);

            foreach (GameRoom room in rooms)
            {
                if (room.Game.State != Game.Enums.GameState.Lobby)
                    filteredRooms.Add(room);
            }

            if (filteredRooms.Count == 0)
                return null;

            return filteredRooms[Program.Random.Next(0, filteredRooms.Count)];
        }

        private static GameRoom CreateRoom(IGameConfig config)
        {
            GameRoom room = new GameRoom(config);
            m_rooms.Add(config.Name, room);
            Logger.WriteLine("Game++");
            return room;
        }

        public static void HandleRooms()
        {
            List<string> toRemove = new List<string>();
            foreach (var room in m_rooms)
            {
                if (room.Value.IsOpen)
                    room.Value.HandleGame();
                else
                    toRemove.Add(room.Key);
            }

            foreach (string room in toRemove)
            {
                m_rooms.Remove(room);
                Logger.WriteLine("Game--");
                if (YGOCore.Program.Config.STDOUT == true)
                    Console.WriteLine("::::network-end");
                if (YGOCore.Program.Config.Recycle == false)
                    System.Environment.Exit(0);
            }
        }

        public static bool GameExists(string name)
        {
            return m_rooms.ContainsKey(name);
        }

        public static string RandomRoomName()
        {
            while (true) //keep searching till one is found!!
            {
                string GuidString = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                GuidString = GuidString.Replace("=", "");
                GuidString = GuidString.Replace("+", "");
                string roomname = GuidString.Substring(0, 5);
                if (!m_rooms.ContainsKey(roomname))
                    return roomname;
            }     
        }

    }
}
