using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using IWshRuntimeLibrary;
using File = System.IO.File;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Shell;
using System.Drawing;
using System.Windows.Automation.Peers;
using System.Xml.Linq;
using System.ComponentModel;
using System.Windows.Shapes;

namespace sDock
{
    [Serializable]
    public class IconData
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string ImagePath { get; set; }

        public IconData()
        {
        }
    }
    public class Icon : INotifyPropertyChanged
    {
        public IconData Data;
        
        public const double DefaultRadius = 40.0;
        public const double HeldScale = 0.9;
        public const double DefaultLargeRadius = 70.0;
        public const double CloseDistance = 0.5;
        public const double EnteringDistance = 2.0;

        public double X { get; set; }
        public double Y { get; set; }

        public double Radius { get; set; }

        public string Name
        {
            get { return Data.Name; }
            set
            {
                Data.Name = value;
                OnPropertyChanged("Name");
                NameBlock.Text = value;
            }
        }

        public string ImagePath
        {
            get { return Data.ImagePath; }
            set
            {
                Data.ImagePath = value;
                OnPropertyChanged("ImagePath");
                IconImage = new Image
                {
                    Source = new BitmapImage(new Uri(value)),
                    Width = Icon.DefaultRadius * 2,
                    Height = Icon.DefaultRadius * 2
                };
            }
        }

        public bool Selected;
        public bool Dragged;

        public TextBlock NameBlock { get; set; }

        public Image IconImage { get; set; }

        public Icon(string path="icon.txt")
        {
            Data = new IconData();
            Radius = DefaultRadius;
            Data.Name = System.IO.Path.GetFileNameWithoutExtension(path);

            Data.Path = ResolvePath(path);
            NameBlock = new TextBlock
            {
                Text = Data.Name,
                FontSize = 16,
                Foreground = System.Windows.Media.Brushes.White
            };
            GetIconImage(Data.Path);

            Selected = false;
            Dragged = false;
        }
        
        public Icon(IconData data)
        {
            Data = data;
            Radius = DefaultRadius;
            NameBlock = new TextBlock
            {
                Text = Data.Name,
                FontSize = 16,
                Foreground = System.Windows.Media.Brushes.White
            };

            // handle icon image
            if (Data.ImagePath != null)
                GetIconImage(Data.ImagePath);
            else
                GetIconImage(Data.Path);

            Selected = false;
            Dragged = false;
        }
    

        public Icon()
        {
        }

        private string ResolvePath(string path)
        {
            if (path.EndsWith(".lnk"))
            {
                var shell = new IWshShell_Class();
                var shortcut = (IWshShortcut)shell.CreateShortcut(path);
                return shortcut.TargetPath;
            }
            else
            {
                return path;
            }
        }
    
        public void Step(Canvas canvas)
        {
            // get the mouse position in the main window
            var mousePos = Mouse.GetPosition(canvas);
            var distance = Math.Sqrt(Math.Pow(mousePos.X - X, 2) + Math.Pow(mousePos.Y - Y, 2));

            Radius = zoomLogisticFunction(distance);

            if (distance < Radius)
                Selected = true;
            else
                Selected = false;

            if (Dragged)
                X = mousePos.X;
        }

        public void Draw(Canvas canvas)
        {
            // draw the image
            if (IconImage != null)
            {
                canvas.Children.Add(IconImage);
                // scale the image to radius
                var scale = 2 * Radius / IconImage.ActualWidth;
                scale *= 0.97;
                ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
                IconImage.RenderTransform = scaleTransform;
                Canvas.SetLeft(IconImage, X - Radius);
                Canvas.SetTop(IconImage, Y - Radius);

                // reflect
                var reflection = new Image
                {
                    Source = IconImage.Source,
                    Width = IconImage.Width,
                    Height = IconImage.Height
                };
                var scaleTransformReflect = new ScaleTransform(scale, scale * -1);
                reflection.RenderTransform = scaleTransformReflect;
                reflection.Opacity = 0.3;
                canvas.Children.Add(reflection);
                Canvas.SetLeft(reflection, X - Radius);
                Canvas.SetTop(reflection, Y + 3 * Radius);
            }

            // draw Text
            if (Selected)
            {
                canvas.Children.Add(NameBlock);
                NameBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                System.Windows.Size textBlockSize = NameBlock.DesiredSize;
                Canvas.SetLeft(NameBlock, X - textBlockSize.Width / 2);
                Canvas.SetTop(NameBlock, canvas.ActualHeight - 25);
            }
        }

        // zoom functions
        private double zoomLogisticFunction(double distance)
        {
            var num = - (DefaultLargeRadius - DefaultRadius);
            var exp = -0.1 * (distance - (DefaultRadius * CloseDistance + DefaultRadius * EnteringDistance) / 2.0);
            var denom = 1 + Math.Exp(exp);

            return num / denom + DefaultLargeRadius;
        }

        private double zoomLinearFunction(double distance)
        {
            if(distance < DefaultRadius * 2)
            {
                if (distance < DefaultRadius * 0.5)
                {
                    return DefaultLargeRadius;
                }
                else
                {
                    return -((DefaultLargeRadius - DefaultRadius) /(DefaultRadius * 2 - DefaultRadius * 0.5)) * (distance - DefaultRadius * 2) + DefaultRadius;
                }
            }
            else
            {
                return DefaultRadius;
            }
        }

        private void GetIconImage(string path)
        {
            if (File.Exists(path))
            {
                // if icon is an image file
                if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg") || path.EndsWith(".ico"))
                {
                    ImagePath = path;
                }
                // icon is a file
                else
                {
                    ShellObject shellObject = ShellObject.FromParsingName(path);
                    var bmp = shellObject.Thumbnail.BitmapSource;
                    IconImage = new Image
                    {
                        Source = bmp,
                        Width = Icon.DefaultRadius * 2,
                        Height = Icon.DefaultRadius * 2
                    };
                }
            }
            else if (Directory.Exists(path))
            {
                // icon image to copy from default icon
                IconImage = new Image();
                IconImage = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/folderIcon.png")),
                    Width = Icon.DefaultRadius * 2,
                    Height = Icon.DefaultRadius * 2
                };
            }
            else
            {
                // invalid path
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}


