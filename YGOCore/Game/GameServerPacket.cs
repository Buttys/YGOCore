using System.IO;
using OcgWrapper.Enums;
using YGOCore.Game.Enums;

namespace YGOCore.Game
{
    public class GameServerPacket
    {
        private BinaryWriter m_writer;
        private MemoryStream m_stream;

        public GameServerPacket(StocMessage message)
        {
            m_stream = new MemoryStream();
            m_writer = new BinaryWriter(m_stream);
            m_writer.Write((byte)message);
        }

        public GameServerPacket(GameMessage message)
        {
            m_stream = new MemoryStream();
            m_writer = new BinaryWriter(m_stream);
            m_writer.Write((byte)(StocMessage.GameMsg));
            m_writer.Write((byte)message);
        }

        public byte[] GetContent()
        {
            return m_stream.ToArray();
        }

        public void Write(byte[] array)
        {
            m_writer.Write(array);
        }

        public void Write(bool value)
        {
            m_writer.Write((byte)(value ? 1 : 0));
        }

        public void Write(sbyte value)
        {
            m_writer.Write(value);
        }

        public void Write(byte value)
        {
            m_writer.Write(value);
        }

        public void Write(short value)
        {
            m_writer.Write(value);
        }

        public void Write(int value)
        {
            m_writer.Write(value);
        }

        public void Write(uint value)
        {
            m_writer.Write(value);
        }

        public void Write(string text, int len)
        {
            m_writer.WriteUnicode(text, len);
        }

        public long GetPosition()
        {
            return m_stream.Position;
        }

        public void SetPosition(long pos)
        {
            m_stream.Position = pos;
        }
    }
}