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

namespace SimonDock
{
    public class Icon
    {
        public const double DefaultRadius = 40.0;
        public const double DefaultLargeRadius = 70.0;
        public const double CloseDistance = 0.5;
        public const double EnteringDistance = 2.0;

        public double X { get; set; }
        public double Y { get; set; }

        public double Radius { get; set; }
        
        public String Name { get; set; }
        
        public TextBlock NameBlock { get; set; }
        
        public String AbsolutePath { get; set; }

        public Image IconImage { get; set; }

        public Icon(string path="icon.txt")
        {
            Radius = DefaultRadius;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);

            AbsolutePath = ResolvePath(path);
            NameBlock = new TextBlock
            {
                Text = Name,
                FontSize = 16,
                Foreground = System.Windows.Media.Brushes.White
            };
            GetIconImage(AbsolutePath);
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
        }

        public void Draw(Canvas canvas)
        {
            // draw the image
            if (IconImage is not null)
            {
                canvas.Children.Add(IconImage);
                // scale the image to radius
                var scale = 2 * Radius / IconImage.ActualWidth;
                scale *= 0.97;
                ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
                IconImage.RenderTransform = scaleTransform;
                Canvas.SetLeft(IconImage, X - Radius);
                Canvas.SetTop(IconImage, Y - Radius);
            }

            // draw Text
            canvas.Children.Add(NameBlock);
            NameBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            System.Windows.Size textBlockSize = NameBlock.DesiredSize;
            Canvas.SetLeft(NameBlock, X - textBlockSize.Width / 2);
            Canvas.SetTop(NameBlock, canvas.ActualHeight - 25);
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
                ShellObject shellObject = ShellObject.FromParsingName(path);
                var bmp = shellObject.Thumbnail.BitmapSource;
                IconImage = new Image
                {
                    Source = bmp,
                    Width = Icon.DefaultRadius * 2,
                    Height = Icon.DefaultRadius * 2
                };
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

    }
    


}


