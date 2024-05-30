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
        private List<Dictionary<string, object>> UserImages { get; set; }
        private BitmapImage AvatarImage { get; set; }
        private SQLiteHelper _GlobalSQLHelper = new SQLiteHelper();
        private int CurrentLoggedUser { get; set; }
        
        public OtherUserProfileWindow(User user, List<Dictionary<string, object>> userImages, BitmapImage avatarImage, int currentLoggedUser)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            User = user;
            UserImages = userImages;
            AvatarImage = avatarImage;
            CurrentLoggedUser = currentLoggedUser;
            LoadUserDataAndImages();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadUserDataAndImages()
        {
            // Add images to ItemsControl obj
            foreach (var dict in UserImages)
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
                                _GlobalSQLHelper.GetPostDataBasedOnUserAndPostID(User.UserId, postId);

                            var displayPostWindow = new DisplayPostWindow((BitmapImage)postData["image"],
                                (string)postData["description"], (DateTime)postData["date"], postId, CurrentLoggedUser);
                            displayPostWindow.Show();
                        };

                        ChosenUserGallery.Items.Add(newImage);
                    }
                }
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
