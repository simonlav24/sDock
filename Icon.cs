using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using IWshRuntimeLibrary;
using File = System.IO.File;
using Microsoft.WindowsAPICodePack.Shell;
using System.ComponentModel;

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

        public double X { get; set; }
        public double Y { get; set; }

        public double Ax { get; set; }

        public double Ay { get; set; }

        public double Radius { get; set; }

        private double AdditiveRadius { get; set; }
        
        private double RadiusVelocity { get; set; }

    public double Aradius { get; set; }
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
                    Width = Settings.DefaultRadius * 2,
                    Height = Settings.DefaultRadius * 2
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
            Radius = Settings.DefaultRadius;
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
            AdditiveRadius = 0;
        }
        
        public Icon(IconData data)
        {
            Data = data;
            Radius = Settings.DefaultRadius;
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
            AdditiveRadius = 0;
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
            var distance = Math.Abs(mousePos.X - X);

            Radius = zoomLogisticFunction(distance);

            if (distance < Radius)
                Selected = true;
            else
                Selected = false;

            if (Dragged)
                X = mousePos.X;

            Ax += (X - Ax) * 0.3;
            Ay += (Y - Ay) * 0.3;
            Aradius += (Radius - Aradius) * 0.3;

            RadiusVelocity += -(AdditiveRadius - 0.0) * 0.05;
            RadiusVelocity *= 0.9;
            AdditiveRadius += RadiusVelocity;

        }

        public void Draw(Canvas canvas)
        {
            // draw the image
            if (IconImage != null)
            {
                canvas.Children.Add(IconImage);
                // scale the image to radius
                var scale = 2 * (Aradius + AdditiveRadius) / IconImage.ActualWidth;
                scale *= 0.97;
                ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
                IconImage.RenderTransform = scaleTransform;
                Canvas.SetLeft(IconImage, Ax - (Aradius + AdditiveRadius));
                Canvas.SetTop(IconImage, Ay - (Aradius + AdditiveRadius));

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
                Canvas.SetLeft(reflection, Ax - Aradius);
                Canvas.SetTop(reflection, Ay + 3 * Aradius);
            }

            // draw Text
            if (Selected)
            {
                canvas.Children.Add(NameBlock);
                NameBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                System.Windows.Size textBlockSize = NameBlock.DesiredSize;
                Canvas.SetLeft(NameBlock, Ax - textBlockSize.Width / 2);
                Canvas.SetTop(NameBlock, canvas.ActualHeight - 25);
            }
        }

        // zoom functions
        private double zoomLogisticFunction(double distance)
        {
            var s = 0.03;
            // smaller s -> bigger spread

            var num = - (Settings.DefaultLargeRadius - Settings.DefaultRadius);
            var exp = - s * (distance - (Settings.DefaultRadius * Settings.CloseDistance + Settings.DefaultRadius * Settings.EnteringDistance) / 2.0);
            var denom = 1 + Math.Exp(exp);

            return num / denom + Settings.DefaultLargeRadius;
        }

        private double zoomLinearFunction(double distance)
        {
            if(distance < Settings.DefaultRadius * 2)
            {
                if (distance < Settings.DefaultRadius * 0.5)
                {
                    return Settings.DefaultLargeRadius;
                }
                else
                {
                    return -((Settings.DefaultLargeRadius - Settings.DefaultRadius) /(Settings.DefaultRadius * 2 - Settings.DefaultRadius * 0.5)) * (distance - Settings.DefaultRadius * 2) + Settings.DefaultRadius;
                }
            }
            else
            {
                return Settings.DefaultRadius;
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
                        Width = Settings.DefaultRadius * 2,
                        Height = Settings.DefaultRadius * 2
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
                    Width = Settings.DefaultRadius * 2,
                    Height = Settings.DefaultRadius * 2
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

        public void Bounce()
        {
            AdditiveRadius = -Settings.DefaultRadius;
        }

    }
}


