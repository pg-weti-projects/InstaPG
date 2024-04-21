using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace InstaPGService
{
    public class SQLiteHelper
    {
        private string connectionString = "Data Source=database.db;Version=3;";

        public SQLiteHelper()
        {
            CreateDatabase();
        }

        public void InsertData(string tableName, Dictionary<string, object> columnData)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    // Budowanie zapytania SQL na podstawie nazwy tabeli i przekazanych kolumn oraz ich danych
                    StringBuilder sqlCommandBuilder = new StringBuilder($"INSERT INTO {tableName} (");
                    StringBuilder valuesBuilder = new StringBuilder("VALUES (");

                    // Dodawanie nazw kolumn do zapytania SQL
                    foreach (var column in columnData)
                    {
                        sqlCommandBuilder.Append($"{column.Key}, ");
                        valuesBuilder.Append($"@{column.Key}, ");
                        command.Parameters.AddWithValue($"@{column.Key}", column.Value);
                    }

                    // Usuwanie ostatniego przecinka i dodawanie nawiasów zamykających
                    sqlCommandBuilder.Remove(sqlCommandBuilder.Length - 2, 2).Append(") ");
                    valuesBuilder.Remove(valuesBuilder.Length - 2, 2).Append(")");

                    command.CommandText = sqlCommandBuilder.ToString() + valuesBuilder.ToString();

                    // Wykonanie zapytania SQL
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool IsTableExists(string tableName)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@TableName";
                    command.Parameters.AddWithValue("@TableName", tableName);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count > 0;
                }
            }
        }

        private void CreateDatabase()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                if (!DatabaseExists())
                    using (SQLiteCommand command = connection.CreateCommand())
                {
                    // Tabela Uzytkownicy
                    command.CommandText = "CREATE TABLE IF NOT EXISTS Uzytkownicy (id_uzytkownika INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                          "imie TEXT, " +
                                          "nazwisko TEXT, " +
                                          "wiek INTEGER, " +
                                          "opis TEXT, " +
                                          "pseudonim TEXT)";
                    command.ExecuteNonQuery();

                    // Tabela Posty
                    command.CommandText = "CREATE TABLE IF NOT EXISTS Posty (id_postu INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                          "id_uzytkownika INTEGER, " +
                                          "tekst TEXT, " +
                                          "FOREIGN KEY(id_uzytkownika) REFERENCES Uzytkownicy(id_uzytkownika))";
                    command.ExecuteNonQuery();

                    // Tabela Zdjecia
                    command.CommandText = "CREATE TABLE IF NOT EXISTS Zdjecia (id_zdjecia INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                          "id_uzytkownika INTEGER, " +
                                          "id_postu INTEGER, " +
                                          "zdjecie BLOB, " +
                                          "FOREIGN KEY(id_uzytkownika) REFERENCES Uzytkownicy(id_uzytkownika), " +
                                          "FOREIGN KEY(id_postu) REFERENCES Posty(id_postu))";
                    command.ExecuteNonQuery();
                }
            }
        }

        private bool DatabaseExists()
        {
            if (File.Exists("database.db"))
            {
                return true;
            }
            return false;
        }

        // Dodaj inne metody do pobierania, aktualizowania, usuwania danych, itp.
    }
}
