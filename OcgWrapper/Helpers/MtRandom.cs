namespace OcgWrapper.Helpers
{
    internal class MtRandom
    {
        private const int N = 624;
        private const int M = 397;

        private uint m_current;
        private uint m_left = 1;
        private uint[] m_state = new uint[N];

        internal MtRandom()
        {
            Init();
        }

        internal MtRandom(uint seed)
        {
            Init(seed);
        }

        internal void Init(uint seed = 19650218U)
        {
            m_state[0] = seed & 4294967295U;
            for (int j = 1; j < N; ++j)
            {
                m_state[j] = (uint)(1812433253U * (m_state[j - 1] ^ (m_state[j - 1] >> 30)) + j);
                m_state[j] &= 4294967295U;
            }
        }

        internal uint Rand()
        {
            uint y;
            if (0 == --m_left)
                NextState();
            y = m_state[m_current++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= (y >> 18);
            return y;
        }

        internal void Reset(uint rs)
        {
            Init(rs);
            NextState();
        }

        private void NextState()
        {
            int k = 0;
            for (int i = N - M + 1; --i != 0; )
            {
                m_state[k] = (m_state[k + M] ^ Twist(m_state[k], m_state[k + 1]));
                k = k + 1;
            }
            for (int i = M; --i != 0; )
            {
                m_state[k] = (m_state[k + M - N] ^ Twist(m_state[k], m_state[k + 1]));
                k = k + 1;
            }
            m_state[k] = (m_state[k + M - N] ^ Twist(m_state[k], m_state[0]));
            m_left = N;
            m_current = 0;
        }

        private static uint Twist(uint u, uint v)
        {
            return ((MixBits(u, v) >> 1) ^ ((v & 1U) != 0 ? 2567483615U : 0U));
        }

        private static uint MixBits(uint u, uint v)
        {
            return (u & 2147483648U) | (v & 2147483647U);
        }
    }
}