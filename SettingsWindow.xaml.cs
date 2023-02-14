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
        }

        public SettingsWindow(SettingsData settings)
        {
            InitializeComponent();
            DataContext = settings;
        }
    }
}
