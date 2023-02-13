﻿using System;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class Settings
    {
        public static double Width { get; set; }
        public static double DefaultRadius { get; set; }
        public static double DefaultLargeRadius { get; set; }
        public static double CloseDistance { get; set; }
        public static double EnteringDistance { get; set; }

        public static void Init()
        {
            DefaultRadius = 30.0;
            DefaultLargeRadius = 90.0;
            CloseDistance = 0.5;
            EnteringDistance = 4.0;
        }
    }

    public partial class MainWindow : Window
    {
        private TaskbarIcon taskbarIcon;

        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            Settings.Init();

            taskbarIcon = new TaskbarIcon();
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/github.ico")).Stream;
            taskbarIcon.Icon = new System.Drawing.Icon(iconStream);
            taskbarIcon.ToolTipText = "Simon Dock";

            // taskbar menu
            taskbarIcon.ContextMenu = new ContextMenu();

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

            dock = new Dock();
            LoadState();
            if (dock.isEmpty())
            {
                // default icons
                InsertIcon(new Icon("empty.txt"));
            }

            System.Diagnostics.Debug.WriteLine("init");
        }

        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            this.Activate();
            this.WindowState = WindowState.Normal;
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
                    //icon.image = image;
                    InsertIcon(icon);
                }
            }
        }

        // dock  member
        private Dock dock;

        private void Timer_Tick(object sender, EventArgs e)
        {
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
                List<IconData> icons = dock.GetIconDataList();
                var serializer = new XmlSerializer(typeof(List<IconData>));
                serializer.Serialize(writer, icons);
            }
        }

        private void LoadState()
        {
            var savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\sDock";
            if (!Directory.Exists(savePath))
            {
                return;
            }

            List<IconData> list;

            using (var reader = new StreamReader(savePath + "\\state.xml"))
            {
                var serializer = new XmlSerializer(typeof(List<IconData>));
                list = (List<IconData>)serializer.Deserialize(reader);
            }

            foreach (var data in list)
            {
                InsertIcon(new Icon(data));
            }

        }

        private void InsertIcon(Icon icon)
        {
            var newWidth = dock.AddIcon(icon);
            Width = newWidth;
            Left = SystemParameters.WorkArea.Width / 2 - Width / 2;
        }

    }
}