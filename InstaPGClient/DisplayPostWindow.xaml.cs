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
using System.Windows.Shapes;

namespace InstaPGClient
{
    public partial class DisplayPostWindow : Window
    {
        public DisplayPostWindow(BitmapImage image, string description, DateTime date)
        {
            InitializeComponent();
            ImagePreview.Source = image;
            DescriptionTextBox.Text = description;
            DatePost.Text = date.ToString("g");
        }
    }
}
