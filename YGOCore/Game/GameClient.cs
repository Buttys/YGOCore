using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using YGOCore;

namespace YGOCore.Game
{
    public class GameClient
    {
        public bool IsConnected { get; private set; }
        public Game Game { get; private set; }
        public Player Player { get; private set; }
        private GameRoom m_room;
        private TcpClient m_client;
        private BinaryReader m_reader;
        private Queue<GameClientPacket> m_recvQueue;
        private Queue<byte[]> m_sendQueue;
        private bool m_disconnected;
        private bool m_closePending;
        private int m_receivedLen;

        public GameClient(TcpClient client)
        {
            IsConnected = true;
            Player = new Player(this);
            m_client = client;
            m_reader = new BinaryReader(m_client.GetStream());
            m_recvQueue = new Queue<GameClientPacket>();
            m_sendQueue = new Queue<byte[]>();
            m_receivedLen = -1;
        }

        public void Close()
        {
            if (!IsConnected)
                return;
            IsConnected = false;
            m_client.Close();
            if(InGame())
                m_room.RemoveClient(this);

        }

        public bool InGame()
        {
            return Game != null;
        }

        public void JoinGame(GameRoom room)
        {
            if (m_room == null)
            {
                m_room = room;
                Game = m_room.Game;
            }
        }

        public void CloseDelayed()
        {
            m_closePending = true;
        }

        public void Send(byte[] raw)
        {
            m_sendQueue.Enqueue(raw);
        }

        public void Tick()
        {
            if (IsConnected)
            {
                try
                {
                    CheckDisconnected();
                    NetworkSend();
                    NetworkReceive();
                }
                catch (Exception)
                {
                    m_disconnected = true;
                }
            }
            if (m_closePending)
            {
                m_disconnected = true;
                Close();
                return;
            }
            if (!m_disconnected)
            {
                try
                {
                    NetworkParse();
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex);
                    m_disconnected = true;
                }
            }
            if (m_disconnected)
            {
                Close();
                Player.OnDisconnected();
                if (YGOCore.Program.Config.STDOUT == true)
                    Console.WriteLine("::::network-end");
                if (YGOCore.Program.Config.Recycle == false)
                    System.Environment.Exit(0);
            }
        }

        private void CheckDisconnected()
        {
            m_disconnected = (m_client.Client.Poll(1, SelectMode.SelectRead) && m_client.Available == 0);
        }

        private void NetworkReceive()
        {
            if (m_client.Available >= 2 && m_receivedLen == -1)
                m_receivedLen = m_reader.ReadUInt16();

            if (m_receivedLen != -1 && m_client.Available >= m_receivedLen)
            {
                GameClientPacket packet = new GameClientPacket(m_reader.ReadBytes(m_receivedLen));
                m_receivedLen = -1;
                lock (m_recvQueue)
                    m_recvQueue.Enqueue(packet);
            }
        }

        private void NetworkSend()
        {
            while (m_sendQueue.Count > 0)
            {
                byte[] raw = m_sendQueue.Dequeue();
                MemoryStream stream = new MemoryStream(raw.Length + 2);
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write((ushort)raw.Length);
                writer.Write(raw);
                m_client.Client.Send(stream.ToArray());
            }
        }

        private void NetworkParse()
        {
            int count;
            lock (m_recvQueue)
                count = m_recvQueue.Count;
            while (count > 0)
            {
                GameClientPacket packet = null;
                lock (m_recvQueue)
                {
                    if (m_recvQueue.Count > 0)
                        packet = m_recvQueue.Dequeue();
                    count = m_recvQueue.Count;
                }
                if (packet != null)
                    Player.Parse(packet);
            }
        }
    }
}