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
                    System.Diagnostics.Debug.WriteLine("clicked on " + icon.Name);
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
            var rename = new MenuItem();
            rename.Header = "Edit";
            contextMenu.Items.Add(rename);

            // remove option
            var remove = new MenuItem();
            remove.Header = "Remove";
            remove.Click += (sender, args) => RemoveIcon(icon);
            contextMenu.Items.Add(remove);

            if (File.Exists(icon.AbsolutePath))
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

        public void on_click(Canvas canvas)
        {
            var icon = findIcon(canvas);
            if (icon == null)
                return;
            if (icon.AbsolutePath != null)
            {
                if (System.IO.File.Exists(icon.AbsolutePath))
                {
                    System.Diagnostics.Process.Start(icon.AbsolutePath);
                }
                else if (System.IO.Directory.Exists(icon.AbsolutePath))
                {
                    Process.Start("explorer.exe", icon.AbsolutePath);
                }
            }
        }

        public List<string> GetPathList()
        {
            var list = new List<string>();
            foreach (var icon in icons)
            {
                list.Add(icon.AbsolutePath);
            }
            return list;
        }

        private void RemoveIcon(Icon iconToRemove)
        {
            icons.Remove(iconToRemove);
        }

        private void OpenFileLocation(Icon icon)
        {
            Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(icon.AbsolutePath));
        }

    }
}
