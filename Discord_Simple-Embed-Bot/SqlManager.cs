using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Simple_Embed_Bot
{
    static class SqlManager
    {
        static readonly string sqlConnectionString;
        static readonly string databasePath;
        static SqlManager()
        {
            databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "MainDatabase.sqlite");
            sqlConnectionString = "Data Source = " + databasePath + "; Version = 3";

            //Create SQLite database file
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }

            string sqlCommand1 = "CREATE TABLE IF NOT EXISTS Events (ServerId INTEGER,ChannelID INTEGER,EndDate TEXT)";
            string sqlCommand2 = "CREATE TABLE IF NOT EXISTS Settings (ServerId INTEGER PRIMARY KEY,Data TEXT)";

            SQLiteConnection m_dbConnection = new SQLiteConnection(sqlConnectionString);
            m_dbConnection.Open();

            SQLiteCommand command = new SQLiteCommand(m_dbConnection);
            command.CommandText = sqlCommand1;
            command.ExecuteNonQuery();
            command.CommandText = sqlCommand2;
            command.ExecuteNonQuery();

            m_dbConnection.Close();
        }

        public static async Task SetData(ulong serverId, string data, char type)
        {
            await Task.Run(() =>
            {
                SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString);
                SQLiteCommand command = new SQLiteCommand("INSERT OR REPLACE INTO Settings (ServerId,Data) values (@serverId, @prefix)", dbConnection);

                command.Parameters.Add(new SQLiteParameter("@serverId", serverId + type));
                command.Parameters.Add(new SQLiteParameter("@prefix", data));

                dbConnection.Open();
                command.Connection = dbConnection;
                command.ExecuteNonQuery();
                dbConnection.Close();
            });
        }

        public static async Task<string> GetData(ulong serverId, char type)
        {
            return await Task<string>.Run(() =>
            {
                string data = null;
                using (SQLiteConnection dbConnection = new SQLiteConnection(sqlConnectionString))
                {
                    
                    SQLiteCommand command = new SQLiteCommand($"SELECT * FROM Settings WHERE ServerId=@type", dbConnection);

                    command.Parameters.Add(new SQLiteParameter("@type", serverId + type));

                    dbConnection.Open();
                    SQLiteDataReader dr = command.ExecuteReader();
                    if (dr.Read())
                    {
                        data = dr.GetString(1);
                    }

                    dbConnection.Close();
                }
                if (data == null)
                {
                    switch (type)
                    {
                        case 'p': return "eb!";
                    }
                }
                return data;
            });
        }
    }
}
