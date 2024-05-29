﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InstaPGClient
{
    public partial class MainWindow : Window
    {
        private InstaPGServiceClient client;
        private SQLiteHelper GlobalSQLHelper = new SQLiteHelper();
        private List<User> users = new List<User>(); // Store here the other users Objects ( all from db or from current session )
        public int CurrentUserId { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            client = new InstaPGServiceClient();
            if(!client.isLogin())
            {
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

            if (GlobalSQLHelper.AuthenticateUser(username, password))
            {
                CurrentUserId = GlobalSQLHelper.GetUserIdByLogin(username);
                MessageBox.Show("Welcome!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                client.CurrentUserData = GlobalSQLHelper.GetUserData(CurrentUserId);
                CurrentUserName.Text = client.getUserName() + " " + client.getUserSurname();
                CurrentUserDescription.Text = client.getUserDescription() + "\nAge: " + client.getUserAge();
                List<Dictionary<string, object>> userImages = GlobalSQLHelper.GetUserImagesObjects(CurrentUserId);
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
                                    GlobalSQLHelper.GetPostDataBasedOnUserAndPostID(CurrentUserId, postId);

                                var displayPostWindow = new DisplayPostWindow((BitmapImage)postData["image"],
                                    (string)postData["description"], (DateTime)postData["date"], postId, CurrentUserId);
                                displayPostWindow.Show();
                            };

                            UserGallery.Items.Add(newImage);
                        }
                    }
                }

                int currentPostCount = userImages.Count;
                CurrentAmountPost.Text = currentPostCount.ToString();

                users = GlobalSQLHelper.GetAllUsersFromDb();
                foreach (User user in users)
                {
                    UsersList.Items.Add(user.FirstName);
                }

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
        
        private void OtherUserSelect_Click(object sender, SelectionChangedEventArgs e)
        {
            if (UsersList.SelectedIndex != -1)
            {
                User chosendUser = users[UsersList.SelectedIndex];
                List<BitmapImage> chosenUserImages = GlobalSQLHelper.GetUserImages(chosendUser.UserId);
                BitmapImage chosenUserAvatarImage = GlobalSQLHelper.GetUserAvatar(chosendUser.UserId);
                OtherUserProfileWindow userProfileWindow = new OtherUserProfileWindow(chosendUser, chosenUserImages, chosenUserAvatarImage);
                userProfileWindow.Show();
            }
        }

        private string GetPassword(PasswordBox passwordBox)
        {
            System.Security.SecureString securePassword = passwordBox.SecurePassword;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(securePassword);
            string plainPassword = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);

            return plainPassword;
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

                int postId = GlobalSQLHelper.InsertPost(CurrentUserId, description, images);
                 
                newImage.MouseLeftButtonUp += (s, args) =>
                {
                    var displayPostWindow = new DisplayPostWindow(bitmap, description, DateTime.Now, postId, CurrentUserId);
                    displayPostWindow.Show();
                };

                
                List<BitmapImage> userImages = GlobalSQLHelper.GetUserImages(CurrentUserId);
                CurrentAmountPost.Text = userImages.Count.ToString();

                MessageBox.Show("Photo added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
