using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

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
                var y = canvas.ActualHeight / 2 - icon.Radius;
                icon.X = x;
                icon.Y = y;
            }

            foreach (var icon in icons)
            {
                icon.Draw(canvas);
            }
        }
        
        public void on_click(Canvas canvas)
        {
            // get the mouse position in the main window
            var mousePos = Mouse.GetPosition(canvas);
            //System.Diagnostics.Debug.WriteLine(mousePos);

            // find the icon 
            foreach (var icon in icons)
            {
                var distance = Math.Sqrt(Math.Pow(mousePos.X - icon.X, 2) + Math.Pow(mousePos.Y - icon.Y, 2));
                if (distance < icon.Radius)
                {
                    System.Diagnostics.Debug.WriteLine("clicked on " + icon.Name);
                    // open the file or folder
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
    }
}
