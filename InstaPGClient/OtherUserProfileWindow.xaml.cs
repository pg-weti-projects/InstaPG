using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace InstaPGClient
{
    /// <summary>
    /// A class that displays other user window that is selected from MainWindow and UsersList.
    /// </summary>
    public partial class OtherUserProfileWindow : Window
    {
        private User User { get; set; }
        private List<BitmapImage> UserImages { get; set; }
        private BitmapImage AvatarImage { get; set; }
        
        public OtherUserProfileWindow(User user, List<BitmapImage> userImages, BitmapImage avatarImage)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            User = user;
            UserImages = userImages;
            AvatarImage = avatarImage;
            LoadUserDataAndImages();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadUserDataAndImages()
        {
            // Add images to ItemsControl obj
            foreach (var image in UserImages)
            {
                Image newImage = new Image
                {
                    Source = image,
                    Width = 126,
                    Height = 115,
                    Margin = new Thickness(5)
                };
                ChosenUserGallery.Items.Add(newImage);
            }

            // Add user avatar to Image obj
            if (AvatarImage != null)
            {
                ChosenUserAvatar.Source = AvatarImage;
            }
            else
            {
                ChosenUserAvatar.Source = new BitmapImage(new Uri("img/default_user_avatar.png", UriKind.Relative));
            }
            
            // Add other data to StackPanel
            if (User != null)
            {
                ChosenUserName.Text = $"{User.FirstName} {User.LastName}";
                ChosenUserDescription.Text = User.Description;
                ChosenAmountPost.Text = UserImages.Count.ToString();
            }
            else
            {
                ChosenUserName.Text = "User name";
                ChosenUserDescription.Text = "Description";
                ChosenAmountPost.Text = "0";
                ChosenUserGallery.Items.Clear();
            }
        }
    }
}
