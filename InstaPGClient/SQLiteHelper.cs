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
                        // Tabela Users
                        command.CommandText = "CREATE TABLE Users (user_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                              "name TEXT, " +
                                              "surname TEXT, " +
                                              "age INTEGER, " +
                                              "description TEXT, " +
                                              "login TEXT, " +
                                              "pass_hash TEXT, " +
                                              "avatar BLOB)"; // Dodaj kolumnę "avatar" jako typ BLOB
                        command.ExecuteNonQuery();

                        // Tabela Posts
                        command.CommandText = "CREATE TABLE Posts (post_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                              "user_id INTEGER, " +
                                              "post_description TEXT, " +
                                              "date DATATIME, " +
                                              "FOREIGN KEY(user_id) REFERENCES Users(user_id))";
                        command.ExecuteNonQuery();

                        // Tabela Photos
                        command.CommandText = "CREATE TABLE Photos (photo_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                              "user_id INTEGER, " +
                                              "zdjecie BLOB, " +
                                              "post_id INTEGER, " + // Dodaj kolumnę post_id
                                              "FOREIGN KEY(user_id) REFERENCES Users(user_id), " +
                                              "FOREIGN KEY(post_id) REFERENCES Posts(post_id))";
                        command.ExecuteNonQuery();

                        // Tabela Comments
                        command.CommandText = "CREATE TABLE Comments (comment_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                              "user_id INTEGER, " +
                                              "comment_text TEXT, " +
                                              "post_id INTEGER, " +
                                              "FOREIGN KEY(user_id) REFERENCES Users(user_id), " +
                                              "FOREIGN KEY(post_id) REFERENCES Posts(post_id))";
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
                command.CommandText = "SELECT * FROM Users WHERE user_id=@UserId";
                command.Parameters.AddWithValue("@UserId", userId);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Wczytaj dane użytkownika z wyniku zapytania i dodaj do słownika
                        userData.Add("user_id", reader.GetInt32(0));
                        userData.Add("name", reader.GetString(1));
                        userData.Add("surname", reader.GetString(2));
                        userData.Add("age", reader.GetInt32(3));
                        userData.Add("description", reader.GetString(4));
                        userData.Add("login", reader.GetString(5));
                        userData.Add("pass_hash", reader.GetString(6));
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
                command.CommandText = "SELECT COUNT(*) FROM Users WHERE user_id=@UserId";
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

                // Wstawianie danych użytkownika do tabeli "Users"
                command.CommandText = "INSERT INTO Users (login, pass_hash, name, surname, age, description) VALUES (@Username, @HashedPassword, @FirstName, @LastName, @Age, @Description)";
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
                command.CommandText = "SELECT COUNT(*) FROM Users WHERE login=@Username";
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
                command.CommandText = "SELECT user_id FROM Users WHERE login = @Username";
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
                command.CommandText = "SELECT pass_hash FROM Users WHERE login = @Username";
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
                    DateTime now = DateTime.Now;

                    // Formatowanie daty do formatu DATETIME dla SQLite
                    string formattedDate = now.ToString("yyyy-MM-dd HH:mm:ss");
                    command.CommandText = "INSERT INTO Posts (user_id, date, post_description) VALUES (@UserId, @Date, @PostText)";
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Date", formattedDate);
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

                            command.CommandText = "INSERT INTO Photos (user_id, zdjecie, post_id) " +
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
                command.CommandText = "SELECT zdjecie FROM Photos WHERE user_id=@UserId";
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
                command.CommandText = "ALTER TABLE Users ADD COLUMN avatar BLOB";
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
                command.CommandText = "UPDATE Users SET avatar = @AvatarData WHERE user_id = @UserId";
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
                command.CommandText = "SELECT avatar FROM Users WHERE user_id = @UserId";
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
