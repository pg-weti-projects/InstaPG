using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;

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
                                              "pseudonim TEXT, " +
                                              "hash_hasla TEXT, " +
                                              "awatar BLOB)"; // Dodaj kolumnę "awatar" jako typ BLOB
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
                                              "zdjecie BLOB, " +
                                              "id_postu INTEGER, " + // Dodaj kolumnę id_postu
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
                        userData.Add("hash_hasla", reader.GetString(6));
                    }
                }
            }
        }

        return userData;
    }

    public bool IsUserExists(int userId)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Uzytkownicy WHERE id_uzytkownika=@UserId";
                command.Parameters.AddWithValue("@UserId", userId);

                int count = Convert.ToInt32(command.ExecuteScalar());

                return count > 0;
            }
        }
    }


    public void RegisterUser(string username, string password, string firstName, string lastName, int age, string description)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                // Hashowanie hasła przed zapisaniem do bazy danych
                string hashedPassword = HashPassword(password);

                // Wstawianie danych użytkownika do tabeli "Uzytkownicy"
                command.CommandText = "INSERT INTO Uzytkownicy (pseudonim, hash_hasla, imie, nazwisko, wiek, opis) VALUES (@Username, @HashedPassword, @FirstName, @LastName, @Age, @Description)";
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@HashedPassword", hashedPassword);
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@Age", age);
                command.Parameters.AddWithValue("@Description", description);
                command.ExecuteNonQuery();
            }
        }
    }

    public bool IsUsernameExists(string username)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Uzytkownicy WHERE pseudonim=@Username";
                command.Parameters.AddWithValue("@Username", username);

                int count = Convert.ToInt32(command.ExecuteScalar());

                return count > 0;
            }
        }
    }

    public int GetUserIdByLogin(string username)
    {
        int res = 0;
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT id_uzytkownika FROM Uzytkownicy WHERE pseudonim = @Username";
                command.Parameters.AddWithValue("@Username", username);
                res = Convert.ToInt32(command.ExecuteScalar());
            }
        }
        return res;
    }
    public bool AuthenticateUser(string username, string password)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                // Pobieranie zahaszowanego hasła użytkownika z bazy danych
                command.CommandText = "SELECT hash_hasla FROM Uzytkownicy WHERE pseudonim = @Username";
                command.Parameters.AddWithValue("@Username", username);
                string hashedPasswordFromDB = command.ExecuteScalar() as string;

                // Hashowanie wprowadzonego hasła i porównywanie z zahaszowanym hasłem z bazy danych
                string hashedPassword = HashPassword(password);
                return hashedPassword == hashedPasswordFromDB;
            }
        }
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                stringBuilder.Append(hashBytes[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }
    }

    public void InsertPost(int userId, string postText, List<BitmapImage> images)
    {
        if (!IsUserExists(userId))
        {
            Console.WriteLine("Użytkownik o podanym identyfikatorze nie istnieje.");
            return;
        }

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Posty (id_uzytkownika, tekst) VALUES (@UserId, @PostText)";
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@PostText", postText);
                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT last_insert_rowid()";
                    int postId = Convert.ToInt32(command.ExecuteScalar());

                    if (images != null && images.Count > 0)
                    {
                        foreach (BitmapImage image in images)
                        {
                            byte[] imageData = null;
                            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));

                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                encoder.Save(memoryStream);
                                imageData = memoryStream.ToArray();
                            }

                            command.CommandText = "INSERT INTO Zdjecia (id_uzytkownika, zdjecie, id_postu) " +
                                                  "VALUES (@UserId, @ImageData, @PostId)";
                            command.Parameters.AddWithValue("@UserId", userId);
                            command.Parameters.AddWithValue("@ImageData", imageData);
                            command.Parameters.AddWithValue("@PostId", postId);
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    Console.WriteLine("Post został pomyślnie dodany.");
                }
            }
        }
    }

    public List<BitmapImage> GetUserImages(int userId)
    {
        List<BitmapImage> images = new List<BitmapImage>();

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT zdjecie FROM Zdjecia WHERE id_uzytkownika=@UserId";
                command.Parameters.AddWithValue("@UserId", userId);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        byte[] imageData = (byte[])reader["zdjecie"];
                        BitmapImage bitmap = new BitmapImage();
                        using (MemoryStream memoryStream = new MemoryStream(imageData))
                        {
                            bitmap.BeginInit();
                            bitmap.StreamSource = memoryStream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                        }
                        images.Add(bitmap);
                    }
                }
            }
        }

        return images;
    }

    public void AddAvatarColumnToUsersTable()
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "ALTER TABLE Uzytkownicy ADD COLUMN awatar BLOB";
                command.ExecuteNonQuery();
            }
        }
    }

    public void SaveUserAvatar(int userId, byte[] avatarData)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE Uzytkownicy SET awatar = @AvatarData WHERE id_uzytkownika = @UserId";
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@AvatarData", avatarData);
                command.ExecuteNonQuery();
            }
        }
    }

    public byte[] GetUserAvatar(int userId)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT awatar FROM Uzytkownicy WHERE id_uzytkownika = @UserId";
                command.Parameters.AddWithValue("@UserId", userId);
                byte[] avatarData = command.ExecuteScalar() as byte[];
                return avatarData;
            }
        }
    }

    public bool IsColumnExists(string tableName, string columnName)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = $"PRAGMA table_info({tableName})";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string existingColumnName = reader["name"].ToString();
                        if (existingColumnName == columnName)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    // Dodaj inne metody do pobierania, aktualizowania, usuwania danych, itp.
}
