using Dsafa.WpfColorPicker;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Media.Media3D;
using AForge.Imaging.ColorReduction;

namespace Flow_Stitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
   

    public partial class MainWindow : Window
    {
        //public BitmapImage pubBitmapImage = new BitmapImage();
        //public Bitmap pubBitmap;
        WriteableBitmap wBitmap;
        
        

        public MainWindow()
        {
            InitializeComponent();
        }

   
        //opens colour picker
        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            var initialColor = Colors.Blue;
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                var newColor = dialog.Color;
            }
        }


        //loads image
        private void ItemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            //filters files, so only images can be selected
            dlg.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
             "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
             "Portable Network Graphic (*.png)|*.png";
            dlg.RestoreDirectory = true;
            Nullable<bool> result = dlg.ShowDialog();

            //if OK is presssed
            if (result == true)
            {
                string selectedFileName = dlg.FileName;
                //create a bitmap and load the image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedFileName);

                //checks if the file selected is an image
                if (selectedFileName.Contains(".jpg") || selectedFileName.Contains(".png") || selectedFileName.Contains(".PNG") || selectedFileName.Contains(".jpeg"))
                {
                    bitmap.EndInit();
                    //pubBitmapImage = bitmap;
                    wBitmap = new WriteableBitmap(bitmap);
                    image.Source = wBitmap;
                }
                //if it isn't an image it displays an error message, the program doens't crash
                else
                {
                    MessageBox.Show("Error: Select a file with .jpg or .png extension.", "ERROR");
                }
            }
        }


        WriteableBitmap BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return new WriteableBitmap(bitmapimage);
            }
        }

        //public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        //{
        //    using (var memory = new MemoryStream())
        //    {
        //        bitmap.Save(memory, ImageFormat.Png);
        //        memory.Position = 0;

        //        var bitmapImage = new BitmapImage();
        //        bitmapImage.BeginInit();
        //        bitmapImage.StreamSource = memory;
        //        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmapImage.EndInit();
        //        bitmapImage.Freeze();

        //        return bitmapImage;
        //    }
        //}


        private void ChangeColour_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Color red = System.Drawing.Color.FromArgb(255, 0, 0);
            System.Drawing.Color white = System.Drawing.Color.FromArgb(255, 255, 255);

            
            MemoryStream outStream = new MemoryStream();

            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(wBitmap));
            enc.Save(outStream);
            System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);

           

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    System.Drawing.Color pixelColor = img.GetPixel(i, j);
                    if (pixelColor == white)
                        img.SetPixel(i, j, red);
                }
            }

            wBitmap = BitmapToImageSource(img);
            image.Source = wBitmap;
            
        }


        private void ItemSave_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document";
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            if (dlg.ShowDialog() == true)
            {
                var encoder = new JpegBitmapEncoder(); // Or PngBitmapEncoder, or whichever encoder you want
                encoder.Frames.Add(BitmapFrame.Create(wBitmap));
                using (var stream = dlg.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
        }


        static System.Drawing.Bitmap ScaleByPercent(System.Drawing.Image imgPhoto, float Percent)
        {
            float nPercent = ((float)Percent / 100);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;

            int destX = 0;
            int destY = 0;
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(destWidth, destHeight,
                                     System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                                    imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new System.Drawing.Rectangle(destX, destY, destWidth, destHeight),
                new System.Drawing.Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }





        private void Resize_Click(object sender, RoutedEventArgs e)
        {
            MemoryStream outStream = new MemoryStream();

            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(wBitmap));
            enc.Save(outStream);
            System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);


            System.Drawing.Bitmap newImage = ScaleByPercent(img, 50);

            wBitmap = BitmapToImageSource(newImage);
            image.Source = wBitmap;
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var clickedPoint = e.GetPosition((System.Windows.Controls.Image)sender);
            // coordinates are now available in clickedPoint.X and clickedPoint.Y
            System.Drawing.Color red = System.Drawing.Color.FromArgb(255, 0, 0);

            MemoryStream outStream = new MemoryStream();

            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(wBitmap));
            enc.Save(outStream);
            System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);

            img.SetPixel((int)clickedPoint.X, (int)clickedPoint.Y, red);


            wBitmap = BitmapToImageSource(img);
            image.Source = wBitmap;
        }

        private void Palette_Click(object sender, RoutedEventArgs e)
        {

            ColorImageQuantizer quantizer = new ColorImageQuantizer(new MedianCutQuantizer());

            //making it into a bitmap
            MemoryStream outStream = new MemoryStream();

            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(wBitmap));
            enc.Save(outStream);
            System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);

            Bitmap newImage = quantizer.ReduceColors(img, 8);

            //    var list = new Dictionary<int, int>();

          


            //    //getting the colours in the image
            //    for (int x = 0; x < img.Width; x++)
            //    {
            //        for (int y = 0; y < img.Height; y++)
            //        {
            //            int rgb = img.GetPixel(x, y).ToArgb();


            //            var added = false;
            //            for (int i = 0; i < 10; i++)
            //            {
            //                if (list.ContainsKey(rgb + i))
            //                {
            //                    list[rgb + i]++;
            //                    added = true;
            //                    break;
            //                }
            //                if (list.ContainsKey(rgb - i))
            //                {
            //                    list[rgb - i]++;
            //                    added = true;
            //                    break;
            //                }
            //            }
            //            //adding new colours to the list, using a value to see how many times they have been added.
            //            if (!added)
            //                list.Add(rgb, 1);
            //        }
            //    }

            //    //sort the list of colours in descending order. Most common on top.
            //    var mySortedList = list.OrderByDescending(d => d.Value).ToList();
            //    var commonColour = mySortedList.Select(kvp => kvp.Key).ToList();


            //    int difference;
            //    int closestColour = 0;
            //    int closestColourDifference = 10000000;

            //    for (int i = 0; i < img.Width; i++)
            //    {
            //        for (int j = 0; j < img.Height; j++)
            //        {
            //            int pixelColor = img.GetPixel(i, j).ToArgb();
            //            closestColourDifference = 10000000;

            //            for (int k = 0; k < commonColour.Count(); k++)
            //            {
            //                difference = pixelColor - commonColour[k];

            //                if(Math.Abs(difference) < closestColourDifference)
            //                {
            //                    closestColour = commonColour[k];
            //                    closestColourDifference = difference;
            //                }
            //            }

            //            System.Drawing.Color c = System.Drawing.Color.FromArgb(closestColour);

            //            img.SetPixel(i, j, c);
            //        }
            //    }

            //    //difference = sqrt(sqr(red1 - red2) + sqr(green1 - green2) + sqr(blue1 - blue2))

                wBitmap = BitmapToImageSource(newImage);
               image.Source = wBitmap;
        }
    }
}
