using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ServiceModel;
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
        private InstaPGServiceClient client;
        private string currentUsername;

        public MainWindow()
        {
            InitializeComponent();
            client = new InstaPGServiceClient();
            UpdateActiveUsersPanel();
        }

        private void LogoutButton_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                client.Logout(currentUsername);
                MessageBox.Show("User has been logged out.", "Logged Out", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateActiveUsersPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                client.Close();
            }
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
                    MessageBox.Show("An unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("An unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void users_SelectionChanged(object sender , RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                currentUsername = UsernameTextBox.Text;
                client.Login(currentUsername);
                MessageBox.Show("User has been logged in.", "Logged In", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateActiveUsersPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateActiveUsersPanel()
        {
            try
            {
                var activeUsers = client.GetActiveUsers();
                ActiveUsersListBox.Items.Clear(); 

                foreach (var user in activeUsers)
                {
                    ActiveUsersListBox.Items.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
