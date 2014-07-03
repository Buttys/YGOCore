using System;
using OcgWrapper.Managers;

namespace OcgWrapper
{
    public class Card
    {
        public struct CardData
        {
            public Int32 Code;
            public Int32 Alias;
            public Int64 Setcode;
            public Int32 Type;
            public Int32 Level;
            public Int32 Attribute;
            public Int32 Race;
            public Int32 Attack;
            public Int32 Defense;
            public Int32 LScale;
            public Int32 RScale;
        }

        public int Id { get; private set; }
        public int Ot { get; private set; }
        public CardData Data { get; private set; }

        public static Card Get(int id)
        {
            return CardsManager.GetCard(id);
        }

        public delegate void LoadCardEventHandler(int cardId, ref CardData card);
        public static event LoadCardEventHandler LoadCard;

        internal static void OnLoadCard(int cardId, ref CardData card)
        {
            LoadCardEventHandler handler = LoadCard;
            if (handler != null) handler(cardId, ref card);
        }

        internal Card(CardData data, int ot)
        {
            Id = data.Code;
            Ot = ot;
            Data = data;
        }
    }
}