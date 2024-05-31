using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using InstaPGClient.ActiveUsersServiceReference;
using System.Linq;
using System.Windows.Threading;

namespace InstaPGClient
{
    public partial class MainWindow : Window
    {
        private InstaPGServiceClient client;
        private ActiveUsersServiceClient activeUsersClient;
        private SQLiteHelper GlobalSQLHelper = new SQLiteHelper();
        private List<User> users = new List<User>(); // Store here the other users Objects ( all from db or from current session )
        public User CurrentUser { get; set; }
        private DispatcherTimer activeUsersTimer;

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            client = new InstaPGServiceClient();
            activeUsersClient = new ActiveUsersServiceClient();
            if (!client.isLogin())
            {
                MainTab.Visibility = Visibility.Collapsed;
            }
            activeUsersTimer = new DispatcherTimer();
            activeUsersTimer.Interval = TimeSpan.FromSeconds(5); // Ustaw interwał na 5 sekund
            activeUsersTimer.Tick += ActiveUsersTimer_Tick;
            activeUsersTimer.Start();
        }

        private void ActiveUsersTimer_Tick(object sender, EventArgs e)
        {
            UpdateActiveUsersList();
        }
        
        /// <summary>
        /// Handle register button.
        /// </summary>
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!GlobalSQLHelper.IsUsernameExists(NewUserLogin.Text))
            {
                GlobalSQLHelper.RegisterUser(NewUserLogin.Text, this.GetPassword(NewUserPasswordBox), NewUserName.Text, NewUserSurname.Text, Convert.ToInt32(NewUserAge.Text), NewUserDescription.Text);
                MessageBox.Show("New account has been created! Login: " + NewUserLogin.Text, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                NewUserSurname.Text = "";
                NewUserName.Text = "";
                NewUserLogin.Text = "";
                NewUserAge.Text = "";
                NewUserDescription.Text = "";
                NewUserPasswordBox.Password = "";
            }
            else
                MessageBox.Show("User with that login already exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        /// <summary>
        /// Handle login button.
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUser.Text;
            string password = LoginPassword.Password;

            if (GlobalSQLHelper.AuthenticateUser(username, password))
            {
                CurrentUser = GlobalSQLHelper.GetUserDataByHisUserName(username);
                client.CurrentUserData = GlobalSQLHelper.GetUserData(CurrentUser.UserId);
                CurrentUserName.Text = client.getUserName() + " " + client.getUserSurname();
                CurrentUserDescription.Text = client.getUserDescription() + "\nAge: " + client.getUserAge();
                List<Dictionary<string, object>> userImages = GlobalSQLHelper.GetUserImagesObjects(CurrentUser.UserId);
                UserGallery.Items.Clear();

                foreach (var dict in userImages)
                {
                    if (dict.TryGetValue("BitmapImage", out object bitmapValue) && dict.TryGetValue("postID", out object postIdValue))
                    {
                        if (bitmapValue is BitmapImage bitmap && postIdValue is int postId)
                        {
                            Image newImage = new Image
                            {
                                Source = bitmap,
                                Width = 126,
                                Height = 115,
                                Margin = new Thickness(5),
                            };

                            newImage.MouseLeftButtonUp += (s, args) =>
                            {
                                Dictionary<string, object> postData =
                                    GlobalSQLHelper.GetPostDataBasedOnUserAndPostID(CurrentUser.UserId, postId);

                                var displayPostWindow = new DisplayPostWindow((BitmapImage)postData["image"],
                                    (string)postData["description"], (DateTime)postData["date"], postId, CurrentUser.UserId);
                                displayPostWindow.Show();
                            };

                            UserGallery.Items.Add(newImage);
                        }
                    }
                }

                int currentPostCount = userImages.Count;
                CurrentAmountPost.Text = currentPostCount.ToString();

                BitmapImage avatarImage = GlobalSQLHelper.GetUserAvatar(CurrentUser.UserId);
                if (avatarImage != null)
                {
                    UserAvatar.Source = avatarImage;
                }
                else
                {
                    UserAvatar.Source = new BitmapImage(new Uri("img/default_user_avatar.png", UriKind.Relative));
                }

                MainTab.Visibility = Visibility.Visible;
                TabControl.SelectedItem = MainTab;
                RegistrationTab.Visibility = Visibility.Collapsed;
                LoginTab.Visibility = Visibility.Collapsed;

                activeUsersClient.AddActiveUser(LoginUser.Text);
                UpdateActiveUsersList();
            }
            else
            {
                MessageBox.Show("Wrong user name or password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Handle logout operation by setting user logged flag to false, clean current logged user data and users list
        /// and set visibility of the login and register tabs.
        /// </summary>
        private void LogoutButton_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                activeUsersClient.RemoveActiveUser(LoginUser.Text);
                client.SetUserLogged(false);
                client.ClearCurrentUserData();
                LoginTab.Visibility = Visibility.Visible;
                RegistrationTab.Visibility = Visibility.Visible;
                MainTab.Visibility = Visibility.Collapsed;
                TabControl.SelectedItem = LoginTab;
                MessageBox.Show("User has been log out.", "Log out", MessageBoxButton.OK, MessageBoxImage.Information);
                users.Clear();
            }
            catch (Exception exception)
            {
                MessageBox.Show("An error occurred during attempt to logout: " + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Handler add avatar button.
        /// </summary>
        private void AddAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.UserId != -1)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                        UserAvatar.Source = bitmap;

                        byte[] avatarData;
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));

                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            encoder.Save(memoryStream);
                            avatarData = memoryStream.ToArray();
                        }
                        GlobalSQLHelper.SaveUserAvatar(CurrentUser.UserId, avatarData);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        
        /// <summary>
        /// Handle adding photo button.
        /// </summary>
        private void AddPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PostWindow postWindow = new PostWindow(CurrentUser.UserId, GlobalSQLHelper);
                postWindow.PhotoAdded += HandlePhotoAdded;
                postWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handle displaying other user profile after clicking on the user in UsersList ListBox.
        /// </summary>
        private void OtherUserSelect_Click(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (UsersList.SelectedItem != null)
                {
                    User chosenUser = users.FirstOrDefault(u => u.UserName == UsersList.SelectedItem.ToString());
                    if (chosenUser != null)
                    {
                        List<Dictionary<string, object>> chosenUserImages = GlobalSQLHelper.GetUserImagesObjects(chosenUser.UserId);
                        BitmapImage chosenUserAvatarImage = GlobalSQLHelper.GetUserAvatar(chosenUser.UserId);
                        OtherUserProfileWindow userProfileWindow = new OtherUserProfileWindow(chosenUser, chosenUserImages, chosenUserAvatarImage, CurrentUser.UserId);
                        userProfileWindow.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error occurred during attempt to select user from ListBox: " +
                                exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UsersList.UnselectAll();
            }
        }

        /// <summary>
        /// Decrypts the password from the password box
        /// </summary>
        private string GetPassword(PasswordBox passwordBox)
        {
            System.Security.SecureString securePassword = passwordBox.SecurePassword;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(securePassword);
            string plainPassword = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);

            return plainPassword;
        }

        /// <summary>
        /// If the user click add photo button it will be converted to Image and added to DB with assigning to user
        /// that added this photo. Post indicator will be increased.
        /// </summary>
        private void HandlePhotoAdded(byte[] imageData, string description)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(imageData);
                bitmap.EndInit();

                Image newImage = new Image
                {
                    Source = bitmap,
                    Width = 126,
                    Height = 115,
                    Margin = new Thickness(5)
                };

                UserGallery.Items.Add(newImage);
                List<BitmapImage> images = new List<BitmapImage> { bitmap };

                int postId = GlobalSQLHelper.InsertPost(CurrentUser.UserId, description, images);
                 
                newImage.MouseLeftButtonUp += (s, args) =>
                {
                    var displayPostWindow = new DisplayPostWindow(bitmap, description, DateTime.Now, postId, CurrentUser.UserId);
                    displayPostWindow.Show();
                };

                List<BitmapImage> userImages = GlobalSQLHelper.GetUserImages(CurrentUser.UserId);
                CurrentAmountPost.Text = userImages.Count.ToString();

                MessageBox.Show("Photo added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates the list of the active users in the UsersList ListBox and users List.
        /// </summary>
        private void UpdateActiveUsersList()
        {
            try
            {
                List<string> activeUsers = activeUsersClient.GetActiveUsers().ToList();
                UsersList.Items.Clear();
                foreach (var userName in activeUsers)
                {
                    users.Add(GlobalSQLHelper.GetUserDataByHisUserName(userName));
                    UsersList.Items.Add(userName);   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
