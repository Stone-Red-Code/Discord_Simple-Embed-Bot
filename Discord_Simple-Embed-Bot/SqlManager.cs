using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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
            sqlConnectionString = "Data Source = " + databasePath;

            //Create SQLite database file
            if (!File.Exists(databasePath))
            {
                File.Create(databasePath).Close();
            }

            string sqlCommand1 = "CREATE TABLE IF NOT EXISTS Settings (ServerId INTEGER PRIMARY KEY,Data TEXT)";

            SqliteConnection m_dbConnection = new SqliteConnection(sqlConnectionString);
            m_dbConnection.Open();

            SqliteCommand command = new SqliteCommand(sqlCommand1,m_dbConnection);
            command.ExecuteNonQuery();

            m_dbConnection.Close();
        }

        public static async Task SetData(ulong serverId, string data, char type)
        {
            await Task.Run(() =>
            {
                SqliteConnection dbConnection = new SqliteConnection(sqlConnectionString);
                SqliteCommand command = new SqliteCommand("INSERT OR REPLACE INTO Settings (ServerId,Data) values (@serverId, @prefix)", dbConnection);

                command.Parameters.Add(new SqliteParameter("@serverId", serverId + type));
                command.Parameters.Add(new SqliteParameter("@prefix", data));

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
                using (SqliteConnection dbConnection = new SqliteConnection(sqlConnectionString))
                {
                    
                    SqliteCommand command = new SqliteCommand($"SELECT * FROM Settings WHERE ServerId=@type", dbConnection);

                    command.Parameters.Add(new SqliteParameter("@type", serverId + type));

                    dbConnection.Open();
                    SqliteDataReader dr = command.ExecuteReader();
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
