using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;


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
                connection.Close();
                Console.WriteLine("Dane zostały pomyślnie wstawione do tabeli.");
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
        Console.WriteLine($"Próba utworzenia bazy danych w: {Path.GetFullPath("database.db")}");
        if (!DatabaseExists())
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    // Tabela Uzytkownicy
                    command.CommandText = "CREATE TABLE Uzytkownicy (id_uzytkownika INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                          "imie TEXT, " +
                                          "nazwisko TEXT, " +
                                          "wiek INTEGER, " +
                                          "opis TEXT, " +
                                          "pseudonim TEXT)";
                    command.ExecuteNonQuery();

                    // Tabela Posty
                    command.CommandText = "CREATE TABLE Posty (id_postu INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                          "id_uzytkownika INTEGER, " +
                                          "tekst TEXT, " +
                                          "FOREIGN KEY(id_uzytkownika) REFERENCES Uzytkownicy(id_uzytkownika))";
                    command.ExecuteNonQuery();

                    // Tabela Zdjecia
                    command.CommandText = "CREATE TABLE Zdjecia (id_zdjecia INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                          "id_uzytkownika INTEGER, " +
                                          "id_postu INTEGER, " +
                                          "zdjecie BLOB, " +
                                          "FOREIGN KEY(id_uzytkownika) REFERENCES Uzytkownicy(id_uzytkownika), " +
                                          "FOREIGN KEY(id_postu) REFERENCES Posty(id_postu))";
                    command.ExecuteNonQuery();
                }
            }
            connection.Close();
        }
    }

    private bool DatabaseExists()
    {
        if (File.Exists("database.db"))
        {
            Console.WriteLine("Baza danych istnieje!");
            return true;
        }
        return false;
    }

    public Dictionary<string, object> GetUserData(int userId)
    {
        Dictionary<string, object> userData = new Dictionary<string, object>();

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Uzytkownicy WHERE id_uzytkownika=@UserId";
                command.Parameters.AddWithValue("@UserId", userId);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Wczytaj dane użytkownika z wyniku zapytania i dodaj do słownika
                        userData.Add("id_uzytkownika", reader.GetInt32(0));
                        userData.Add("imie", reader.GetString(1));
                        userData.Add("nazwisko", reader.GetString(2));
                        userData.Add("wiek", reader.GetInt32(3));
                        userData.Add("opis", reader.GetString(4));
                        userData.Add("pseudonim", reader.GetString(5));
                    }
                }
            }
        }

        return userData;
    }

    // Dodaj inne metody do pobierania, aktualizowania, usuwania danych, itp.
}
