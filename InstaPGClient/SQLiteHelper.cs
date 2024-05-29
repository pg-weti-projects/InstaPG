using InstaPGClient;
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
                StringBuilder sqlCommandBuilder = new StringBuilder($"INSERT INTO {tableName} (");
                StringBuilder valuesBuilder = new StringBuilder("VALUES (");

                // Adding column names to SQL query
                foreach (var column in columnData)
                {
                    sqlCommandBuilder.Append($"{column.Key}, ");
                    valuesBuilder.Append($"@{column.Key}, ");
                    command.Parameters.AddWithValue($"@{column.Key}", column.Value);
                }

                // Removing last comma and adding closing brackets
                sqlCommandBuilder.Remove(sqlCommandBuilder.Length - 2, 2).Append(") ");
                valuesBuilder.Remove(valuesBuilder.Length - 2, 2).Append(")");

                command.CommandText = sqlCommandBuilder.ToString() + valuesBuilder.ToString();

                // Make SQL query
                command.ExecuteNonQuery();
                connection.Close();
                Console.WriteLine("Data has been successfuly added to table.");
            }
        }
    }

    /// <summary>
    /// Creates database.
    /// </summary>
    private void CreateDatabase()
    {
        Console.WriteLine($"Attempt to create database in: {Path.GetFullPath("database.db")}");
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

    /// <summary>
    /// Checks if database file exists.
    /// </summary>
    private bool DatabaseExists()
    {
        if (File.Exists("database.db"))
        {
            Console.WriteLine("Database already exists!");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets user data from DB in dict format.
    /// </summary>
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

    /// <summary>
    /// Gets all users from DB and returns list of users objects.
    /// </summary>
    public List<User> GetAllUsersFromDb()
    {
        List<User> users = new List<User>();

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT user_id, name, surname, age, description, login, avatar FROM Users";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int userId = reader.GetInt32(0);
                        string firstName = reader.GetString(1);
                        string lastName = reader.GetString(2);
                        int age = reader.GetInt32(3);
                        string description = reader.GetString(4);
                        string username = reader.GetString(5);

                        User user = new User(userId, firstName, lastName, age, description, username);
                        users.Add(user);
                    }
                }
            }
        }
        return users;
    }

    /// <summary>
    /// Adds new user data to DB.
    /// </summary>
    public void RegisterUser(string username, string password, string firstName, string lastName, int age, string description)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                string hashedPassword = HashPassword(password);

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

    /// <summary>
    /// Finds a user with a given name in the DB and checks whether his decrypted password is the same as the one
    /// given in the arg.
    /// </summary>
    public bool AuthenticateUser(string username, string password)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                // Download hashed user password from DB
                command.CommandText = "SELECT pass_hash FROM Users WHERE login = @Username";
                command.Parameters.AddWithValue("@Username", username);
                string hashedPasswordFromDB = command.ExecuteScalar() as string;

                string hashedPassword = HashPassword(password);
                return hashedPassword == hashedPasswordFromDB;
            }
        }
    }

    /// <summary>
    /// Hashes the given string.
    /// </summary>
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

    public int InsertPost(int userId, string postText, List<BitmapImage> images)
    {
        if (!IsUserExists(userId))
        {
            Console.WriteLine("Użytkownik o podanym identyfikatorze nie istnieje.");
            return -1;
        }

        int postId;
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    DateTime now = DateTime.Now;

                    string formattedDate = now.ToString("yyyy-MM-dd HH:mm:ss");
                    command.CommandText = "INSERT INTO Posts (user_id, date, post_description) VALUES (@UserId, @Date, @PostText)";
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Date", formattedDate);
                    command.Parameters.AddWithValue("@PostText", postText);
                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT last_insert_rowid()";
                    postId = Convert.ToInt32(command.ExecuteScalar());

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

        return postId;
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

    /// <summary>
    /// Checks if the user of given user ID exists in database.
    /// </summary>
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

    /// <summary>
    /// Checks if user with given username exists in DB.
    /// </summary>
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

    /// <summary>
    /// Gets user ID by given username.
    /// </summary>
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

    /// <summary>
    /// Gets all saved user images ( based on user ID ) from DB and returns BitmapImage objects list.
    /// </summary>
    public List<BitmapImage> GetUserImages(int userId)
    {
        List<BitmapImage> images = new List<BitmapImage>();

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT zdjecie, post_id FROM Photos WHERE user_id=@UserId";
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
    
    /// <summary>
    /// Gets all saved user images ( based on user ID ) from DB and returns BitmapImage objects list.
    /// </summary>
    public List<Dictionary<string, object>> GetUserImagesObjects(int userId)
    {
        List<Dictionary<string, object>> images = new List<Dictionary<string, object>>();

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT zdjecie, post_id FROM Photos WHERE user_id=@UserId";
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
                        int postid = Convert.ToInt32(reader["post_id"]);

                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("BitmapImage", bitmap);
                        dict.Add("postID", postid);

                        images.Add(dict);
                    }
                }
            }
        }
        return images;
    }
    
    /// <summary>
    /// Get post data from DB (description, date and image ) based on userID and postID.
    /// </summary>
    public  Dictionary<string, object> GetPostDataBasedOnUserAndPostID(int userID, int postID)
    {
        Dictionary<string, object> postData = new Dictionary<string, object>();
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT post_description, date, zdjecie FROM Posts INNER JOIN Photos ON Posts.post_id = Photos.post_id WHERE Photos.user_id = @UserId AND Photos.photo_id = @PhotoId";
                command.Parameters.AddWithValue("@UserId", userID);
                command.Parameters.AddWithValue("@PhotoId", postID);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        postData.Add("description", reader.GetString(0));
                        postData.Add("date", reader.GetDateTime(1));
                        postData.Add("image", LoadImageFromBytes((byte[])reader["zdjecie"]));
                        return postData;

                    }
                }
            }
        }
        return null;
    }
    
    /// <summary>
    /// Converts byte image to BitmapImage object.
    /// </summary>
    private BitmapImage LoadImageFromBytes(byte[] imageData)
    {
        BitmapImage bitmap = new BitmapImage();
        using (MemoryStream memoryStream = new MemoryStream(imageData))
        {
            bitmap.BeginInit();
            bitmap.StreamSource = memoryStream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
        }
        return bitmap;
    }

    /// <summary>
    /// Gets user avatar ( based on user ID ) from DB and returns it as BitmapImage obj.
    /// </summary>
    public BitmapImage GetUserAvatar(int userId)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT avatar FROM Users WHERE user_id = @UserId";
                command.Parameters.AddWithValue("@UserId", userId);
                byte[] avatarData = command.ExecuteScalar() as byte[];
            
                if (avatarData != null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    using (MemoryStream memoryStream = new MemoryStream(avatarData))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = memoryStream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                    }
                    return bitmap;
                }
            }
        }
        return null;
    }

    public void AddCommentToDatabase(int postId, int userId, string comment)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = "INSERT INTO Comments (post_id, user_id, comment_text) VALUES (@PostId, @UserId, @Comment)";
                command.Parameters.AddWithValue("@PostId", postId);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Comment", comment);
                command.ExecuteNonQuery();
            }
        }
    }



    public List<Tuple<string, string>> GetCommentsForPost(int postId)
    {
        List<Tuple<string, string>> comments = new List<Tuple<string, string>>();

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = "SELECT Users.login, Comments.comment_text FROM Comments " +
                                      "INNER JOIN Users ON Comments.user_id = Users.user_id " +
                                      "WHERE Comments.post_id = @PostId";
                command.Parameters.AddWithValue("@PostId", postId);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string username = reader.GetString(0);
                        string comment = reader.GetString(1);
                        comments.Add(new Tuple<string, string>(username, comment));
                    }
                }
            }
        }

        return comments;
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
}
