using System.Collections.Generic;
#if __MonoCS__
using Mono.Data.Sqlite;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
#else
using System.Data.SQLite;
#endif

namespace OcgWrapper.Managers
{
    internal static class CardsManager
    {
        private static IDictionary<int, Card> m_cards;

        internal static void Init()
        {
            m_cards = new Dictionary<int, Card>();

            string absolutePath = PathManager.GetCardsDb();
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + absolutePath);
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT id, ot, alias, setcode, type, level, race, attribute, atk, def FROM datas", connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                int ot = reader.GetInt32(1);
                int levelinfo = reader.GetInt32(5);
                int level = levelinfo & 0xff;
                int lscale = (levelinfo >> 24) & 0xff;
                int rscale = (levelinfo >> 16) & 0xff;
                Card.CardData data = new Card.CardData
                {
                    Code = id,
                    Alias = reader.GetInt32(2),
                    Setcode = reader.GetInt64(3),
                    Type = reader.GetInt32(4),
                    Level = level,
                    LScale = lscale,
                    RScale = rscale,
                    Race = reader.GetInt32(6),
                    Attribute = reader.GetInt32(7),
                    Attack = reader.GetInt32(8),
                    Defense = reader.GetInt32(9)
                };
                m_cards.Add(id, new Card(data, ot));
            }
            reader.Close();

            connection.Close();
        }

        internal static Card GetCard(int id)
        {
            if (m_cards.ContainsKey(id))
                return m_cards[id];
            return null;
        }
    }
}