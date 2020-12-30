using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PaintTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Working array of colours - we will edit pixels in this
        private Color[,] pixels;
        private int width, height;

        // Bitmap of pixels for display in WPF, it is a copy of the pixels array above. Updated it with the UpdatePixelsBitmap method (see below)
        private WriteableBitmap pixelsBitmap;

        // Zoom
        private Double zoomMax = 25;
        private Double zoomMin = 0.5;
        private Double zoomSpeed = 1.001;
        private Double zoom = 1;


        public MainWindow()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create a writable bitmap and load a file into it
            BitmapImage loadImage = new BitmapImage(new Uri(@"../../john.png", UriKind.RelativeOrAbsolute));
            pixelsBitmap = new WriteableBitmap(loadImage);
            width = pixelsBitmap.PixelWidth;
            height = pixelsBitmap.PixelHeight;

            zoom = this.ActualWidth / width;
          
            // Zoom from top left when zooming out
            CanvasDraw.RenderTransform = new ScaleTransform(zoom, zoom); // transform Canvas size
            

            // New array of pixels to match the bitmap, one for each pixel
            pixels = new Color[height, width];
            GetPixelsFromBitmap();

            // Set bitmap as source of image which is in the WPF
            ImageDraw.Source = pixelsBitmap;
        }


        // Update writable bitmap with our own array of pixel colours
        private void UpdatePixelsBitmap()
        {
            // Copy the data into a one-dimensional array.
            byte[] pixels1d = new byte[height * width * 4];
            int index = 0;
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    pixels1d[index++] = pixels[row, col].R;
                    pixels1d[index++] = pixels[row, col].G;
                    pixels1d[index++] = pixels[row, col].B;
                    pixels1d[index++] = pixels[row, col].A;
                }
            }

            // Copy pixels over to writeable bitmap
            Int32Rect rect = new Int32Rect(0, 0, width, height);
            int stride = 4 * width;
            pixelsBitmap.WritePixels(rect, pixels1d, stride, 0);
        }


        // Get pixels array from writable bitmap (opposite of above method), used when we load an image into a bitmap
        private void GetPixelsFromBitmap()
        {
            // One dimensional array to get pixel data
            byte[] pixels1d = new byte[height * width * 4];

            // Copy pixels from writeable bitmap
            Int32Rect rect = new Int32Rect(0, 0, width, height);
            int stride = 4 * width;
            pixelsBitmap.CopyPixels(rect, pixels1d, stride, 0);

            // Copy the data from one-dimensional array into our pixels array
            int index = 0;
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    pixels[row, col].R = pixels1d[index++];
                    pixels[row, col].G = pixels1d[index++];
                    pixels[row, col].B = pixels1d[index++];
                    pixels[row, col].A = pixels1d[index++];
                }
            }
        }


        // Zoom on Mouse wheel
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            zoom *= Math.Pow(zoomSpeed, e.Delta); // Ajust zooming speed (e.Delta = Mouse wheel value )
            if (zoom < zoomMin) { zoom = zoomMin; } // Limit Min Scale
            if (zoom > zoomMax) { zoom = zoomMax; } // Limit Max Scale

            Point mousePos = e.GetPosition(CanvasDraw);

            if (zoom > 1)
            {
                // Zoom in on mouse cursor when zooming in (doesn't quite work right if you move the mouse, but it's a start)
                CanvasDraw.RenderTransform = new ScaleTransform(zoom, zoom, mousePos.X, mousePos.Y); 
            }
            else
            {
                // Zoom from top left when zooming out
                CanvasDraw.RenderTransform = new ScaleTransform(zoom, zoom); // transform Canvas size
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            zoom = this.ActualWidth / width;



            // Zoom from top left when zooming out
            CanvasDraw.RenderTransform = new ScaleTransform(zoom, zoom); // transform Canvas size
        }

        // Plot pixels where you click
        private void ImageDraw_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(ImageDraw);
            pixels[(int)p.Y, (int)p.X] = Color.FromRgb(0,0,0);
            UpdatePixelsBitmap();
        }
    }
}
