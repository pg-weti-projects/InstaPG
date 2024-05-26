using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace InstaPGClient
{
    public partial class PostWindow : Window
    {
        private byte[] selectedImageBytes;
        private SQLiteHelper GlobalSQLHelper;
        private int CurrentUserId;

        public PostWindow(int userId, SQLiteHelper sqlHelper)
        {
            InitializeComponent();
            CurrentUserId = userId;
            GlobalSQLHelper = sqlHelper;
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                    selectedImageBytes = GetImageBytes(bitmap);
                    SelectedImagePreview.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private byte[] GetImageBytes(BitmapImage bitmap)
        {
            byte[] imageBytes = null;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            return imageBytes;
        }

        public delegate void PhotoAddedEventHandler(byte[] imageData, string description);

        public event PhotoAddedEventHandler PhotoAdded;
        private void AddPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedImageBytes == null)
                {
                    MessageBox.Show("Please select an image first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string description = DescriptionTextBox.Text;

                PhotoAdded?.Invoke(selectedImageBytes, description);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
