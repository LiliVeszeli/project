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

namespace Flow_Stitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
   

    public partial class MainWindow : Window
    {
        public BitmapImage pubBitmapImage = new BitmapImage();
        public Bitmap pubBitmap;
        

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
                    pubBitmapImage = bitmap;
                    image.Source = bitmap;
                }
                //if it isn't an image it displays an error message, the program doens't crash
                else
                {
                    MessageBox.Show("Error: Select a file with .jpg or .png extension.", "ERROR");
                }
            }
        }


        BitmapImage BitmapToImageSource(Bitmap bitmap)
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

                return bitmapimage;
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
            enc.Frames.Add(BitmapFrame.Create(pubBitmapImage));
            enc.Save(outStream);
            System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);


           // BitmapSourceToBitmap2(image.Source);

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    System.Drawing.Color pixelColor = img.GetPixel(i, j);
                    if (pixelColor == white)
                        img.SetPixel(i, j, red);
                }
            }

            pubBitmapImage = BitmapToImageSource(img);
            image.Source = pubBitmapImage;
            
        }


        private void ItemSave_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document";
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            if (dlg.ShowDialog() == true)
            {
                var encoder = new JpegBitmapEncoder(); // Or PngBitmapEncoder, or whichever encoder you want
                encoder.Frames.Add(BitmapFrame.Create(pubBitmapImage));
                using (var stream = dlg.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
        }

    }
}
