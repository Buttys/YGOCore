using System.Collections.Generic;

namespace YGOCore.Game
{
    public class GameRoom
    {
        public Game Game { get; private set; }
        public List<GameClient> m_clients { get; private set; }
        public bool IsOpen { get; private set; }
        private bool m_closePending { get; set; }

        public GameRoom(IGameConfig config)
        {
            m_clients = new List<GameClient>();
            Game = new Game(this, config);
            IsOpen = true;
        }

        public void AddClient(GameClient client) 
        {
            m_clients.Add(client);
        }
        public void RemoveClient(GameClient client) 
        {
            m_clients.Remove(client);
        }
        public void Close() 
        {
            IsOpen = false;
            foreach (GameClient client in m_clients)
                client.Close();
        }
        public void CloseDelayed() 
        {
            foreach (GameClient client in m_clients)
                client.CloseDelayed();
            m_closePending = true;
        }

        public void HandleGame()
        {
            foreach (GameClient user in m_clients)
                user.Tick();

            Game.TimeTick();

            if (m_closePending && m_clients.Count == 0)
                Close();
        }
    }
}
