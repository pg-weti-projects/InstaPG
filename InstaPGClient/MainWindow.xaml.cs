using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();
            client = new InstaPGServiceClient();
            if(!client.isLogin())
            {
                //Pokaz ekran logowania
                MainTab.Visibility = Visibility.Collapsed;

                // operacje po zalogowaniu
                client.SetUserLogged(true);
                CurrentUserName.Text = client.getUserName() + " " + client.getUserSurname();
                CurrentUserDescription.Text = client.getUserDescription();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!GlobalSQLHelper.IsUsernameExists(NewUserLogin.Text))
            {
                GlobalSQLHelper.RegisterUser(NewUserLogin.Text, this.GetPassword(NewUserPasswordBox), NewUserName.Text, NewUserSurname.Text, Convert.ToInt32(NewUserAge.Text), NewUserDescription.Text);
                MessageBox.Show("Zalozono nowe konto! Login: "+NewUserLogin.Text, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                NewUserLogin.Text = "";
                NewUserName.Text = "";
                NewUserSurname.Text = "";
                NewUserDescription.Text = "";
                NewUserPasswordBox.Password = "";
            }
            else
                MessageBox.Show("Uzytkownik o takim loginie juz istnieje!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if(GlobalSQLHelper.AuthenticateUser(LoginUser.Text, GetPassword(LoginPassword)))
            {
                
                client.SetUserLogged(true);
                client.CurrentUserData = GlobalSQLHelper.GetUserData(GlobalSQLHelper.GetUserIdByLogin(LoginUser.Text));
                CurrentUserName.Text = client.getUserName() + " " + client.getUserSurname();
                CurrentUserDescription.Text = client.getUserDescription() + " lvl:"+client.getUserAge();

                MainTab.Visibility = Visibility.Visible;
                MainTabControl.SelectedItem = MainTab;
                RegistrationTab.Visibility = Visibility.Collapsed;
                LoginTab.Visibility = Visibility.Collapsed;
                MessageBox.Show("Witamy na portalu!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Zla nazwa uzytkownika albo haslo!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                string result = client.GetData(0);
                client.SetUserLogged(false);
                client.ClearCurrentUserData();
                LoginTab.Visibility = Visibility.Visible;
                RegistrationTab.Visibility = Visibility.Visible;
                MainTab.Visibility = Visibility.Collapsed;
                MainTabControl.SelectedItem = LoginTab;
                MessageBox.Show("Użytkownik został wylogowany.", "Wylogowano", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

           // client.Close();
        }


        private void AddAvatarButton_Click(object sender, RoutedEventArgs e)
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Image newImage = new Image();
                    BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                    newImage.Source = bitmap;
                    newImage.Width = 126;
                    newImage.Height = 115;
                    newImage.Margin = new Thickness(5);

                    UserGallery.Items.Add(newImage);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void users_SelectionChanged(object sender , RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
