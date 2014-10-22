using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using YGOCore.Game;
using OcgWrapper;
using YGOCore.Game.Enums;

namespace YGOCore
{
    public class Server
    {
        public bool IsListening { get; private set; }
        private TcpListener m_listener;
        private List<GameClient> m_clients;
        Player announcer;
                

        public Server()
        {
            m_clients = new List<GameClient>();
        }

        public bool Start(int port = 0)
        {
            try
            {
                Api.Init(Program.Config.Path, Program.Config.ScriptFolder, Program.Config.CardCDB);
                BanlistManager.Init(Program.Config.BanlistFile);
                m_listener = new TcpListener(IPAddress.Any, port == 0 ? Program.Config.ServerPort : port);
                m_listener.Start();
                IsListening = true;
            }
            catch (SocketException)
            {
                Logger.WriteError("The " + (port == 0 ? Program.Config.ServerPort : port) + " port is currently in use.");
                return false;
            }
            catch (Exception e)
            {
                Logger.WriteError(e);
                return false;
            }

            Logger.WriteLine("Listening on port " + (port == 0 ? Program.Config.ServerPort : port));
            return true;
        }

        public void Stop()
        {
            if (IsListening)
            {
                m_listener.Stop();
                IsListening = false;

                foreach (GameClient client in m_clients)
                    client.Close();
            }
        }

        public void Process()
        {
            GameManager.HandleRooms();

            while (IsListening && m_listener.Pending())
            {
                m_clients.Add(new GameClient(m_listener.AcceptTcpClient()));
            }
            List<GameClient> toRemove = new List<GameClient>();

            foreach (GameClient client in m_clients)
            {
                client.Tick();
                if (!client.IsConnected || client.InGame())
                    toRemove.Add(client);
            }

            while (toRemove.Count > 0)
            {
                m_clients.Remove(toRemove[0]);
                toRemove.RemoveAt(0);
            }
        }

    }
}
