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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls.Ribbon;

namespace SimonDock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private TaskbarIcon taskbarIcon;

        private int MouseDownTimer;

        public MainWindow()
        {
            InitializeComponent();
            Focus();
            taskbarIcon = new TaskbarIcon();
            taskbarIcon.Icon = new System.Drawing.Icon("../../../Resources/github.ico");
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
            MouseDownTimer = 0;

            CompositionTarget.Rendering += OnRendering;
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
                dock.AddIcon(new Icon("1.txt"));
                dock.AddIcon(new Icon("2.txt"));
                dock.AddIcon(new Icon("3.txt"));
            }

            System.Diagnostics.Debug.WriteLine("init");
        }

        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            this.Activate();
            this.WindowState = WindowState.Normal;
        }

        private void MainWindow_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if(e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                foreach (string file in files)
                {
                    //retrive icon of file
                    


                    System.Diagnostics.Debug.WriteLine("adding file: " + file);
                    var icon = new Icon(file);
                    //icon.image = image;
                    dock.AddIcon(icon);
                }
            }
        }

        // icon  member
        private Dock dock;


        private void OnRendering(object sender, EventArgs e)
        {
            // check mouse pos
            var mousePos = System.Windows.Forms.Control.MousePosition;
            if (mousePos.Y > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 20 &&
                mousePos.X > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * 0.3 &&
                mousePos.X < System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * 0.6)
            {
                MouseDownTimer++;
            }
            else
            {
                MouseDownTimer = 0;
            }
            if (MouseDownTimer > 20)
            {
                MouseDownTimer = 0;
                this.Activate();
                this.WindowState = WindowState.Normal;
            }

            WinCanvas.Children.Clear();
            dock.Draw(WinCanvas);
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("mouse down");
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("mouse up");
            dock.on_click(WinCanvas);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveState();
        }

        private void SaveState()
        {
            using (var writer = new StreamWriter("state.xml"))
            {
                List<string> paths = dock.GetPathList();
                var serializer = new XmlSerializer(typeof(List<string>));
                serializer.Serialize(writer, paths);
            }
        }

        private void LoadState()
        {
            if (!File.Exists("state.xml"))
            {
                return;
            }

            List<string> list;
            
            using (var reader = new StreamReader("state.xml"))
            {
                var serializer = new XmlSerializer(typeof(List<string>));
                list = (List<string>)serializer.Deserialize(reader);
            }

            foreach(var path in list)
            {
                dock.AddIcon(new Icon(path));
            }
       
        }

    }
}
