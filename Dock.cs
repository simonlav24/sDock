using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace sDock
{
    public enum DockState
    {
        Idle,
        DraggingTimer,
        Dragging
    }
    public class Dock
    {
        public Dock()
        {
            State = DockState.Idle;
            DraggingTimer = 0;
        }
        
        // collection of icons
        public List<Icon> icons = new List<Icon>();
        
        private DockState State;
        private int DraggingTimer;
        
        private Icon CurrentIcon;

        // add icon method
        public double AddIcon(Icon icon)
        {           
            icons.Add(icon);
            return Math.Min(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, icons.Count * 2 * Settings.settings.IconRadius + 2 * Settings.settings.IconLargeRadius);
        }

        public bool isEmpty()
        {
            return icons.Count == 0;
        }

        public void Step(Canvas canvas)
        {
            switch (State)
            {
                case DockState.Idle:
                    DraggingTimer = 0;
                    break;
                case DockState.DraggingTimer:
                    DraggingTimer++;
                    if (DraggingTimer > 20)
                    {
                        State = DockState.Dragging;
                        DraggingTimer = 0;
                    }
                    break;
                case DockState.Dragging:
                    if (CurrentIcon == null)
                    {
                        State = DockState.Idle;
                        DraggingTimer = 0;
                    }
                    else
                    {
                        CurrentIcon.Dragged = true;
                        var iconIndex = icons.IndexOf(CurrentIcon);
                        Icon NextIcon = null;
                        Icon PrevIcon = null;
                        if (iconIndex != icons.Count - 1)
                        {
                            NextIcon = icons[iconIndex + 1];
                        }
                        if (iconIndex != 0)
                        {
                            PrevIcon = icons[iconIndex - 1];
                        }
                        if (NextIcon != null)
                        {
                            if (CurrentIcon.X > NextIcon.X - NextIcon.Radius)
                            {
                                icons.Remove(CurrentIcon);
                                icons.Insert(iconIndex + 1, CurrentIcon);
                            }
                        }
                        if (PrevIcon != null)
                        {
                            if (CurrentIcon.X < PrevIcon.X + PrevIcon.Radius)
                            {
                                icons.Remove(CurrentIcon);
                                icons.Insert(iconIndex - 1, CurrentIcon);
                            }
                        }
                    }
                    break;
            }


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
                icon.Step(canvas);
            }
        }

        // draw method        
        public void Draw(Canvas canvas)
        {
            foreach (var icon in icons)
            {
                icon.Draw(canvas);
            }

            if (false)
            {
                // draw rectangle
                Rectangle rect = new Rectangle();
                rect.Fill = System.Windows.Media.Brushes.Red;
                rect.Width = 2.0 * icons.Count * Settings.settings.IconLargeRadius;
                rect.Height = 50;
                canvas.Children.Add(rect);
                Canvas.SetLeft(rect, canvas.ActualWidth / 2 - rect.Width / 2);
                Canvas.SetTop(rect, 0);
            }
        }
        
        private Icon findIcon(Canvas canvas)
        {
            var mousePos = Mouse.GetPosition(canvas);
            foreach (var icon in icons)
            {
                var distance = Math.Sqrt(Math.Pow(mousePos.X - icon.X, 2) + Math.Pow(mousePos.Y - icon.Y, 2));
                if (distance < icon.Radius)
                {
                    //Debug.WriteLine("clicked on " + icon.Data.Name);
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

        public void on_leftDownClick(Canvas canvas)
        {
            Debug.WriteLine("left down click");
            CurrentIcon = findIcon(canvas);
            if (CurrentIcon == null)
                return;
            State = DockState.DraggingTimer;
        }

        public void on_leftUpClick(Canvas canvas)
        {
            Debug.WriteLine("left up click");
            if (State == DockState.Dragging)
            {
                State = DockState.Idle;
                if (CurrentIcon != null)
                {
                    CurrentIcon.Dragged = false;
                    CurrentIcon = null;
                }
                return;
            }
            
            State = DockState.Idle;
            if (CurrentIcon == null)
                return;
            CurrentIcon.Dragged = false;
            
            if (CurrentIcon.Data.Path != null)
            {
                if (CurrentIcon.Data.Path.Contains(".py"))
                {
                    // python script
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = "py",
                        Arguments = CurrentIcon.Data.Path,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(CurrentIcon.Data.Path)
                };
                    Process.Start(startInfo);
                }
                else if (File.Exists(CurrentIcon.Data.Path))
                {
                    // open file
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = CurrentIcon.Data.Path,
                        Arguments = CurrentIcon.Data.Args,
                        UseShellExecute = true
                    };
                    try
                    {
                        Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }


                }
                else if (Directory.Exists(CurrentIcon.Data.Path))
                {
                    // open folder
                    Process.Start("explorer.exe", CurrentIcon.Data.Path);
                }
                else
                {
                    // run script
                    if(CurrentIcon.Data.Path.ToLower().Contains(".exe"))
                    {
                        if(CurrentIcon.Data.Path.Contains("\""))
                        {
                            var strings = CurrentIcon.Data.Path.Split('\"');
                            Process.Start(strings[1], strings[2]);
                        }
                        else
                        {
                            var strings = CurrentIcon.Data.Path.Split(' ');
                            string arguments = string.Join(" ", strings, 1, strings.Length - 1);
                            Process.Start(strings[0], arguments);
                        }
                    }
                }
                CurrentIcon.Bounce();
            }
            CurrentIcon = null;
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

        private void OpenLocation_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
