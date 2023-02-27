using System;
using System.Windows;

namespace sDock
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Resources/github.ico"));
        }

        public SettingsWindow(SettingsData settings)
        {
            InitializeComponent();
            Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Resources/github.ico"));
            DataContext = settings;
        }
    }
}
