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

namespace SimonDock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            dock = new Dock();
            LoadState();

            IsEnabled = true;

            CompositionTarget.Rendering += OnRendering;
            // assign mouse down left click event to the main window to call the method MainWindow_MouseLeftButtonDown
            Drop += MainWindow_Drop;

            // assign window closing event to the main window to call the method MainWindow_Closing
            Closing += Window_Closing;

            // allow file drop
            AllowDrop = true;

            if (dock.isEmpty())
            {
                dock.AddIcon(new Icon("1.txt"));
                dock.AddIcon(new Icon("2.txt"));
                dock.AddIcon(new Icon("3.txt"));
            }

            System.Diagnostics.Debug.WriteLine("init");
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
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

            double width = WinCanvas.ActualWidth;
            double height = WinCanvas.ActualHeight;
            //double radius = Math.Min(width, height) / 2;
            double x = width / 2;
            double y = height / 2;

            WinCanvas.Children.Clear();
            dock.Draw(WinCanvas);
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("mouse down");
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
