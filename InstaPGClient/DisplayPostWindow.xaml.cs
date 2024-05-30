using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace InstaPGClient
{
    public partial class DisplayPostWindow : Window
    {
        private int _postId;
        private int _userId;
        private SQLiteHelper GlobalSQLHelper = new SQLiteHelper();

        public DisplayPostWindow(BitmapImage image, string description, DateTime date, int postId, int currentUserId)
        {
            InitializeComponent();
            ImagePreview.Source = image;
            DescriptionTextBox.Text = description;
            DatePost.Text = date.ToString("g");
            _postId = postId;
            _userId = currentUserId;

            LoadComments();
        }


        private void LoadComments()
        {
            List<Tuple<string, string>> comments = GlobalSQLHelper.GetCommentsForPost(_postId);
            CommentsListBox.Items.Clear();
            foreach (var comment in comments)
            {
                string displayText = $"{comment.Item1} : {comment.Item2}";
                CommentsListBox.Items.Add(displayText);
            }
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            string newComment = NewCommentTextBox.Text;
            if (!string.IsNullOrEmpty(newComment))
            {
                GlobalSQLHelper.AddCommentToDatabase(_postId, _userId, newComment);
                LoadComments();
                NewCommentTextBox.Clear();
                PlaceholderTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Comment could not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void NewCommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(NewCommentTextBox.Text))
            {
                PlaceholderTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                PlaceholderTextBlock.Visibility = Visibility.Hidden;
            }
        }
    }
}
