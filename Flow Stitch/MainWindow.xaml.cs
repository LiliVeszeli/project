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
using CsvHelper;
using System.Globalization;

namespace Flow_Stitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class DMC
    {
        public string Floss { get; set; }
        public string Description { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

       // public System.Windows.Media.Color color { get; set; }

    }

    //class for the listbox item
    public class ListItemColour : INotifyPropertyChanged
    {
        public string _Name { get; set; } //description

        public string _Number { get; set; } //floss
        public System.Windows.Media.Color _Color { get; set; } //rgb
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        public ListItemColour()
        {
        }        

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

        public string Number
        {
            get { return _Number; }
            set
            {
                _Number = value;
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
        
        //stores the image 
        WriteableBitmap wBitmap;

        // Working array of colours - we will edit pixels in this
        System.Windows.Media.Color[,] pixels;
        private int width, height;

        bool isDrawing = false;
        bool isEraser = false;
        System.Windows.Media.Color currentColour = System.Windows.Media.Color.FromRgb(0, 0, 0); //stores the color that is currently used
        //DMC currentDMCColour = new DMC();
        System.Drawing.Color[] palette; //stores the colours in the pattern
        ObservableCollection<ListItemColour> items = new ObservableCollection<ListItemColour>(); //stores listbox items

        ObservableCollection<DMC> DMCitems = new ObservableCollection<DMC>(); //stores lisbox items, but DMC colors
        List<DMC> DMCColors = new List<DMC>(); //stores all DMC colours

        //stores image for undo and redo functionality
        List<WriteableBitmap> patternStates = new List<WriteableBitmap>();
        int currentIndex = -1; //stores which image stored is the current state

        public MainWindow()
        {
            InitializeComponent();

            //reading in DMC colors
            using (var reader = new StreamReader("dmc.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {

                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var record = new DMC
                    {
                        Floss = csv.GetField("Floss"),
                        Description = csv.GetField("Description"),
                        Red = csv.GetField<int>("Red"),
                        Green = csv.GetField<int>("Green"),
                        Blue = csv.GetField<int>("Blue"),
                    };
                    DMCColors.Add(record);
                }
            }

            //currentDMCColour.Description = "Black";
            //currentDMCColour.Floss = "310";
            //currentDMCColour.Red = 0;
            //currentDMCColour.Green = 0;
            //currentDMCColour.Blue = 0;
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

                patternStates.Clear();
                currentIndex = -1;

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
                System.Drawing.Bitmap requantizedImage = new Bitmap(quantizer.ReduceColors(scaledImage, numberOfColours));

                //getting the colours in the pattern
               // palette = quantizer.CalculatePalette(requantizedImage, numberOfColours);

                wBitmap = BitmapToImageSource(requantizedImage);
                image.Source = wBitmap;

                //getting color palette of quantized image
                BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);

                DMC closestColor = new DMC();
                double distance = 1000;
                List<System.Drawing.Color> paletteList = new List<System.Drawing.Color>();
                DMCitems.Clear();

                //getting closest DMC colours to RGB
                for (int i = 0; i < myPalette.Colors.Count(); i++)
                {
                    for(int j = 0; j < DMCColors.Count(); j++)
                    {
                        double d = ((myPalette.Colors[i].R - DMCColors[j].Red) * 0.30) * ((myPalette.Colors[i].R - DMCColors[j].Red) * 0.30)
                            +((myPalette.Colors[i].G - DMCColors[j].Green) * 0.59) * ((myPalette.Colors[i].G - DMCColors[j].Green) * 0.59)
                            +((myPalette.Colors[i].B - DMCColors[j].Blue) * 0.11) * ((myPalette.Colors[i].B - DMCColors[j].Blue) * 0.11);

                        if(d < distance)
                        {
                            closestColor = DMCColors[j];
                            distance = d;
                        }
                    }
                    DMCitems.Add(closestColor);
                    distance = 1000;
                    paletteList.Add(System.Drawing.Color.FromArgb(closestColor.Red, closestColor.Green, closestColor.Blue));
                }

                //making the list into a simple array so that it can be passed to the quantizer
                palette = paletteList.ToArray();
  
                items.Clear();

                //data binding
                //making listbox items dynamically
                //for (int i = 0; i < palette.Count(); i++)
                //{                      
                //    items.Add(new ListItemColour() { Name= "   #" + palette[i].Name , color= System.Windows.Media.Color.FromRgb(palette[i].R, palette[i].G, palette[i].B) });
                //}
                // items = new HashSet<T>(items).ToList();

                for (int i = 0; i < DMCitems.Count(); i++)
                {
                    items.Add(new ListItemColour() { Number = "  " + DMCitems[i].Floss, Name = "  " + DMCitems[i].Description, color = System.Windows.Media.Color.FromRgb((byte)DMCitems[i].Red, (byte)DMCitems[i].Green, (byte)DMCitems[i].Blue) });
                }

                listBox.ItemsSource = items;

                //changing the colours in the pattern to the DMC colours
                System.Drawing.Bitmap requantizedImage2 = new Bitmap(quantizer.ReduceColors(requantizedImage, palette));
                Bitmap newBitmap = new Bitmap(requantizedImage2);

                //putting it back into the image and the writable bitmap
                wBitmap = BitmapToImageSource(newBitmap);
                image.Source = wBitmap;

                //store state of image
                patternStatesAdd();

                //width = wBitmap.PixelWidth;
                //height = wBitmap.PixelHeight;

                //// New array of pixels to match the bitmap, one for each pixel
                //pixels = new System.Windows.Media.Color[height, width];
                //GetPixelsFromBitmap();
            }         
        }

        void patternStatesAdd()
        {
            if(currentIndex >= 0 && currentIndex != patternStates.Count() - 1)
            {
                int range = patternStates.Count() - (currentIndex + 1);

                patternStates.RemoveRange(currentIndex + 1, range);
            }
        
            //store state of image
            patternStates.Add(wBitmap);
            currentIndex++;
        }

        //convering bitmap to writeable bitmap
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

       


        //save image
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
        private void drawButtonImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            isEraser = false;
        }

        //clicking on the colour picker button
        private void Image_colorPicker(object sender, MouseButtonEventArgs e)
        {
            var initialColor = Colors.Blue;
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                //currentColour = dialog.Color;

                DMC closestColor = new DMC();
                double distance = 1000;

                for (int j = 0; j < DMCColors.Count(); j++)
                {
                    double d = ((dialog.Color.R - DMCColors[j].Red)) * ((dialog.Color.R - DMCColors[j].Red) )
                        + ((dialog.Color.G - DMCColors[j].Green) ) * ((dialog.Color.G - DMCColors[j].Green) )
                        + ((dialog.Color.B - DMCColors[j].Blue) ) * ((dialog.Color.B - DMCColors[j].Blue) );

                    if (d < distance)
                    {
                        closestColor = DMCColors[j];
                        distance = d;
                    }
                }

                currentColour = System.Windows.Media.Color.FromRgb((byte)closestColor.Red, (byte)closestColor.Green, (byte)closestColor.Blue);
               // currentDMCColour = closestColor;
            }

            isEraser = false;
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
                //nextColor = dialog.Color;

                DMC closestColor = new DMC();
                double distance = 1000;

                for (int j = 0; j < DMCColors.Count(); j++)
                {
                    double d = ((dialog.Color.R - DMCColors[j].Red) * 0.30) * ((dialog.Color.R - DMCColors[j].Red) * 0.30)
                        + ((dialog.Color.G - DMCColors[j].Green) * 0.59) * ((dialog.Color.G - DMCColors[j].Green) * 0.59)
                        + ((dialog.Color.B - DMCColors[j].Blue) * 0.11) * ((dialog.Color.B - DMCColors[j].Blue) * 0.11);

                    if (d < distance)
                    {
                        closestColor = DMCColors[j];
                        distance = d;
                    }
                }

                nextColor = System.Windows.Media.Color.FromRgb((byte)closestColor.Red, (byte)closestColor.Green, (byte)closestColor.Blue);

                //getting the selected listbox item
                ListItemColour selectedItem = (ListItemColour)listBox.SelectedItem;

                //getting current color in palette that will be changed
                existingColor = selectedItem.color;
                currentName = selectedItem.Name;              

                //making color variables for setting the pixels
                System.Drawing.Color previous = System.Drawing.Color.FromArgb(existingColor.R, existingColor.G, existingColor.B);
                System.Drawing.Color next = System.Drawing.Color.FromArgb(nextColor.R, nextColor.G, nextColor.B);
                //nextName = next.R.ToString("X2") + next.G.ToString("X2") + next.B.ToString("X2");

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

                //check that it is not an already existing color!!!        
                (listBox.SelectedItem as ListItemColour).color = nextColor;
                (listBox.SelectedItem as ListItemColour).Name = "  " + closestColor.Description;
                (listBox.SelectedItem as ListItemColour).Number = "  " + closestColor.Floss;


                wBitmap = BitmapToImageSource(img);
                image.Source = wBitmap;

                //store state of image
                patternStatesAdd();
            }
        }

        //clicking a colour in the palette - you can draw with that color
        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //getting the selected listbox item
            ListItemColour selectedItem = (ListItemColour)listBox.SelectedItem;

            //setting current color from the palette that was clicked
            currentColour = selectedItem.color;
        }

        //deleting a colour from the palette
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (items.Count > 1)
            {
                //getting the selected listbox item
                ListItemColour selectedItem = (ListItemColour)listBox.SelectedItem;

                //getting current color in palette that will be deleted
                System.Windows.Media.Color deleteColor = selectedItem.color;

                //remove selected colour from palette list
                for (int i = 0; i < items.Count(); i++)
                {
                    if (items[i].color == deleteColor)
                    {
                        items.RemoveAt(i);
                    }
                }

                System.Drawing.Color[] palette = new System.Drawing.Color[items.Count];

                //getting new palette
                for (int i = 0; i < items.Count(); i++)
                {
                    palette[i] = System.Drawing.Color.FromArgb(items[i].color.R, items[i].color.G, items[i].color.B);
                }


                //converting to bitmap
                MemoryStream outStream = new MemoryStream();
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(wBitmap));
                enc.Save(outStream);
                System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);

                //reduce colour palette
                //checking if there is more than 1 colour
                if (palette.Count() > 1)
                {
                    ColorImageQuantizer quantizer = new ColorImageQuantizer(new MedianCutQuantizer());
                    System.Drawing.Bitmap quantizedImage = quantizer.ReduceColors(img, palette);

                    Bitmap newBitmap = new Bitmap(quantizedImage);

                    //putting it back into the image and the writable bitmap
                    wBitmap = BitmapToImageSource(newBitmap);
                }
                else
                {
                    for (int i = 0; i < img.Width; i++)
                    {
                        for (int j = 0; j < img.Height; j++)
                        {
                            img.SetPixel(i, j, palette[0]);
                        }
                    }

                    wBitmap = BitmapToImageSource(img);
                }

                image.Source = wBitmap;

                //store state of image
                patternStatesAdd();
            }
            else
            {
                //cant delete all colours
                MessageBox.Show("Error: Cannot delete all colours from pattern.", "ERROR");
            }
        }


        private void eraserImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentColour = System.Windows.Media.Color.FromRgb(255, 255, 255);
            isEraser = true;
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            UndoFunction();
            //if(currentIndex > 0)
            //{
            //    currentIndex--;
            //    wBitmap = patternStates[currentIndex];
            //    image.Source = wBitmap;

               

            //    //store state of image
            //    //patternStatesAdd();

            //    BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);

            //    //converting to bitmap
            //    //MemoryStream outStream = new MemoryStream();
            //    //BitmapEncoder enc = new BmpBitmapEncoder();
            //    //enc.Frames.Add(BitmapFrame.Create(wBitmap));
            //    //enc.Save(outStream);
            //    //System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);


            //    //ColorImageQuantizer quantizer = new ColorImageQuantizer(new MedianCutQuantizer());
            //    //palette = quantizer.CalculatePalette(img, myPalette.Colors.Count());

            //    items.Clear();

            //    //making listbox items dynamically
            //    for (int i = 0; i < myPalette.Colors.Count(); i++)
            //    {
            //        if (!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
            //            items.Add(new ListItemColour() { Name = "   " + myPalette.Colors[i].ToString().ToLower(), color = myPalette.Colors[i] });
            //    }

                
            //}

        }

        void UndoFunction()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                wBitmap = patternStates[currentIndex];
                image.Source = wBitmap;

                BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);
                //items.Clear();

                ////making listbox items dynamically
                //for (int i = 0; i < myPalette.Colors.Count(); i++)
                //{
                //    if (!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
                //        items.Add(new ListItemColour() { Name = "   " + myPalette.Colors[i].ToString().ToLower(), color = myPalette.Colors[i] });
                //}

                DMC closestColor = new DMC();
                double distance = 1000;
                List<System.Drawing.Color> paletteList = new List<System.Drawing.Color>();
                DMCitems.Clear();

                //getting closest DMC colours to RGB
                for (int i = 0; i < myPalette.Colors.Count(); i++)
                {
                    for (int j = 0; j < DMCColors.Count(); j++)
                    {
                        double d = ((myPalette.Colors[i].R - DMCColors[j].Red) * 0.30) * ((myPalette.Colors[i].R - DMCColors[j].Red) * 0.30)
                            + ((myPalette.Colors[i].G - DMCColors[j].Green) * 0.59) * ((myPalette.Colors[i].G - DMCColors[j].Green) * 0.59)
                            + ((myPalette.Colors[i].B - DMCColors[j].Blue) * 0.11) * ((myPalette.Colors[i].B - DMCColors[j].Blue) * 0.11);

                        if (d < distance)
                        {
                            closestColor = DMCColors[j];
                            distance = d;
                        }
                    }
                    DMCitems.Add(closestColor);
                    distance = 1000;
                    paletteList.Add(System.Drawing.Color.FromArgb(closestColor.Red, closestColor.Green, closestColor.Blue));
                }

                //making the list into a simple array so that it can be passed to the quantizer
                palette = paletteList.ToArray();

                items.Clear();

                for (int i = 0; i < DMCitems.Count(); i++)
                {
                    if (!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
                        items.Add(new ListItemColour() { Number = "  " + DMCitems[i].Floss, Name = "  " + DMCitems[i].Description, color = System.Windows.Media.Color.FromRgb((byte)DMCitems[i].Red, (byte)DMCitems[i].Green, (byte)DMCitems[i].Blue) });
                }
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            //if(currentIndex != patternStates.Count() -1)
            //{
            //    currentIndex++;
            //    wBitmap = patternStates[currentIndex];
            //    image.Source = wBitmap;



            //    BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);

            //    items.Clear();

            //    //making listbox items dynamically
            //    for (int i = 0; i < myPalette.Colors.Count(); i++)
            //    {
            //        if (!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
            //            items.Add(new ListItemColour() { Name = "   " + myPalette.Colors[i].ToString().ToLower(), color = myPalette.Colors[i] });
            //    }
            //} 
            RedoFunction();
        }

        void RedoFunction()
        {
            if (currentIndex != patternStates.Count() - 1)
            {
                currentIndex++;
                wBitmap = patternStates[currentIndex];
                image.Source = wBitmap;



                BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);

                //items.Clear();

                ////making listbox items dynamically
                //for (int i = 0; i < myPalette.Colors.Count(); i++)
                //{
                //    if (!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
                //        items.Add(new ListItemColour() { Name = "   " + myPalette.Colors[i].ToString().ToLower(), color = myPalette.Colors[i] });
                //}

                DMC closestColor = new DMC();
                double distance = 1000;
                List<System.Drawing.Color> paletteList = new List<System.Drawing.Color>();
                DMCitems.Clear();

                //getting closest DMC colours to RGB
                for (int i = 0; i < myPalette.Colors.Count(); i++)
                {
                    for (int j = 0; j < DMCColors.Count(); j++)
                    {
                        double d = ((myPalette.Colors[i].R - DMCColors[j].Red) * 0.30) * ((myPalette.Colors[i].R - DMCColors[j].Red) * 0.30)
                            + ((myPalette.Colors[i].G - DMCColors[j].Green) * 0.59) * ((myPalette.Colors[i].G - DMCColors[j].Green) * 0.59)
                            + ((myPalette.Colors[i].B - DMCColors[j].Blue) * 0.11) * ((myPalette.Colors[i].B - DMCColors[j].Blue) * 0.11);

                        if (d < distance)
                        {
                            closestColor = DMCColors[j];
                            distance = d;
                        }
                    }
                    DMCitems.Add(closestColor);
                    distance = 1000;
                    paletteList.Add(System.Drawing.Color.FromArgb(closestColor.Red, closestColor.Green, closestColor.Blue));
                }

                //making the list into a simple array so that it can be passed to the quantizer
                palette = paletteList.ToArray();

                items.Clear();

                for (int i = 0; i < DMCitems.Count(); i++)
                {
                    if (!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
                        items.Add(new ListItemColour() { Number = "  " + DMCitems[i].Floss, Name = "  " + DMCitems[i].Description, color = System.Windows.Media.Color.FromRgb((byte)DMCitems[i].Red, (byte)DMCitems[i].Green, (byte)DMCitems[i].Blue) });
                }
            }
        }


        //when ctrl + another key is pressed, event handler
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl)
            {
                switch (e.Key)
                {
                    case Key.Y: RedoFunction(); break;
                    case Key.Z: UndoFunction(); break;
                    default: break;
                }
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

            wBitmap = BitmapToImageSource(img);
            image.Source = wBitmap;

            //store state of image
            patternStatesAdd();

            if (!isEraser)
            {
                //adding new color to palette
                //string nextName = "   #" + "ff" + (bitmapColour.R.ToString("X2") + bitmapColour.G.ToString("X2") + bitmapColour.B.ToString("X2")).ToLower();

                DMC closestColor = new DMC();
                double distance = 1000;

                //getting closest DMC colours to RGB
               
                
                for (int j = 0; j < DMCColors.Count(); j++)
                {
                    double d = ((currentColour.R - DMCColors[j].Red) * 0.30) * ((currentColour.R - DMCColors[j].Red) * 0.30)
                        + ((currentColour.G - DMCColors[j].Green) * 0.59) * ((currentColour.G - DMCColors[j].Green) * 0.59)
                        + ((currentColour.B - DMCColors[j].Blue) * 0.11) * ((currentColour.B - DMCColors[j].Blue) * 0.11);

                    if (d < distance)
                    {
                        closestColor = DMCColors[j];
                        distance = d;
                    }
                }

                string nextName = "  " + closestColor.Description;


                int count = 0;
                for (int i = 0; i < items.Count(); i++)
                {
                    if (items[i].Name == nextName)
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    items.Add(new ListItemColour() { Number = "  " + closestColor.Floss, Name = nextName, color = System.Windows.Media.Color.FromRgb((byte)closestColor.Red, (byte)closestColor.Green, (byte)closestColor.Blue) });
                }
            }
            else
            {
                BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);

                //items.Clear();

                ////making listbox items dynamically
                //for (int i = 0; i < myPalette.Colors.Count(); i++)
                //{
                //    if(!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
                //    items.Add(new ListItemColour() { Name = "   " + myPalette.Colors[i].ToString().ToLower(), color = myPalette.Colors[i] });
                //}


                DMC closestColor = new DMC();
                double distance = 1000;
                List<System.Drawing.Color> paletteList = new List<System.Drawing.Color>();
                DMCitems.Clear();

                //getting closest DMC colours to RGB
                for (int i = 0; i < myPalette.Colors.Count(); i++)
                {
                    for (int j = 0; j < DMCColors.Count(); j++)
                    {
                        double d = ((myPalette.Colors[i].R - DMCColors[j].Red) * 0.30) * ((myPalette.Colors[i].R - DMCColors[j].Red) * 0.30)
                            + ((myPalette.Colors[i].G - DMCColors[j].Green) * 0.59) * ((myPalette.Colors[i].G - DMCColors[j].Green) * 0.59)
                            + ((myPalette.Colors[i].B - DMCColors[j].Blue) * 0.11) * ((myPalette.Colors[i].B - DMCColors[j].Blue) * 0.11);

                        if (d < distance)
                        {
                            closestColor = DMCColors[j];
                            distance = d;
                        }
                    }
                    DMCitems.Add(closestColor);
                    distance = 1000;
                    paletteList.Add(System.Drawing.Color.FromArgb(closestColor.Red, closestColor.Green, closestColor.Blue));
                }

                items.Clear();

                for (int i = 0; i < DMCitems.Count(); i++)
                {
                    if (!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
                        items.Add(new ListItemColour() { Number = "  " + DMCitems[i].Floss, Name = "  " + DMCitems[i].Description, color = System.Windows.Media.Color.FromRgb((byte)DMCitems[i].Red, (byte)DMCitems[i].Green, (byte)DMCitems[i].Blue) });
                }

            }

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
        //private void UpdatePixelsBitmap()
        //{
        //    // Copy the data into a one-dimensional array.
        //    byte[] pixels1d = new byte[height * width * 4];
        //    int index = 0;
        //    for (int row = 0; row < height; row++)
        //    {
        //        for (int col = 0; col < width; col++)
        //        {
        //            pixels1d[index++] = pixels[row, col].R;
        //            pixels1d[index++] = pixels[row, col].G;
        //            pixels1d[index++] = pixels[row, col].B;
        //            pixels1d[index++] = pixels[row, col].A;
        //        }
        //    }

        //    // Copy pixels over to writeable bitmap
        //    Int32Rect rect = new Int32Rect(0, 0, width, height);
        //    int stride = 4 * width;
        //    wBitmap.WritePixels(rect, pixels1d, stride, 0);
        //}


        // Get pixels array from writable bitmap (opposite of above method), used when we load an image into a bitmap
        //private void GetPixelsFromBitmap()
        //{
        //    // One dimensional array to get pixel data
        //    byte[] pixels1d = new byte[height * width * 4];

        //    // Copy pixels from writeable bitmap
        //    Int32Rect rect = new Int32Rect(0, 0, width, height);
        //    int stride = 4 * width;
        //    wBitmap.CopyPixels(rect, pixels1d, stride, 0);

        //    // Copy the data from one-dimensional array into our pixels array
        //    int index = 0;
        //    for (int row = 0; row < height; row++)
        //    {
        //        for (int col = 0; col < width; col++)
        //        {
        //            pixels[row, col].R = pixels1d[index++];
        //            pixels[row, col].G = pixels1d[index++];
        //            pixels[row, col].B = pixels1d[index++];
        //            pixels[row, col].A = pixels1d[index++];
        //        }
        //    }
        //}


        //private void ChangeColour_Click(object sender, RoutedEventArgs e)
        //{
        //    System.Drawing.Color red = System.Drawing.Color.FromArgb(255, 0, 0);
        //    System.Drawing.Color white = System.Drawing.Color.FromArgb(255, 255, 255);


        //    MemoryStream outStream = new MemoryStream();

        //    BitmapEncoder enc = new BmpBitmapEncoder();
        //    enc.Frames.Add(BitmapFrame.Create(wBitmap));
        //    enc.Save(outStream);
        //    System.Drawing.Bitmap img = new System.Drawing.Bitmap(outStream);



        //    for (int i = 0; i < img.Width; i++)
        //    {
        //        for (int j = 0; j < img.Height; j++)
        //        {
        //            System.Drawing.Color pixelColor = img.GetPixel(i, j);
        //            if (pixelColor == white)
        //                img.SetPixel(i, j, red);
        //        }
        //    }

        //    wBitmap = BitmapToImageSource(img);
        //    image.Source = wBitmap;

        //}


        ////data binding
        ////making listbox items dynamically
        //for (int i = 0; i < palette.Count(); i++)
        //{[]
        //    items.Add(new ListItemColour() { Name = "   " + palette[i].Name, color = System.Windows.Media.Color.FromRgb(palette[i].R, palette[i].G, palette[i].B) });
        //}




        //remove colour

        //    bool colourExist = false;
        //    List<ListItemColour> colourNames = new List<ListItemColour>();


        //    for (int k = 0; k < items.Count(); k++)
        //    {
        //        for (int i = 0; i < img.Width; i++)
        //        {
        //            for (int j = 0; j < img.Height; j++)
        //            {
        //                System.Drawing.Color pixelColor = img.GetPixel(i, j);                    

        //                System.Drawing.Color paletteColour = System.Drawing.Color.FromArgb(items[k].color.R, items[k].color.G, items[k].color.B);

        //                if (pixelColor.ToArgb() == paletteColour.ToArgb())
        //                {
        //                    colourExist = true;
        //                }
        //            }
        //        }

        //        //if a colour was removed by undoing something, then it is removed from the palette too
        //        if (colourExist == false)
        //        {
        //            colourNames.Add(items[k]);
        //        }

        //        colourExist = false;
        //    }

        //    for (int i = 0; i < colourNames.Count(); i++)
        //    {
        //        if (items.Contains(colourNames[i]))
        //        {
        //            items.Remove(colourNames[i]);
        //        }
        //    }

        // color brightness as perceived:
        //float getBrightness(System.Windows.Media.Color c)
        //{ return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f; }

        //// distance between two hues:
        //float getHueDistance(float hue1, float hue2)
        //{
        //    float d = Math.Abs(hue1 - hue2); return d > 180 ? 360 - d : d;
        //}

        ////  weighed only by saturation and brightness (from my trackbars)
        //float ColorNum(System.Drawing.Color c)
        //{
        //    return c.GetSaturation() * factorSat +
        //                getBrightness(c) * factorBri;
        //}

        //// distance in RGB space
        //int ColorDiff(System.Windows.Media.Color c1, System.Windows.Media.Color c2)
        //{
        //    return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
        //                           + (c1.G - c2.G) * (c1.G - c2.G)
        //                           + (c1.B - c2.B) * (c1.B - c2.B));
        //}
    }
}
