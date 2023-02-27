using System.Windows;
using System;
using System.Windows.Forms;
using System.IO;

namespace sDock
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public EditWindow()
        {
            InitializeComponent();
            Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Resources/github.ico"));
        }

        public EditWindow(Icon data)
        {
            InitializeComponent();
            Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Resources/github.ico"));
            Title = "sDock - Edit " + data.Data.Name + " Icon";
            DataContext = data;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.gif;*.ico;)|*.png;*.jpeg;*.jpg;*.gif;*.ico";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var imageChoicePath = openFileDialog.FileName;
                Icon icon = (Icon)DataContext;

                // move the image to documents/icons
                var savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\sDock";
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                savePath = savePath + "\\icons";
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                var finalImagePath = savePath + "\\" + Path.GetFileName(imageChoicePath);
                if (!File.Exists(finalImagePath))
                {
                    File.Copy(imageChoicePath, finalImagePath);
                }
                icon.ImagePath = finalImagePath;
                System.Diagnostics.Debug.WriteLine("image picked" + finalImagePath);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ((Icon)DataContext).ResetIconToDefault();
        }
    }
}
