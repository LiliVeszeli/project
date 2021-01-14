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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Flow_Stitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    //public class ListItemColour
    //{
    //    public string Name { get; set; }
    //    public System.Windows.Media.Color color { get; set; }

    //}

    public class ListItemColour : INotifyPropertyChanged
    {
        public string _Name { get; set; }
        public System.Windows.Media.Color _Color { get; set; }
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        public ListItemColour()
        {
        }

        //public ListItemColour(System.Windows.Media.Color value, string name)
        //{
        //    this.color = value;
        //    this.Name = name;
        //}

        public System.Windows.Media.Color color
        {
            get { return _Color; }
            set
            {
                _Color = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public partial class MainWindow : Window
    {
        
     
        WriteableBitmap wBitmap;

        // Working array of colours - we will edit pixels in this
        System.Windows.Media.Color[,] pixels;
        private int width, height;

        bool isDrawing = false;
        System.Windows.Media.Color currentColour = System.Windows.Media.Color.FromRgb(0, 0, 0);
        System.Drawing.Color[] palette;
        ObservableCollection<ListItemColour> items = new ObservableCollection<ListItemColour>();

        public MainWindow()
        {
            InitializeComponent();
           
        }

   
        //opens colour picker
        private void OpenColourPicker()
        {
            var initialColor = Colors.Blue;
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                currentColour = dialog.Color;
            }
            
        }


        //loads image
        private void ItemOpen_Click(object sender, RoutedEventArgs e)
        {

            int numberOfColours = 0;
            int heightOfPattern = 0;

            //opening other window for setup
            NewPicture window = new NewPicture();
            window.ShowDialog();

            //parsing input strings
            if (!(int.TryParse(window.ColourtextBox.Text, out numberOfColours)))
            {
                numberOfColours = 0;
            }
            if (!(int.TryParse(window.HeighttextBox.Text, out heightOfPattern)))
            {
                heightOfPattern = 0;
            }
           
            //only opening a picture if input is not empty/invalid
            if(numberOfColours != 0 && heightOfPattern != 0)
            {
                //opening files
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

           
                //convert to bitmap
                MemoryStream outStream = new MemoryStream();

                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(wBitmap));
                enc.Save(outStream);
                System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);

                //calculating new size in percentage
                float newSizePercentage = (float)heightOfPattern / (float)img.Height;
                newSizePercentage *= 100;

                //reduce colour palette
               ColorImageQuantizer quantizer = new ColorImageQuantizer(new MedianCutQuantizer());
               System.Drawing.Bitmap quantizedImage = quantizer.ReduceColors(img, numberOfColours);

               

                //resize image
                System.Drawing.Bitmap scaledImage = ScaleByPercent(quantizedImage, newSizePercentage, heightOfPattern);
                
                //requantize
                System.Drawing.Bitmap requantizedImage = quantizer.ReduceColors(scaledImage, numberOfColours);

                //getting the colours in the pattern
                palette = quantizer.CalculatePalette(scaledImage, numberOfColours);

                //data binding
               


                for (int i = 0; i < palette.Count(); i++)
                {
                    //string val = "   " + palette[i].Name;
                    //System.Windows.Media.Color col = System.Windows.Media.Color.FromRgb(palette[i].R, palette[i].G, palette[i].B);

                    items.Add(new ListItemColour() { Name= "   " + palette[i].Name , color= System.Windows.Media.Color.FromRgb(palette[i].R, palette[i].G, palette[i].B) });
                }
                listBox.ItemsSource = items;


                Bitmap newBitmap = new Bitmap(requantizedImage);
                //putting it back into the image and the writable bitmap
                wBitmap = BitmapToImageSource(newBitmap);
                image.Source = wBitmap;

                width = wBitmap.PixelWidth;
                height = wBitmap.PixelHeight;

                // New array of pixels to match the bitmap, one for each pixel
                pixels = new System.Windows.Media.Color[height, width];
                GetPixelsFromBitmap();
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
            wBitmap.WritePixels(rect, pixels1d, stride, 0);
        }


        // Get pixels array from writable bitmap (opposite of above method), used when we load an image into a bitmap
        private void GetPixelsFromBitmap()
        {
            // One dimensional array to get pixel data
            byte[] pixels1d = new byte[height * width * 4];

            // Copy pixels from writeable bitmap
            Int32Rect rect = new Int32Rect(0, 0, width, height);
            int stride = 4 * width;
            wBitmap.CopyPixels(rect, pixels1d, stride, 0);

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


        //scaling down image
        static System.Drawing.Bitmap ScaleByPercent(System.Drawing.Image imgPhoto, float Percent, int Height)
        {
            float nPercent = ((float)Percent / 100);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;

            int destX = 0;
            int destY = 0;
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = Height;

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

        //clicking on the draw button
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
        }

        //clicking on the colour picker button
        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            var initialColor = Colors.Blue;
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                currentColour = dialog.Color;
            }
        }


        //change existing palette colour
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Color existingColor;
            System.Windows.Media.Color nextColor;
            string currentName;
            string nextName;
            var initialColor = Colors.Blue;
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                //the colour it will change to
                nextColor = dialog.Color;

                //getting the selected listbox item
                ListItemColour selectedItem = (ListItemColour)listBox.SelectedItem;

                //getting current color in palette that will be changed
                existingColor = selectedItem.color;
                currentName = selectedItem.Name;

                //making color variables for setting the pixels
                System.Drawing.Color previous = System.Drawing.Color.FromArgb(existingColor.R, existingColor.G, existingColor.B);
                System.Drawing.Color next = System.Drawing.Color.FromArgb(nextColor.R, nextColor.G, nextColor.B);
                nextName = next.R.ToString("X2") + next.G.ToString("X2") + next.B.ToString("X2");

                //converting to bitmap
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
                        if (pixelColor == previous)
                            img.SetPixel(i, j, next);
                    }
                }

         
                (listBox.SelectedItem as ListItemColour).color = nextColor;
                (listBox.SelectedItem as ListItemColour).Name = "   " + nextName.ToLower();


                wBitmap = BitmapToImageSource(img);
                image.Source = wBitmap;
            }
        }


        

        //if image pixel is clicked
        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // coordinates are now available in p.X and p.Y
            var p = e.GetPosition(image);

            System.Drawing.Color bitmapColour = System.Drawing.Color.FromArgb(currentColour.R, currentColour.G, currentColour.B);

            //converting to bitmap
            MemoryStream outStream = new MemoryStream();

            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(wBitmap));
            enc.Save(outStream);
            System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);

            
            var source = (BitmapSource)image.Source;

            //calculating pixel position
            double pixelWidth = source.PixelWidth;
            double pixelHeight = source.PixelHeight;
            double dx = pixelWidth * p.X / image.ActualWidth;
            double dy = pixelHeight * p.Y / image.ActualHeight;

            //converting to int
            int x = (int)dx;
            int y = (int)dy;
           
            img.SetPixel(x, y, bitmapColour);

            //System.Windows.Point p = e.GetPosition(image);
            //var pix = img.GetPixel((int)clickedPoint.X, (int)clickedPoint.Y);

            //double pixelWidth = image.Source.Width;
            //double pixelHeight = image.Source.Height;
            //double x = pixelWidth * p.X / image.ActualWidth;
            //double y = pixelHeight * p.Y / image.ActualHeight;

            //pixels[(int)y, (int)x] = System.Windows.Media.Color.FromRgb(0, 0, 0);
            //UpdatePixelsBitmap();

            wBitmap = BitmapToImageSource(img);
            image.Source = wBitmap;
        }

        //private void Palette_Click(object sender, RoutedEventArgs e)
        //{

      

        //    //making it into a bitmap
        //    MemoryStream outStream = new MemoryStream();

        //    BitmapEncoder enc = new BmpBitmapEncoder();
        //    enc.Frames.Add(BitmapFrame.Create(wBitmap));
        //    enc.Save(outStream);
        //    System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);

        

        //    //    var list = new Dictionary<int, int>();

          


        //    //    //getting the colours in the image
        //    //    for (int x = 0; x < img.Width; x++)
        //    //    {
        //    //        for (int y = 0; y < img.Height; y++)
        //    //        {
        //    //            int rgb = img.GetPixel(x, y).ToArgb();


        //    //            var added = false;
        //    //            for (int i = 0; i < 10; i++)
        //    //            {
        //    //                if (list.ContainsKey(rgb + i))
        //    //                {
        //    //                    list[rgb + i]++;
        //    //                    added = true;
        //    //                    break;
        //    //                }
        //    //                if (list.ContainsKey(rgb - i))
        //    //                {
        //    //                    list[rgb - i]++;
        //    //                    added = true;
        //    //                    break;
        //    //                }
        //    //            }
        //    //            //adding new colours to the list, using a value to see how many times they have been added.
        //    //            if (!added)
        //    //                list.Add(rgb, 1);
        //    //        }
        //    //    }

        //    //    //sort the list of colours in descending order. Most common on top.
        //    //    var mySortedList = list.OrderByDescending(d => d.Value).ToList();
        //    //    var commonColour = mySortedList.Select(kvp => kvp.Key).ToList();


        //    //    int difference;
        //    //    int closestColour = 0;
        //    //    int closestColourDifference = 10000000;

        //    //    for (int i = 0; i < img.Width; i++)
        //    //    {
        //    //        for (int j = 0; j < img.Height; j++)
        //    //        {
        //    //            int pixelColor = img.GetPixel(i, j).ToArgb();
        //    //            closestColourDifference = 10000000;

        //    //            for (int k = 0; k < commonColour.Count(); k++)
        //    //            {
        //    //                difference = pixelColor - commonColour[k];

        //    //                if(Math.Abs(difference) < closestColourDifference)
        //    //                {
        //    //                    closestColour = commonColour[k];
        //    //                    closestColourDifference = difference;
        //    //                }
        //    //            }

        //    //            System.Drawing.Color c = System.Drawing.Color.FromArgb(closestColour);

        //    //            img.SetPixel(i, j, c);
        //    //        }
        //    //    }

        //    //    //difference = sqrt(sqr(red1 - red2) + sqr(green1 - green2) + sqr(blue1 - blue2))

        //        wBitmap = BitmapToImageSource(newImage);
        //       image.Source = wBitmap;
        //}
    }
}
