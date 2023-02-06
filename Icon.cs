using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimonDock
{    
    public class Icon
    {
        public const double DefaultRadius = 50;
        public const double DefaultLargeRadius = 70;

        public double X { get; set; }
        public double Y { get; set; }

        public double Radius { get; set; }
        
        public String Name { get; set; }
        
        public String AbsolutePath { get; set; }

        public System.Windows.Controls.Image? image;

        public Icon(string path="icon.txt")
        {
            Radius = DefaultRadius;
            AbsolutePath = path;
            Name = System.IO.Path.GetFileName(path);
        }

        public Icon()
        {
            X = 0;
            Y = 0;
            Radius = DefaultRadius;
            Name = "parameterless";
        }

        public void Draw(Canvas canvas)
        {
            // get the mouse position in the main window
            var mousePos = Mouse.GetPosition(canvas);
            //System.Diagnostics.Debug.WriteLine(mousePos);

            

            var distance = Math.Sqrt(Math.Pow(mousePos.X - X, 2) + Math.Pow(mousePos.Y - Y, 2));

            if(distance < DefaultRadius * 2)
            {
                Radius = -0.2 * distance + DefaultLargeRadius;
            }
            else
            {
                Radius = DefaultRadius;
            }

            // draw circle
            Ellipse ellipse = new Ellipse
            {
                Fill = System.Windows.Media.Brushes.Red,
                Width = 2 * Radius,
                Height = 2 * Radius
            };
            canvas.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, X - Radius);
            Canvas.SetTop(ellipse, Y - Radius);

            // draw image
            //Image image = new Image
            //{ 
            //    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("D:\\Projects\\SimonDock\\SimonDock\\assets\\github.png")),
            //    Width = 2 * Radius,
            //    Height = 2 * Radius
            //};
            //canvas.Children.Add(image);
            //Canvas.SetLeft(image, X - Radius);
            //Canvas.SetTop(image, Y - Radius);

            // draw the image
            if (image is not null)
            {
                canvas.Children.Add(image);
                // scale the image to radius
                ScaleTransform scale = new ScaleTransform(4, 4);
                image.RenderTransform = scale;
                Canvas.SetLeft(image, X - Radius);
                Canvas.SetTop(image, Y - Radius);

            }


            // draw Text
            TextBlock textBlock = new TextBlock
            {
                Text = Name,
                FontSize = 20,
                Foreground = System.Windows.Media.Brushes.White
            };
            
            canvas.Children.Add(textBlock);
            // draw the text in the center of the circle
            textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            System.Windows.Size textBlockSize = textBlock.DesiredSize;
            Canvas.SetLeft(textBlock, X - textBlockSize.Width / 2);
            Canvas.SetTop(textBlock, Y - textBlockSize.Height / 2);

        }

    }



}


