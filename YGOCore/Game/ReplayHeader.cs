namespace YGOCore.Game
{
    public struct ReplayHeader
    {
        public uint Id;
        public uint Version;
        public uint Flag;
        public uint Seed;
        public uint DataSize;
        public uint Hash;
        public byte[] Props;
    }
}