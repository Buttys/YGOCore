using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using YGOCore.Game;
using OcgWrapper;

namespace YGOCore
{
    public class Server
    {
        public bool IsListening { get; private set; }
        private TcpListener m_listener;
        public ServerConfig Config { get; private set; }
        private List<GameClient> m_clients;

        public Server()
        {
            Config = new ServerConfig();
            m_clients = new List<GameClient>();

            if (Config.Load())
                Logger.WriteLine("Config loaded.");
            else
                Logger.WriteLine("Unable to load config.txt, using default settings.");
        }

        public bool Start(int port = 0)
        {
            try
            {
                Api.Init(Config.Path, Config.ScriptFolder, Config.CardCDB);
                BanlistManager.Init(Config.BanlistFile);
                m_listener = new TcpListener(IPAddress.Any, port == 0 ? Config.ServerPort : port);
                m_listener.Start();
                IsListening = true;
            }
            catch (SocketException)
            {
                Logger.WriteError("The " + (port == 0 ? Config.ServerPort : port) + " port is currently in use.");
                return false;
            }
            catch (Exception e)
            {
                Logger.WriteError(e);
                return false;
            }

            Logger.WriteLine("Listening on port " + (port == 0 ? Config.ServerPort : port));
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
                m_clients.Add(new GameClient(m_listener.AcceptTcpClient()));

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
