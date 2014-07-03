using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using OcgWrapper.Managers;

namespace OcgWrapper
{
    public static unsafe class Api
    {
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_card_reader(CardReader f);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_message_handler(MessageHandler f);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_script_reader(ScriptReader f);

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_duel(UInt32 seed);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void start_duel(IntPtr pduel, Int32 options);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void end_duel(IntPtr pduel);

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_player_info(IntPtr pduel, Int32 playerid, Int32 lp, Int32 startcount, Int32 drawcount);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void new_card(IntPtr pduel, UInt32 code, Byte owner, Byte playerid, Byte location, Byte sequence, Byte position);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void new_tag_card(IntPtr pduel, UInt32 code, Byte owner, Byte location);

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 process(IntPtr pduel);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 get_message(IntPtr pduel, IntPtr buf);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_log_message(IntPtr pduel, IntPtr buf);

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_responseb(IntPtr pduel, IntPtr buf);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_responsei(IntPtr pduel, UInt32 value);

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 query_card(IntPtr pduel, Byte playerid, Byte location, Byte sequence, Int32 queryFlag, IntPtr buf, Int32 useCache);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 query_field_count(IntPtr pduel, Byte playerid, Byte location);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 query_field_card(IntPtr pduel, Byte playerid, Byte location, Int32 queryFlag, IntPtr buf, Int32 useCache);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 query_field_info(IntPtr pduel, IntPtr buf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ScriptReader(String scriptName, Int32* len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 CardReader(UInt32 code, Card.CardData* pData);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 MessageHandler(IntPtr pDuel, UInt32 messageType);

        private static ScriptReader m_sreader;
        private static CardReader m_creader;
        private static MessageHandler m_msghandler;

        private static IntPtr m_buffer;

        public static void Init(string path = ".", string scripts = "script", string cards = "cards.cdb")
        {
            PathManager.Init(path, scripts, cards);
            CardsManager.Init();
            Duel.Duels = new ConcurrentDictionary<IntPtr, Duel>();

            m_buffer = Marshal.AllocHGlobal(65536);

            m_creader = MyCardReader;
            m_sreader = MyScriptReader;
            m_msghandler = MyMessageHandler;

            set_card_reader(m_creader);
            set_script_reader(m_sreader);
            set_message_handler(m_msghandler);
        }

        public static void Dispose()
        {
            foreach (Duel duel in Duel.Duels.Values)
                duel.Dispose();
            Marshal.FreeHGlobal(m_buffer);
        }

        private static UInt32 MyCardReader(UInt32 code, Card.CardData* pData)
        {
            Card card = CardsManager.GetCard((int) code);
            if (card != null)
                *pData = card.Data;
            return code;
        }

        private static IntPtr MyScriptReader(String scriptName, Int32* len)
        {
            string filename = PathManager.GetScript(scriptName);
            if (!File.Exists(filename))
                return IntPtr.Zero;
            byte[] content = File.ReadAllBytes(filename);
            *len = content.Length;
            Marshal.Copy(content, 0, m_buffer, content.Length);
            return m_buffer;
        }

        private static UInt32 MyMessageHandler(IntPtr pDuel, UInt32 messageType)
        {
            if (Duel.Duels.ContainsKey(pDuel))
            {
                Duel duel = Duel.Duels[pDuel];
                duel.OnMessage(messageType);
            }
            return 0;
        }
    }
}