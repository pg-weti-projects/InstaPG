using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace InstaPGService
{
    public class SQLiteHelper
    {
        private string connectionString = "Data Source=database.db;Version=3;";

        public SQLiteHelper()
        {
            // TODO tworzenie tabeli i bd jesli nie ma jej lokalnie
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

        // Dodaj inne metody do pobierania, aktualizowania, usuwania danych, itp.
    }
}
