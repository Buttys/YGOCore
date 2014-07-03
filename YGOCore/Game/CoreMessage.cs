using System;
using System.IO;
using OcgWrapper.Enums;

namespace YGOCore.Game
{
    public class CoreMessage
    {
        public GameMessage Message { get; private set; }
        public BinaryReader Reader { get; private set; }
        private byte[] m_raw;
        private MemoryStream m_stream;
        private long m_startPosition;
        private long m_endPosition;
        private long m_length;

        public CoreMessage(GameMessage msg, BinaryReader reader, byte[] raw)
        {
            Message = msg;
            Reader = reader;
            m_raw = raw;
            m_stream = (MemoryStream)reader.BaseStream;
            m_startPosition = m_stream.Position;
        }

        public byte[] CreateBuffer()
        {
            SetEndPosition();
            byte[] buffer = new byte[m_length];
            Array.Copy(m_raw, m_startPosition, buffer, 0L, m_length);
            return buffer;
        }

        private void SetEndPosition()
        {
            m_endPosition = m_stream.Position;
            m_length = m_endPosition - m_startPosition;
        }
    }
}