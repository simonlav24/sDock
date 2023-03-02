using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.ComponentModel;
using System.Xml.Serialization;
using Hardcodet.Wpf.TaskbarNotification;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using Application = System.Windows.Application;
using System.Windows.Threading;

namespace sDock
{
    /// <summary>
    /// This class holds the model data of the app and serialized to saved file
    /// </summary>
    [Serializable]
    public class AppState
    {
        public SettingsData settings { get; set; }

        public List<IconData> icons { get; set; }
    }

    /// <summary>
    /// Holds the global settings for the app
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        private double _iconRadius;
        public double IconRadius 
        {
            get { return _iconRadius; }
            set
            {
                _iconRadius = value;
                MainWindow.GetInstance().UpdateWindowDimension();
            }
        }

        private double _iconLargeRadius;
        public double IconLargeRadius
        {
            get { return _iconLargeRadius; }
            set
            {
                _iconLargeRadius = value;
                MainWindow.GetInstance().UpdateWindowDimension();
            }
        }

        public double CloseDistance { get; set; }
        public double EnteringDistance { get; set; }

        private string _version = "1.0.7";
        public string Version
        {
            get { return _version; }
        }

        public SettingsData()
        {
            // default values
            IconRadius = 30.0;
            IconLargeRadius = 70.0;
            CloseDistance = 0.5;
            EnteringDistance = 4.0;
        }
    }

    /// <summary>
    /// holder for class SettingsData as a means to reach them from all outer classes globally
    /// </summary>
    public class Settings
    {
        public static SettingsData settings { get; set; }
    }

    /// <summary>
    /// Main window logic
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskbarIcon taskbarIcon;

        private DispatcherTimer _timer;

        private static MainWindow _instance;

        private bool QuietMode;

        private MenuItem QuietModeMenuItem;

        public MainWindow()
        {
            InitializeComponent();
            _instance = this;
            Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Resources/github.ico"));
            taskbarIcon = new TaskbarIcon();
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/github.ico")).Stream;
            taskbarIcon.Icon = new System.Drawing.Icon(iconStream);
            taskbarIcon.ToolTipText = "Simon Dock";

            // init settings
            Settings.settings = new SettingsData();

            // taskbar menu
            taskbarIcon.ContextMenu = new ContextMenu();
            // add a settings button
            var settingsButton = new MenuItem();
            settingsButton.Header = "Settings";
            settingsButton.Click += (s, e) => EditSettings(s, e);
            taskbarIcon.ContextMenu.Items.Add(settingsButton);
            // add a mode button
            QuietModeMenuItem = new MenuItem();
            QuietModeMenuItem.Header = "Quiet Mode";
            QuietModeMenuItem.Click += (s, e) => QuietModeToggle(s, e);
            taskbarIcon.ContextMenu.Items.Add(QuietModeMenuItem);
            // add a close button 
            var closeButton = new MenuItem();
            closeButton.Header = "Close";
            closeButton.Click += (s, e) => Close();
            taskbarIcon.ContextMenu.Items.Add(closeButton);


            taskbarIcon.TrayLeftMouseDown += TaskbarIcon_TrayLeftMouseDown;

            IsEnabled = true;

            CompositionTarget.Rendering += OnRendering;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.5);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            // assign mouse down left click event to the main window to call the method MainWindow_MouseLeftButtonDown
            Drop += MainWindow_Drop;

            // assign window closing event to the main window to call the method MainWindow_Closing
            Closing += Window_Closing;

            // allow file drop
            AllowDrop = true;

            // move window to bottom
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = SystemParameters.WorkArea.Width / 2 - Width / 2;
            Top = SystemParameters.WorkArea.Height - Height;

            QuietMode = false;
            dock = new Dock();

            LoadState();

            if (dock.isEmpty())
            {
                // default icons
                InsertIcon(new Icon("empty.txt"));
            }

            System.Diagnostics.Debug.WriteLine("init");
        }

        public static MainWindow GetInstance()
        {
            return _instance;
        }

        private void EditSettings(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(Settings.settings);
            settingsWindow.ShowDialog();
        }

        private void QuietModeToggle(object sender, RoutedEventArgs e)
        {
            QuietMode = !QuietMode;
            if(QuietMode)
            {
                QuietModeMenuItem.Header = "Disable Quiet Mode";
            }
            else
            {
                QuietModeMenuItem.Header = "Quiet Mode";
            }
        }

        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Activate();
            WindowState = WindowState.Normal;
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    System.Diagnostics.Debug.WriteLine("adding file: " + file);
                    var icon = new Icon(file);
                    InsertIcon(icon);
                }
            }
        }

        // dock  member
        private Dock dock;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (QuietMode)
                return;

            var mousePos = System.Windows.Forms.Control.MousePosition;
            if (mousePos.Y > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 10 &&
                mousePos.X > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * 0.3 &&
                mousePos.X < System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * 0.6)
            {
                Activate();
                WindowState = WindowState.Normal;
                Topmost = true;
                Topmost = false;
            }
        }
        private void OnRendering(object sender, EventArgs e)
        {
            // step
            dock.Step(WinCanvas);

            // draw
            WinCanvas.Children.Clear();
            dock.Draw(WinCanvas);
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dock.on_leftDownClick(WinCanvas);
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("mouse up");
            dock.on_leftUpClick(WinCanvas);
        }

        private void Window_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("mouse right up");
            dock.on_rightclick(WinCanvas);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveState();
            taskbarIcon.Icon.Dispose();
        }

        private void SaveState()
        {
            var savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\sDock";
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            
            using (var writer = new StreamWriter(savePath + "\\state.xml"))
            {
                AppState state = new AppState()
                {
                    settings = Settings.settings,
                    icons = dock.GetIconDataList()
                };
                
                var serializer = new XmlSerializer(typeof(AppState));
                serializer.Serialize(writer, state);
            }
        }

        private void LoadState()
        {
            var savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\sDock";
            if (!Directory.Exists(savePath))
            {
                return;
            }
            savePath += "\\state.xml";
            if (!File.Exists(savePath))
            {
                return;
            }

            AppState appState;
            using (var reader = new StreamReader(savePath))
            {
                var serializer = new XmlSerializer(typeof(AppState));
                appState = (AppState)serializer.Deserialize(reader);
            }

            // load settings
            Settings.settings = appState.settings;

            // load icons
            foreach (var iconData in appState.icons)
            {
                InsertIcon(new Icon(iconData));
            }

        }

        private void InsertIcon(Icon icon)
        {
            dock.AddIcon(icon);
            UpdateWindowDimension();
        }

        public void UpdateWindowDimension()
        {
            if (dock == null)
                return;

            var width = Math.Min(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, dock.icons.Count * 2 * Settings.settings.IconRadius + 2 * Settings.settings.IconLargeRadius);
            var height = Settings.settings.IconLargeRadius * 2.0 + 20.0;

            Width = width;
            Height = height;

            Left = SystemParameters.WorkArea.Width / 2 - Width / 2;
            Top = SystemParameters.WorkArea.Height - Height;
        }

    }
}