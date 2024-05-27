using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InstaPGClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random random = new Random();
        private InstaPGServiceClient client;
        private SQLiteHelper GlobalSQLHelper = new SQLiteHelper();
        private List<User> users = new List<User>(); // przechowujmy tutaj uzytkownikow ktorych wykryjemy po zalogowaniu sie
        public int CurrentUserId { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            client = new InstaPGServiceClient();
            if(!client.isLogin())
            {
                //Ukryj ekran główny aplikacji
                MainTab.Visibility = Visibility.Collapsed;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!GlobalSQLHelper.IsUsernameExists(NewUserLogin.Text))
            {
                GlobalSQLHelper.RegisterUser(NewUserLogin.Text, this.GetPassword(NewUserPasswordBox), NewUserName.Text, NewUserSurname.Text, Convert.ToInt32(NewUserAge.Text), NewUserDescription.Text);
                MessageBox.Show("New account has been created! Login: " + NewUserLogin.Text, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                NewUserLogin.Text = "";
                NewUserName.Text = "";
                NewUserSurname.Text = "";
                NewUserDescription.Text = "";
                NewUserPasswordBox.Password = "";
            }
            else
                MessageBox.Show("User with that login already exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUser.Text;
            string password = LoginPassword.Password;

            SQLiteHelper db = new SQLiteHelper();
            if (db.AuthenticateUser(username, password))
            {
                CurrentUserId = db.GetUserIdByLogin(username);
                MessageBox.Show("Welcome!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                client.CurrentUserData = db.GetUserData(CurrentUserId);
                CurrentUserName.Text = client.getUserName() + " " + client.getUserSurname();
                CurrentUserDescription.Text = client.getUserDescription() + "\nAge: " + client.getUserAge();
                List<BitmapImage> userImages = db.GetUserImages(CurrentUserId);
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

                MainTab.Visibility = Visibility.Visible;
                TabControl.SelectedItem = MainTab;
                RegistrationTab.Visibility = Visibility.Collapsed;
                LoginTab.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Wrong user name or password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetPassword(PasswordBox passwordBox)
        {
            // Pobierz wprowadzone hasło jako SecureString
            System.Security.SecureString securePassword = passwordBox.SecurePassword;

            // Konwertuj SecureString na String
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(securePassword);
            string plainPassword = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);

            return plainPassword;
        }

        private void LogoutButton_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                client.SetUserLogged(false);
                client.ClearCurrentUserData();
                LoginTab.Visibility = Visibility.Visible;
                RegistrationTab.Visibility = Visibility.Visible;
                MainTab.Visibility = Visibility.Collapsed;
                TabControl.SelectedItem = LoginTab;
                MessageBox.Show("User has been log out.", "Log out", MessageBoxButton.OK, MessageBoxImage.Information);
                users.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //client.Close();
        }


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

        private void SaveAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserAvatar.Source != null && CurrentUserId != -1)
            {
                try
                {
                    byte[] avatarData;
                    BitmapImage bitmapImage = (BitmapImage)UserAvatar.Source;
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        encoder.Save(memoryStream);
                        avatarData = memoryStream.ToArray();
                    }
                    GlobalSQLHelper.SaveUserAvatar(CurrentUserId, avatarData);

                    MessageBox.Show("Avatar has been saved.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No avatar selected or no user logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetCurrentUserId()
        {
            return ((MainWindow)Application.Current.MainWindow).CurrentUserId;
        }

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
                int currentPostCount = userImages.Count;
                CurrentAmountPost.Text = currentPostCount.ToString();

                MessageBox.Show("Photo added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateOrUpdateAvatarColumn()
        {
            if (!GlobalSQLHelper.IsColumnExists("Users", "avatar"))
            {
                GlobalSQLHelper.AddAvatarColumnToUsersTable();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedIndex == 2)
            {
                if (CurrentUserId == -1)
                {
                    ShowDefaultView();
                }
                else
                {
                    try
                    {
                        CreateOrUpdateAvatarColumn();

                        var userData = GlobalSQLHelper.GetUserData(CurrentUserId);
                        CurrentUserName.Text = userData["name"].ToString() + " " + userData["surname"].ToString();
                        CurrentUserDescription.Text = CurrentUserDescription.Text = client.getUserDescription() + "\nAge: " + client.getUserAge();


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

                        var avatarImage = GlobalSQLHelper.GetUserAvatar(CurrentUserId);
                        if (avatarImage != null)
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = new MemoryStream(avatarImage);
                            bitmap.EndInit();
                            UserAvatar.Source = bitmap;
                        }
                        else
                        {
                            UserAvatar.Source = new BitmapImage(new Uri("user_avatar.png", UriKind.Relative));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ShowDefaultView();
                    }
                }
            }
        }


        private void ShowDefaultView()
        {
            CurrentUserName.Text = "User name";
            CurrentUserDescription.Text = "Description";
            CurrentAmountPost.Text = "0";
            UserGallery.Items.Clear();
            UserAvatar.Source = new BitmapImage(new Uri("user_avatar.png", UriKind.Relative));
        }


        private void users_SelectionChanged(object sender , RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
