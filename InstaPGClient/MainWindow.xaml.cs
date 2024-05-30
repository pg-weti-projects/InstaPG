using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using InstaPGClient.ActiveUsersServiceReference;
using System.Linq;

namespace InstaPGClient
{
    public partial class MainWindow : Window
    {
        private InstaPGServiceClient client;
        private ActiveUsersServiceClient activeUsersClient;
        private SQLiteHelper GlobalSQLHelper = new SQLiteHelper();
        private List<User> users = new List<User>(); // Store here the other users Objects ( all from db or from current session )
        public int CurrentUserId { get; set; }

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
                CurrentUserId = GlobalSQLHelper.GetUserIdByLogin(username);
                client.CurrentUserData = GlobalSQLHelper.GetUserData(CurrentUserId);
                CurrentUserName.Text = client.getUserName() + " " + client.getUserSurname();
                CurrentUserDescription.Text = client.getUserDescription() + "\nAge: " + client.getUserAge();
                List<BitmapImage> userImages = GlobalSQLHelper.GetUserImages(CurrentUserId);
                UserGallery.Items.Clear();
                foreach (var image in userImages)
                {
                    Image newImage = new Image
                    {
                        Source = image,
                        Width = 126,
                        Height = 115,
                        Margin = new Thickness(5)
                    };
                    UserGallery.Items.Add(newImage);
                }
                int currentPostCount = userImages.Count;
                CurrentAmountPost.Text = currentPostCount.ToString();

                BitmapImage avatarImage = GlobalSQLHelper.GetUserAvatar(CurrentUserId);
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
                // activeUsersClient.RemoveActiveUser(client.CurrentUserData["pseudonim"].ToString()); TODO: fix this ( does not work if you click on the logout button )
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
            if (CurrentUserId != -1)
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
                        GlobalSQLHelper.SaveUserAvatar(CurrentUserId, avatarData);
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
                PostWindow postWindow = new PostWindow(CurrentUserId, GlobalSQLHelper);
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
                User chosenUser = users.FirstOrDefault(u => u.UserName == UsersList.SelectedItem.ToString());
                if (chosenUser != null)
                {
                    List<BitmapImage> chosenUserImages = GlobalSQLHelper.GetUserImages(chosenUser.UserId);
                    BitmapImage chosenUserAvatarImage = GlobalSQLHelper.GetUserAvatar(chosenUser.UserId);
                    OtherUserProfileWindow userProfileWindow = new OtherUserProfileWindow(chosenUser, chosenUserImages, chosenUserAvatarImage);
                    userProfileWindow.Show();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error occurred during attempt to select user from ListBox: " + 
                                exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                GlobalSQLHelper.InsertPost(CurrentUserId, description, images);

                
                List<BitmapImage> userImages = GlobalSQLHelper.GetUserImages(CurrentUserId);
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
