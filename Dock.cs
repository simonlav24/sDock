using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;

namespace SimonDock
{
    public class Dock
    {
        public Dock()
        {
        }
        
        // collection of icons
        public List<Icon> icons = new List<Icon>();

        // add icon method
        public void AddIcon(Icon icon)
        {           
            icons.Add(icon);
        }

        public bool isEmpty()
        {
            return icons.Count == 0;
        }

        public void Step(Canvas canvas)
        {
            foreach (var icon in icons)
            {
                icon.Step(canvas);
            }
        }

        // draw method        
        public void Draw(Canvas canvas)
        {
            var sumOfRadius = 0.0;
            foreach (var icon in icons)
            {
                sumOfRadius += icon.Radius;
            }

            var currentX = canvas.ActualWidth / 2 - sumOfRadius;
            foreach (var icon in icons)
            {
                var x = currentX + icon.Radius;
                currentX = x + icon.Radius;
                var y = canvas.ActualHeight - icon.Radius;
                icon.X = x;
                icon.Y = y - 20;
            }

            foreach (var icon in icons)
            {
                icon.Draw(canvas);
            }
        }
        
        private Icon? findIcon(Canvas canvas)
        {
            var mousePos = Mouse.GetPosition(canvas);
            foreach (var icon in icons)
            {
                var distance = Math.Sqrt(Math.Pow(mousePos.X - icon.X, 2) + Math.Pow(mousePos.Y - icon.Y, 2));
                if (distance < icon.Radius)
                {
                    Debug.WriteLine("clicked on " + icon.Data.Name);
                    return icon;
                }
            }
            return null;
        }

        public void on_rightclick(Canvas canvas)
        {
            var icon = findIcon(canvas);
            if (icon == null)
                return;
            // create a new context menu
            var contextMenu = new ContextMenu();
            
            // rename option
            var edit = new MenuItem();
            edit.Header = "Edit";
            edit.Click += (sender, args) => EditIcon(icon);
            contextMenu.Items.Add(edit);

            // remove option
            var remove = new MenuItem();
            remove.Header = "Remove";
            remove.Click += (sender, args) => RemoveIcon(icon);
            contextMenu.Items.Add(remove);

            if (File.Exists(icon.Data.Path))
            {
                var openLocation = new MenuItem();
                openLocation.Header = "Open file location";
                openLocation.Click += (sender, args) => OpenFileLocation(icon);
                contextMenu.Items.Add(openLocation);
            }

            // show the context menu
            contextMenu.IsOpen = true;
            
        }

        private void OpenLocation_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void on_leftDownClick(Canvas canvas)
        {
            var icon = findIcon(canvas);
            if (icon == null)
                return;
            icon.held = true;
        }

        public void on_leftUpClick(Canvas canvas)
        {
            var icon = findIcon(canvas);
            if (icon == null)
                return;
            foreach (var i in icons)
            {
                i.held = false;
            }
            if (icon.Data.Path != null)
            {
                if (File.Exists(icon.Data.Path))
                {
                    Process.Start(icon.Data.Path);
                }
                else if (Directory.Exists(icon.Data.Path))
                {
                    Process.Start("explorer.exe", icon.Data.Path);
                }
            }
        }

        public List<IconData> GetIconDataList()
        {
            var list = new List<IconData>();
            foreach (var icon in icons)
            {
                list.Add(icon.Data);
            }
            return list;
        }

        private void RemoveIcon(Icon icon)
        {
            icons.Remove(icon);
        }

        private void EditIcon(Icon icon)
        {
            var editWindow = new EditWindow(icon);
            editWindow.ShowDialog();
        }
    

        private void OpenFileLocation(Icon icon)
        {
            Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(icon.Data.Path));
        }

    }
}
