using System.Windows;
using System.Windows.Forms;

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
        }

        public EditWindow(Icon data)
        {
            InitializeComponent();
            DataContext = data;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.gif)|*.png;*.jpeg;*.jpg;*.gif";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Icon icon = (Icon)DataContext;
                icon.ImagePath = openFileDialog.FileName;
                System.Diagnostics.Debug.WriteLine("image picked" + icon.ImagePath);
            }
        }

    }
}
