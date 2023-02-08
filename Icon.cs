using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TsudaKageyu;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;
using Image = System.Windows.Controls.Image;

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
            AbsolutePath = path;
            Name = System.IO.Path.GetFileName(path);
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

        public void Draw(Canvas canvas)
        {
            // get the mouse position in the main window
            var mousePos = Mouse.GetPosition(canvas);
            //System.Diagnostics.Debug.WriteLine(mousePos);

            var distance = Math.Sqrt(Math.Pow(mousePos.X - X, 2) + Math.Pow(mousePos.Y - Y, 2));

            Radius = zoomLogisticFunction(distance);
            //Radius = zoomLinearFunction(distance);

            // draw the image
            if (IconImage is not null)
            {
                canvas.Children.Add(IconImage);
                // scale the image to radius
                var scale = 2 * Radius / IconImage.ActualWidth;
                ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
                IconImage.RenderTransform = scaleTransform;
                Canvas.SetLeft(IconImage, X - Radius);
                Canvas.SetTop(IconImage, Y - Radius);
            }
            else
            {
                /*// draw circle
                Ellipse ellipse = new Ellipse
                {
                    Fill = System.Windows.Media.Brushes.Red,
                    Width = 2 * Radius,
                    Height = 2 * Radius
                };
                if (!canvas.Children.Contains(ellipse))
                    canvas.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, X - Radius);
                Canvas.SetTop(ellipse, Y - Radius);*/
            }

            // draw Text
            canvas.Children.Add(NameBlock);
            NameBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            System.Windows.Size textBlockSize = NameBlock.DesiredSize;
            Canvas.SetLeft(NameBlock, X - textBlockSize.Width / 2);
            Canvas.SetTop(NameBlock, canvas.ActualHeight - 25);

        }

        // icon image
        private void GetIconImage(string path)
        {
            if (File.Exists(path))
            {
                // is file
                IconExtractor ie = new IconExtractor(path);
                string fileName = ie.FileName;
                int iconCount = ie.Count;

                //System.Drawing.Icon icon0 = ie.GetIcon(0);
                //System.Drawing.Icon icon1 = ie.GetIcon(1);

                System.Drawing.Icon[] allIcons = ie.GetAllIcons();
                // get the first biggest
                System.Drawing.Icon biggest = allIcons[0];
                foreach(var i in allIcons)
                {
                    if (i.Size.Width > biggest.Size.Width)
                        biggest = i;
                }
                IconImage = new Image();
                IconImage.Source = Imaging.CreateBitmapSourceFromHIcon(
                    biggest.Handle,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

            }
            else if (Directory.Exists(path))
            {
                // icon image to copy from default icon
                IconImage = new Image();
                IconImage = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/circle.png")),
                    Width = Icon.DefaultRadius * 2,
                    Height = Icon.DefaultRadius * 2
                };
            }
            else
            {
                // invalid path
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

    }



}


