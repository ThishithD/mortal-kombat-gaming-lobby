using GamingLobbyInterface;
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
using System.Windows.Shapes;

namespace GamingLobbyClient
{
    public partial class FilePreviewWindow : Window
    {
        // Constructor to initialize the file preview window
        public FilePreviewWindow(SharedFile file)
        {
            InitializeComponent();
            this.Title = $"Preview: {file.FileName}";

            // Display the file content based on its type
            if (file.IsImage)
            {
                PreviewScrollViewer.Visibility = Visibility.Collapsed;

                var imageSource = new BitmapImage();
                var stream = new MemoryStream(file.FileContent);

                imageSource.BeginInit();
                imageSource.StreamSource = stream;
                imageSource.EndInit();

                PreviewImage.Source = imageSource;
            }
            else // File is a text file
            {
                PreviewImage.Visibility = Visibility.Collapsed;

                string textContent = Encoding.UTF8.GetString(file.FileContent);
                PreviewTextBlock.Text = textContent;
            }
        }
    }
}
