using Dsafa.WpfColorPicker;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using AForge.Imaging.ColorReduction;
using System.Collections.ObjectModel;
using CsvHelper;
using System.Globalization;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;

namespace Flow_Stitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    //public class DMC
    //{
    //    public string Floss { get; set; }
    //    public string Description { get; set; }
    //    public int Red { get; set; }
    //    public int Green { get; set; }
    //    public int Blue { get; set; }

    //}

    //class for the listbox item
    //public class ListItemColour : INotifyPropertyChanged
    //{
    //    public string _Name { get; set; } //description

    //    public string _Number { get; set; } //floss
    //    public System.Windows.Media.Color _Color { get; set; } //rgb
    //    // Declare the event
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    public ListItemColour()
    //    {
    //    }        

    //    public System.Windows.Media.Color color
    //    {
    //        get { return _Color; }
    //        set
    //        {
    //            _Color = value;
    //            // Call OnPropertyChanged whenever the property is updated
    //            OnPropertyChanged();
    //        }
    //    }
    //    public string Name
    //    {
    //        get { return _Name; }
    //        set
    //        {
    //            _Name = value;
    //            // Call OnPropertyChanged whenever the property is updated
    //            OnPropertyChanged();
    //        }
    //    }

    //    public string Number
    //    {
    //        get { return _Number; }
    //        set
    //        {
    //            _Number = value;
    //            // Call OnPropertyChanged whenever the property is updated
    //            OnPropertyChanged();
    //        }
    //    }

    //    // Create the OnPropertyChanged method to raise the event
    //    // The calling member's name will be used as the parameter.
    //    protected void OnPropertyChanged([CallerMemberName] string name = null)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    //    }
    //}



    public partial class MainWindow : Window
    {
        
        //stores the image 
        WriteableBitmap wBitmap;

        bool isDrawing = false;
        bool isEraser = false;
        System.Windows.Media.Color currentColour = System.Windows.Media.Color.FromRgb(0, 0, 0); //stores the color that is currently used
        System.Drawing.Color[] palette; //stores the colours in the pattern
        ObservableCollection<ListItemColour> items = new ObservableCollection<ListItemColour>(); //stores listbox items

        List<DMC> DMCitems = new List<DMC>(); //stores lisbox items, but DMC colors
        ObservableCollection<ListItemColour> DMCColoursList = new ObservableCollection<ListItemColour>(); //stores all DMC colors but as ListItemColours
        List<DMC> DMCColors = new List<DMC>(); //stores all DMC colours

        ////stores image for undo and redo functionality
        //List<WriteableBitmap> patternStates = new List<WriteableBitmap>();
        /*int currentIndex = -1;*/ //stores which image stored is the current state
       
        float upscalePercentage; //stores how much to upsacle image to go back to original size
        int patternWidth; //width of pattern in stitches
        int patternHeight; //height of pattern in stitches

        //number of colours in the pattern
        public string numberColours 
        {
            get { return (string)GetValue(numberColoursProperty); }
            set { SetValue(numberColoursProperty, value); }
        }

        public static readonly DependencyProperty numberColoursProperty =
            DependencyProperty.Register("numberColours", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        private Pattern Pattern = new Pattern();
        private Utilities utilities = new Utilities();

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
        }

        
        //loads image
        private void ItemOpen_Click(object sender, RoutedEventArgs e)
        {
            //saves input from new picture window
            int numberOfColours = 0;
            int heightOfPattern = 0;

            //opening new picture window for setup
            NewPicture window = new NewPicture();
            Nullable<bool> result2 = window.ShowDialog();

            //parsing input strings
            if (!(int.TryParse(window.ColourtextBox.Text, out numberOfColours)))
            {
                numberOfColours = 0;
            }
            if (!(int.TryParse(window.HeighttextBox.Text, out heightOfPattern)))
            {
                heightOfPattern = 0;
            }

            //only opening a picture if input is not empty/invalid and if OK was clicked
            if (numberOfColours != 0 && heightOfPattern != 0 && result2 == true && numberOfColours <= 256 && numberOfColours >= 2)
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

                //if OK is pressed
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
                        wBitmap = new WriteableBitmap(bitmap);
                        image.Source = wBitmap; //loaded in picture is stored in image now
                    }
                    //if it isn't an image it displays an error message, the program doens't crash.
                    else
                    {
                        MessageBox.Show("Error: Select a file with .jpg or .png extension.", "ERROR");
                    }
                }

                //Pattern = ImageProcessor.OpenImage(ref wBitmap);
                ////outputting width to properties
                //WidthTextBlock.Text = " Width: " + scaledImage.Width.ToString();

                // resetting the undo states
                //patternStates.Clear();
                Pattern.Clear();
                //currentIndex = -1;

                //making sure a picture was loaded in before doing operations
                if (wBitmap != null)
                {
                    //convert to bitmap
                    System.Drawing.Bitmap img = utilities.ConvertToBitmap(wBitmap);

                    if (img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb || img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb || img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb || img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                    {

                        //calculating new size in percentage
                        float newSizePercentage = (float)heightOfPattern / (float)img.Height;
                        newSizePercentage *= 100;

                        //???
                        upscalePercentage = 100 * ((float)img.Height / (float)heightOfPattern);

                        //reduce colour palette
                        ColorImageQuantizer quantizer = new ColorImageQuantizer(new MedianCutQuantizer());
                        //var quantizer2 = new WuQuantizer();
                        System.Drawing.Bitmap quantizedImage = quantizer.ReduceColors(img, numberOfColours);


                        //resize image
                        System.Drawing.Bitmap scaledImage = Utilities.ScaleByPercent(quantizedImage, newSizePercentage, heightOfPattern);
                        //storing height
                        patternHeight = heightOfPattern;
                        //outputting width to properties
                        WidthTextBlock.Text = " Width: " + scaledImage.Width.ToString();
                        //requantize
                        System.Drawing.Bitmap requantizedImage = new Bitmap(quantizer.ReduceColors(scaledImage, numberOfColours)); //original

                        wBitmap = utilities.BitmapToImageSource(requantizedImage);
                        image.Source = wBitmap;

                        //getting color palette of quantized image
                        BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);

                        DMC closestColor = new DMC();
                        double distance = 1000;
                        List<System.Drawing.Color> paletteList = new List<System.Drawing.Color>();
                        DMCitems.Clear();
                        List<DMC> DMCitemsDup = new List<DMC>();
                        DMCitemsDup.Clear();

                        //getting closest DMC colours to RGB
                        for (int i = 0; i < myPalette.Colors.Count(); i++)
                        {
                            for (int j = 0; j < DMCColors.Count(); j++)
                            {

                                DMCColoursList.Add(new ListItemColour() { Number = "  " + DMCColors[j].Floss, Name = "  " + DMCColors[j].Description, color = System.Windows.Media.Color.FromRgb((byte)DMCColors[j].Red, (byte)DMCColors[j].Green, (byte)DMCColors[j].Blue) });

                                //getting a closer blue colour because of lack of bright blue threads
                                var b = myPalette.Colors[i].B;
                                if (myPalette.Colors[i].B > 180 && myPalette.Colors[i].G < 100 && myPalette.Colors[i].R < 100)
                                {
                                    b = (byte)(160 + (20 * (b - 160)) / (256 - 160));
                                }

                                //finding closest colour to image in DMC threads using Lab colour space
                                var dialogRgb = new Rgb { R = myPalette.Colors[i].R, G = myPalette.Colors[i].G, B = b };
                                var lab1 = dialogRgb.To<Lab>();
                                var lch1 = lab1.To<Lch>();

                                var DMCRgb = new Rgb { R = DMCColors[j].Red, G = DMCColors[j].Green, B = DMCColors[j].Blue };
                                var lab2 = DMCRgb.To<Lab>();
                                var lch2 = lab2.To<Lch>();

                                var comparison = new CmcComparison();
                                var deltaE = comparison.Compare(dialogRgb, DMCRgb);

                                //finding the smallest distance 
                                if (deltaE < distance)
                                {
                                    closestColor = DMCColors[j];
                                    //distance = d;
                                    distance = deltaE;
                                }
                            }
                            DMCitemsDup.Add(closestColor);
                            distance = 1000;
                            paletteList.Add(System.Drawing.Color.FromArgb(closestColor.Red, closestColor.Green, closestColor.Blue));
                        }


                        //removing duplicates
                        DMCitems = DMCitemsDup.Distinct().ToList();
                        List<System.Drawing.Color> uniquePL = paletteList.Distinct().ToList();

                        //making the list into a simple array so that it can be passed to the quantizer
                        palette = uniquePL.ToArray();

                        //resetting palette 
                        items.Clear();

                        //data binding
                        //making listbox items dynamically
                        for (int i = 0; i < DMCitems.Count(); i++)
                        {
                            items.Add(new ListItemColour() { Number = "  " + DMCitems[i].Floss, Name = "  " + DMCitems[i].Description, color = System.Windows.Media.Color.FromRgb((byte)DMCitems[i].Red, (byte)DMCitems[i].Green, (byte)DMCitems[i].Blue) });
                        }

                        listBox.ItemsSource = items;

                        //changing the colours in the pattern to the DMC colours
                        System.Drawing.Bitmap requantizedImage2 = new Bitmap(quantizer.ReduceColors(requantizedImage, palette)); //CHANGE BACK
                        Bitmap newBitmap = new Bitmap(requantizedImage2);

                        //putting it back into the image and the writable bitmap
                        wBitmap = utilities.BitmapToImageSource(newBitmap);
                        image.Source = wBitmap;

                        //ThresholdEffect effect = new ThresholdEffect();
                        //System.Windows.Media.Color inputColor = new System.Windows.Media.Color();
                        ////inputColor = System.Windows.Media.Color.FromRgb(1, 0, 0);
                       
                        //effect.BlankColor = System.Windows.Media.Color.FromArgb(255, 0, 255, 0); 
                        //image.Effect = effect;

                        //store state of image
                        //patternStatesAdd();
                        Pattern.patternStatesAdd(ref wBitmap);

                        //patternHeight = (int)wBitmap.Height;
                        patternWidth = (int)image.Source.Width;

                        //displaying height of pattern in properties
                        HeightTextBlock.Text = " Height: " + heightOfPattern.ToString();

                        //bindig number of colours
                        this.DataContext = this;
                        this.numberColours = items.Count().ToString();
                    }
                    else
                    {
                        image.Source = null;
                        MessageBox.Show("Error: Select a file with .24 or 32 bit depth.", "ERROR");
                    }

                }//if wbitmap not null
            }
            else
            {
                if (result2 == true)
                {
                    if (numberOfColours == 0 || heightOfPattern == 0)
                        MessageBox.Show("Error: Please input a value.", "ERROR");
                    else if (numberOfColours < 2 || numberOfColours > 256)
                        MessageBox.Show("Error: Input a number between 2 and 256 for Number of colours.", "ERROR");
                    else if (heightOfPattern < 1)
                        MessageBox.Show("Error: Input a value bigger than 0 for Height of pattern.", "ERROR");
                }
            }
        }

        //helper function for undo/redo
        //void patternStatesAdd()
        //{
            
        //    if(currentIndex >= 0 && currentIndex != patternStates.Count() - 1)
        //    {
        //        int range = patternStates.Count() - (currentIndex + 1);

        //        patternStates.RemoveRange(currentIndex + 1, range);
        //    }
        
        //    //store state of image
        //    patternStates.Add(wBitmap);
        //    currentIndex++;
        //}

        //converting bitmap to writeable bitmap
        //WriteableBitmap BitmapToImageSource(Bitmap bitmap)
        //{
        //    using (MemoryStream memory = new MemoryStream())
        //    {
        //        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        //        memory.Position = 0;
        //        BitmapImage bitmapimage = new BitmapImage();
        //        bitmapimage.BeginInit();
        //        bitmapimage.StreamSource = memory;
        //        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmapimage.EndInit();

        //        return new WriteableBitmap(bitmapimage);
        //    }
        //}

        //WriteableBitmap BitmapToImageSourcePng(Bitmap bitmap)
        //{
        //    using (MemoryStream memory = new MemoryStream())
        //    {
        //        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        //        memory.Position = 0;
        //        BitmapImage bitmapimage = new BitmapImage();
        //        bitmapimage.BeginInit();
        //        bitmapimage.StreamSource = memory;
        //        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmapimage.EndInit();

        //        return new WriteableBitmap(bitmapimage);
        //    }
        //}


       
        //save image
        private void ItemSave_Click(object sender, RoutedEventArgs e)
        {
            if(wBitmap != null)
            {  
                //convert to bitmap
                System.Drawing.Bitmap img = utilities.ConvertToBitmap(wBitmap);

                //upscaling image
                System.Drawing.Bitmap scaledImage = Utilities.ScaleByPercentUp(img, upscalePercentage*2);

                //save
                utilities.Save(utilities.BitmapToImageSource(scaledImage));

                //Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                //dlg.FileName = "Document";
                //dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
                //Nullable<bool> result = dlg.ShowDialog();
                //string fileName = "";

                //if (result == true)
                //{
                //    fileName = dlg.FileName;
                //    PngBitmapEncoder jpg = new PngBitmapEncoder(); //pngbit
                //    jpg.Frames.Add(BitmapFrame.Create(utilities.BitmapToImageSource(scaledImage)));
                //    using (Stream stm = File.Create(fileName))
                //    {
                //        jpg.Save(stm);
                //    }
                //}
            }        
        }


        ////scaling down image
        //static System.Drawing.Bitmap ScaleByPercent(System.Drawing.Image imgPhoto, float Percent, int Height)
        //{
        //    float nPercent = ((float)Percent / 100);

        //    int sourceWidth = imgPhoto.Width;
        //    int sourceHeight = imgPhoto.Height;
        //    int sourceX = 0;
        //    int sourceY = 0;

        //    int destX = 0;
        //    int destY = 0;
        //    int destWidth = (int)(sourceWidth * nPercent);
        //    int destHeight = Height;
            
        //    Bitmap bmPhoto = new Bitmap(destWidth, destHeight,
        //                             System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        //    bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
        //                            imgPhoto.VerticalResolution);

        //    Graphics grPhoto = Graphics.FromImage(bmPhoto);
        //    grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
           

        //    using (ImageAttributes wrapMode = new ImageAttributes())
        //    {
        //        wrapMode.SetWrapMode(WrapMode.TileFlipXY);

        //        grPhoto.DrawImage(imgPhoto,
        //            new System.Drawing.Rectangle(destX, destY, destWidth, destHeight),
        //            sourceX, sourceY, sourceWidth, sourceHeight,
        //            GraphicsUnit.Pixel, wrapMode);
        //    }

        //    grPhoto.Dispose();
        //    return bmPhoto;
        //}


        ////scaling up image
        //static System.Drawing.Bitmap ScaleByPercentUp(System.Drawing.Image imgPhoto, float Percent)
        //{
        //    float nPercent = ((float)Percent / 100);

        //    int sourceWidth = imgPhoto.Width;
        //    int sourceHeight = imgPhoto.Height;
        //    int sourceX = 0;
        //    int sourceY = 0;

        //    int destX = 0;
        //    int destY = 0;
        //    int destWidth = (int)(sourceWidth * nPercent);
        //    int destHeight = (int)(sourceHeight * nPercent);

        //    Bitmap bmPhoto = new Bitmap(destWidth, destHeight,
        //                             System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        //    bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
        //                            imgPhoto.VerticalResolution);

        //    Graphics grPhoto = Graphics.FromImage(bmPhoto);
        //    grPhoto.InterpolationMode = InterpolationMode.NearestNeighbor;


        //    using (ImageAttributes wrapMode = new ImageAttributes())
        //    {
        //        wrapMode.SetWrapMode(WrapMode.TileFlipXY);

        //        grPhoto.DrawImage(imgPhoto,
        //            new System.Drawing.Rectangle(destX, destY, destWidth, destHeight),
        //            sourceX, sourceY, sourceWidth, sourceHeight,
        //            GraphicsUnit.Pixel, wrapMode);
        //    }

        //    grPhoto.Dispose();
        //    return bmPhoto;
        //}

        //clicking on the draw button
        private void drawButtonImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            isEraser = false;

            //opening window to choose from DMC colours
            DMCWindow window = new DMCWindow(DMCColoursList);
            window.ShowDialog();

            //setting the drawing colour to the selected one
            if(window.currentColourW != null)
            currentColour = window.currentColourW;
        }

        //clicking on the colour picker button
        private void Image_colorPicker(object sender, MouseButtonEventArgs e)
        {
            //opening colour picker dialog
            var initialColor = Colors.Blue;
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {

                DMC closestColor = new DMC();
                double distance = 1000;

                //getting closest DMC coolour to selected colour
                for (int j = 0; j < DMCColors.Count(); j++)
                {

                    //adjusting blue
                    var b = dialog.Color.B;
                    if (dialog.Color.B > 180 && dialog.Color.G < 100 && dialog.Color.R < 100)
                    {
                        b = (byte)(160 + (20*(b -160))/ (256 - 160));
                    }

                    //using Lab space
                    var dialogRgb = new Rgb { R = dialog.Color.R, G = dialog.Color.G, B = b };
                    var lab1 = dialogRgb.To<Lab>();
                    var lch1 = lab1.To<Lch>();

                    var DMCRgb = new Rgb { R = DMCColors[j].Red, G = DMCColors[j].Green, B = DMCColors[j].Blue };
                    var lab2 = DMCRgb.To<Lab>();
                    var lch2 = lab2.To<Lch>();

                   // var deltaE = lch1.Compare(lch2, new CmcComparison(lightness: 2, chroma: 1));
                    var comparison = new CmcComparison();
                    var deltaE = comparison.Compare(dialogRgb, DMCRgb);

                    //finding smallest distance
                    if (deltaE < distance)
                    {
                        closestColor = DMCColors[j];
                        // distance = d;
                        distance = deltaE;
                    }
                }

                //setting drawing colour to the chosen one
                currentColour = System.Windows.Media.Color.FromRgb((byte)closestColor.Red, (byte)closestColor.Green, (byte)closestColor.Blue);
            }

            isEraser = false;
        }


        //change existing palette colour
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Color existingColor; //stores the colour that is changed
            System.Windows.Media.Color nextColor; //stored the colour that it is changed to
            string currentName; //name of the changed colour
            string nextName;

            //open colour picker
            var initialColor = Colors.Blue;
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();

            //getting closest DMC colour to selected colour
            if (result.HasValue && result.Value)
            {

                DMC closestColor = new DMC();
                double distance = 1000;

                //getting closest DMC colour to the selected colour
                for (int j = 0; j < DMCColors.Count(); j++)
                {    

                    //adjusting blue
                    var b = dialog.Color.B;
                    if (dialog.Color.B > 180 && dialog.Color.G < 100 && dialog.Color.R < 100)
                    {
                        b = (byte)(160 + (20 * (b - 160)) / (256 - 160));
                    }

                    //using Lab space
                    var dialogRgb = new Rgb { R = dialog.Color.R, G = dialog.Color.G, B = b };
                    var lab1 = dialogRgb.To<Lab>();
                    var lch1 = lab1.To<Lch>();

                    var DMCRgb = new Rgb { R = DMCColors[j].Red, G = DMCColors[j].Green, B = DMCColors[j].Blue };
                    var lab2 = DMCRgb.To<Lab>();
                    var lch2 = lab2.To<Lch>();

                    // var deltaE = lch1.Compare(lch2, new CmcComparison(lightness: 2, chroma: 1));
                    var comparison = new CmcComparison();
                    var deltaE = comparison.Compare(dialogRgb, DMCRgb);


                    //finding smallest distance
                    if (deltaE < distance)
                    {
                        closestColor = DMCColors[j];
                        // distance = d;
                        distance = deltaE;
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


                //converting to bitmap
                System.Drawing.Bitmap img = utilities.ConvertToBitmap(wBitmap);

                //changing all pixels from previous colour
                for (int i = 0; i < img.Width; i++)
                {
                    for (int j = 0; j < img.Height; j++)
                    {
                        System.Drawing.Color pixelColor = img.GetPixel(i, j);
                        if (pixelColor == previous)
                            img.SetPixel(i, j, next);
                    }
                }


                nextName = "  " + closestColor.Description;

                //checking if that colour is already in the palette
                int count = 0;
                for (int i = 0; i < items.Count(); i++)
                {
                    if (items[i].Name == nextName)
                    {
                        count++;
                    }
                }

                //if the colour is not in the palette, then it is changed
                if (count == 0)
                {      
                    (listBox.SelectedItem as ListItemColour).color = nextColor;
                    (listBox.SelectedItem as ListItemColour).Name = "  " + closestColor.Description;
                    (listBox.SelectedItem as ListItemColour).Number = "  " + closestColor.Floss;
                }
                //if it is already in the palette, then it is deleted
                else
                {
                    items.Remove(listBox.SelectedItem as ListItemColour);
                    //number of colours updated
                    this.DataContext = this;
                    this.numberColours = items.Count().ToString();
                }

                wBitmap = utilities.BitmapToImageSource(img);
                image.Source = wBitmap;

                //store state of image
                Pattern.patternStatesAdd(ref wBitmap);
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

                //binding number of colours
                this.DataContext = this;
                this.numberColours = items.Count().ToString();

                //getting palette of image
                System.Drawing.Color[] palette = new System.Drawing.Color[items.Count];

                //making new palette
                for (int i = 0; i < items.Count(); i++)
                {
                    palette[i] = System.Drawing.Color.FromArgb(items[i].color.R, items[i].color.G, items[i].color.B);
                }


                //converting to bitmap
                System.Drawing.Bitmap img = utilities.ConvertToBitmap(wBitmap);

                //reduce colour palette
                //checking if there is more than 1 colour
                if (palette.Count() > 1)
                {
                    ColorImageQuantizer quantizer = new ColorImageQuantizer(new MedianCutQuantizer());
                    System.Drawing.Bitmap quantizedImage = quantizer.ReduceColors(img, palette);

                    Bitmap newBitmap = new Bitmap(quantizedImage);

                    //putting it back into the image and the writable bitmap
                    wBitmap = utilities.BitmapToImageSource(newBitmap);
                }
                //setting all the image to one colour
                else
                {
                    for (int i = 0; i < img.Width; i++)
                    {
                        for (int j = 0; j < img.Height; j++)
                        {
                            img.SetPixel(i, j, palette[0]);
                        }
                    }

                    wBitmap = utilities.BitmapToImageSource(img);
                }

                image.Source = wBitmap;

                //store state of image
                Pattern.patternStatesAdd(ref wBitmap);
            }
            else
            {
                //cant delete all colours
                MessageBox.Show("Error: Cannot delete all colours from pattern.", "ERROR");
            }
        }

        //selecting eraser tool
        private void eraserImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //currect colour is white
            currentColour = System.Windows.Media.Color.FromRgb(255, 255, 255);
            isEraser = true;
        }

        //undo operation
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            UndoFunction();
        }

        void UndoFunction()
        {
            if (Pattern.hasPreviousState())
            {
                //currentIndex--;
                wBitmap = Pattern.GetPreviousState();//patternStates[currentIndex];
                image.Source = wBitmap;

                BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);

                DMC closestColor = new DMC();
                double distance = 1000;
                //List<System.Drawing.Color> paletteList = new List<System.Drawing.Color>();
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
                    //paletteList.Add(System.Drawing.Color.FromArgb(closestColor.Red, closestColor.Green, closestColor.Blue));
                }

                //making the list into a simple array so that it can be passed to the quantizer
               // palette = paletteList.ToArray();

                items.Clear();

                //remaking palette
                for (int i = 0; i < DMCitems.Count(); i++)
                {
                    if (!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
                        items.Add(new ListItemColour() { Number = "  " + DMCitems[i].Floss, Name = "  " + DMCitems[i].Description, color = System.Windows.Media.Color.FromRgb((byte)DMCitems[i].Red, (byte)DMCitems[i].Green, (byte)DMCitems[i].Blue) });
                }
                this.DataContext = this;
                this.numberColours = items.Count().ToString();
            }
        }

        //redo operation
        private void Redo_Click(object sender, RoutedEventArgs e)
        { 
            RedoFunction();
        }

        void RedoFunction()
        {
            if (Pattern.hasNextState())//currentIndex != patternStates.Count() - 1)
            {
                //currentIndex++;
                //wBitmap = patternStates[currentIndex];
                wBitmap = Pattern.GetNextState();
                image.Source = wBitmap;

                BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);

                DMC closestColor = new DMC();
                double distance = 1000;
                //List<System.Drawing.Color> paletteList = new List<System.Drawing.Color>();
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
                }

                items.Clear();

                //creating the palette
                for (int i = 0; i < DMCitems.Count(); i++)
                {
                    if (!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
                        items.Add(new ListItemColour() { Number = "  " + DMCitems[i].Floss, Name = "  " + DMCitems[i].Description, color = System.Windows.Media.Color.FromRgb((byte)DMCitems[i].Red, (byte)DMCitems[i].Green, (byte)DMCitems[i].Blue) });
                }
                this.DataContext = this;
                this.numberColours = items.Count().ToString();
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


        //setting symbols on pattern
        private void UpScale_Click(object sender, RoutedEventArgs e)
        {

            //convert to bitmap
            System.Drawing.Bitmap img = utilities.ConvertToBitmap(wBitmap);


            double iconHeight = (image.ActualHeight / patternHeight) * 0.6;
            double stitchSize = image.ActualHeight / patternHeight;
            double stitchStartPosition = (stitchSize / 2) - (iconHeight / 2);
            double stitchPositionY = stitchStartPosition;
          

            // Create a DrawingGroup to combine the ImageDrawing objects.
            DrawingGroup imageDrawings = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(imageDrawings, BitmapScalingMode.NearestNeighbor);

            ImageDrawing pattern = new ImageDrawing();
            pattern.Rect = new Rect(0,0, image.ActualWidth, image.ActualHeight);
            pattern.ImageSource = wBitmap;

            //patternWidth = (int)(image.ActualWidth / stitchSize);

            imageDrawings.Children.Add(pattern);

            //drawing symbols
            for (int j = 0; j < img.Height; j++)
            {
                //changing Y position
                stitchPositionY = stitchStartPosition + j * stitchSize;
                for (int i = 0; i < img.Width; i++)
                {
                    System.Drawing.Color stitchColor = img.GetPixel(i, j);

                    for(int k = 0; k < items.Count(); k++)
                    {
                        if(stitchColor == System.Drawing.Color.FromArgb(items[k].color.R, items[k].color.G, items[k].color.B))
                        {
                            string iconName = (k+0).ToString() + ".PNG";
                            // Create a 100 by 100 image with an upper-left point of (75,75).
                            ImageDrawing icon1 = new ImageDrawing();
                            icon1.Rect = new Rect(stitchStartPosition + (stitchSize * i), stitchPositionY, iconHeight, iconHeight);
                            icon1.ImageSource = new BitmapImage(
                                new Uri(iconName, UriKind.Relative));

                            imageDrawings.Children.Add(icon1);
                            break;
                        }                      
                    }
                }                      
            }
            
            DrawingImage drawingImageSource = new DrawingImage(imageDrawings);

            // Freeze the DrawingImage for performance benefits.
            drawingImageSource.Freeze();

            double destWidth = (int)drawingImageSource.Width;
            double destHeight = (int)drawingImageSource.Height;

            if ((patternHeight > 20 || patternWidth > 20) && (patternHeight < 200 && patternWidth < 200))
            {
                //destHeight = destHeight*patternHeight/ (10/(patternHeight* patternHeight));
                //destWidth = destWidth* patternHeight/ (10 / (patternHeight * patternHeight));

                destHeight = destHeight * stitchSize;
                destWidth = destWidth * stitchSize;
            }
            else if(patternHeight > 200 || patternWidth > 200)
            {                          
                destHeight = destHeight * patternHeight / 20;
                destWidth = destWidth * patternHeight / 20;               
            }
           

            // DrawingImage -> DrawingVisual -> Render -> (RenderTarget)Bitmap seems to be the best way
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();
            Rect rect = new Rect(0, 0, destWidth, destHeight);
            context.DrawImage(drawingImageSource, rect);
            context.Close();

            
                           
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)destWidth, (int)destHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);



            System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image();
            imageControl.Stretch = Stretch.None;
            imageControl.Source = drawingImageSource;

            //image.Source = imageControl.Source; IMPORTANT

            //making a new image to pass to the symbol window
            System.Windows.Controls.Image passImage = new System.Windows.Controls.Image();
            passImage.Source = imageControl.Source;

            //show the symbol window, passing the image
            Symbol symbolWindow = new Symbol(imageControl);
            Nullable<bool> result2 = symbolWindow.ShowDialog();

            if(result2 == true)
            {
                //save
                utilities.Save(bitmap);
            }
        }


        //if image pixel is clicked
        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // coordinates are now available in p.X and p.Y
            var p = e.GetPosition(image);

            System.Drawing.Color bitmapColour = System.Drawing.Color.FromArgb(currentColour.R, currentColour.G, currentColour.B);

            //converting to bitmap
            System.Drawing.Bitmap img = utilities.ConvertToBitmap(wBitmap);


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

            wBitmap = utilities.BitmapToImageSource(img);
            image.Source = wBitmap;

            //store state of image
            //patternStatesAdd();
            Pattern.patternStatesAdd(ref wBitmap);

            //if drawing tool is used
            if (!isEraser)
            {
                //adding new color to palette
               
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

                //checking if that colour is already in the palette
                int count = 0;
                for (int i = 0; i < items.Count(); i++)
                {
                    if (items[i].Name == nextName)
                    {
                        count++;
                    }
                }

                //if the colour is not in the palette, then it it added
                if (count == 0)
                {
                    items.Add(new ListItemColour() { Number = "  " + closestColor.Floss, Name = nextName, color = System.Windows.Media.Color.FromRgb((byte)closestColor.Red, (byte)closestColor.Green, (byte)closestColor.Blue) });
                }

                this.DataContext = this;
                this.numberColours = items.Count().ToString();
            }
            //if eraser tool is used
            else
            {
                //getting the RGB colours in the pattern
                BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);


                DMC closestColor = new DMC();
                double distance = 1000; //initial distance between colours, set to a way too big value
                List<System.Drawing.Color> paletteList = new List<System.Drawing.Color>();
                DMCitems.Clear(); //clearing the palette, so that the right colours could be added back in

                //getting closest DMC colours to RGB in pattern
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

                //remaking palette based on the colours found in the pattern to make sure that if a colour was erased,
                //then it is removed from the palette
                for (int i = 0; i < DMCitems.Count(); i++)
                {
                    if (!(myPalette.Colors[i].ToString().ToLower() == "#ffffffff"))
                        items.Add(new ListItemColour() { Number = "  " + DMCitems[i].Floss, Name = "  " + DMCitems[i].Description, color = System.Windows.Media.Color.FromRgb((byte)DMCitems[i].Red, (byte)DMCitems[i].Green, (byte)DMCitems[i].Blue) });
                }

                //binding number of colours to display in properties
                this.DataContext = this;
                this.numberColours = items.Count().ToString();
            }

        }

        private void previewButton_Click(object sender, RoutedEventArgs e)
        {
            //convert to bitmap
            System.Drawing.Bitmap img = utilities.ConvertToBitmap(wBitmap);

            double stitchSize = ((image.ActualHeight / patternHeight) * (patternHeight / 5.0)) / 1.5 - 10;
            double stitchSizeY = stitchSize - 7.5;
            double stitchSizeX = stitchSize - 13;
            double aidaSize = 1772;
            double stitchStartPosition;
            double stitchStartPositionY = aidaSize / 2 - (image.ActualHeight) + 85;


            // Create a DrawingGroup to combine the ImageDrawing objects.
            DrawingGroup imageDrawings = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(imageDrawings, BitmapScalingMode.NearestNeighbor);

            
            ImageDrawing background = new ImageDrawing();
           
            if (patternHeight > 15 || patternWidth > 15)
            {
                background.Rect = new Rect(0, 0, ((stitchSize*1.1) * patternWidth)*1.2, (stitchSize * patternHeight)+10);
                background.ImageSource = new BitmapImage(new Uri("aida.png", UriKind.Relative));
                //aidaSize = 1772;
                stitchStartPositionY = ((stitchSize * 1.1) * patternWidth)*0.1;
                stitchStartPosition = ((stitchSize * 1.1) * patternWidth) * 0.1;
            }
            else 
            {           
                background.Rect = new Rect(0, 0, 1772, 1772);
                background.ImageSource = new BitmapImage(new Uri("aidasmall.png", UriKind.Relative));
                aidaSize = 1772;
                //stitchStartPosition = aidaSize / 2 - image.ActualWidth / 2;
                stitchStartPosition = aidaSize / 2 - image.ActualWidth;
            }

            //adding background 
            imageDrawings.Children.Add(background);

             
            double stitchPositionY = stitchStartPositionY;

            //set up for bitmap that is passed to the shader
            BitmapImage bitmap2 = new BitmapImage();
            bitmap2.BeginInit();
            bitmap2.UriSource = new Uri(@"../Debug/stitch4WhiteS.png", UriKind.Relative);
            bitmap2.EndInit();
          
            //creating pixel shader object
            ThresholdEffect effect = new ThresholdEffect();

            //objects for shader setup
            BitmapSource bitmapX;
            System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle();
            
            System.Windows.Media.Color inputColor = new System.Windows.Media.Color();
            ImageDrawing icon1 = new ImageDrawing();
            DrawingImage drawingImageSourceTemp;
            DrawingVisual visual2 = new DrawingVisual();
            DrawingContext context2;
            ImageDrawing icon2 = new ImageDrawing();
           //RenderTargetBitmap bitmapTemp;

            //drawing stitches
            for (int j = 0; j < img.Height; j++)
            {
                //changing Y position
                stitchPositionY = stitchStartPositionY + (j * stitchSizeY);
                for (int i = 0; i < img.Width; i++)
                {
                    //getting colour from pattern
                    System.Drawing.Color stitchColor = img.GetPixel(i, j);

                    //shader setup
                    inputColor = System.Windows.Media.Color.FromArgb(255, stitchColor.R, stitchColor.G, stitchColor.B);
                    effect.BlankColor = inputColor;

                    //bitmap to fill the rectangle with
                    bitmapX = bitmap2;
                    r.Fill = new ImageBrush(bitmapX);
                    r.Effect = effect; // set rectangle effect to shader

                    System.Windows.Size sz = new System.Windows.Size(bitmapX.PixelWidth, bitmapX.PixelHeight);
                    r.Measure(sz);
                    r.Arrange(new Rect(sz));
                    {
                        //render rectangle with shader effect
                        var rtb = new RenderTargetBitmap((int)sz.Width, (int)sz.Height, 96, 96, PixelFormats.Pbgra32);
                        rtb.Render(r);

                        //ImageDrawing icon1 = new ImageDrawing(); 
                        //creating image drawing at the right stitch position 
                        icon1.Rect = new Rect(stitchStartPosition + (stitchSizeX * i), stitchPositionY, stitchSize, stitchSize);
                        icon1.ImageSource = rtb; //setting the rendered rectangle as the source
                        imageDrawings.Children.Add(icon1);
                    }
                    GC.Collect();
                    drawingImageSourceTemp = new DrawingImage(imageDrawings);

                    double destWidth2 = (int)drawingImageSourceTemp.Width;
                    double destHeight2 = (int)drawingImageSourceTemp.Height;

                    //DrawingVisual visual2 = new DrawingVisual();
                    context2 = visual2.RenderOpen();
                    Rect rect2 = new Rect(0, 0, destWidth2, destHeight2);
                    context2.DrawImage(drawingImageSourceTemp, rect2);
                    context2.Close();

                    { 
                        RenderTargetBitmap bitmapTemp = new RenderTargetBitmap((int)destWidth2, (int)destHeight2, 96, 96, PixelFormats.Pbgra32);
                        //bitmapTemp = new RenderTargetBitmap((int)destWidth2, (int)destHeight2, 96, 96, PixelFormats.Pbgra32);
                        bitmapTemp.Render(visual2);
                       
                        imageDrawings.Children.Clear();
                       
                        //ImageDrawing icon2 = new ImageDrawing();
                        icon2.Rect = new Rect(0, 0, destWidth2, destHeight2);
                        icon2.ImageSource = bitmapTemp;
                        imageDrawings.Children.Add(icon2);

                    }
                    GC.Collect();
                }
            }

            //creating drawingImage from the drawingImage group
            DrawingImage drawingImageSource = new DrawingImage(imageDrawings);

            // Freeze the DrawingImage for performance benefits.
            drawingImageSource.Freeze();

            double destWidth = (int)drawingImageSource.Width;
            double destHeight = (int)drawingImageSource.Height;


            // DrawingImage -> DrawingVisual -> Render -> (RenderTarget)Bitmap seems to be the best way
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();
            Rect rect = new Rect(0, 0, destWidth, destHeight);
            context.DrawImage(drawingImageSource, rect);
            context.Close();

            //render the drawingImage to a bitmap
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)destWidth, (int)destHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);


            System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image();
            imageControl.Stretch = Stretch.None;
            imageControl.Source = drawingImageSource;

            //image.Source = imageControl.Source; IMPORTANT

            //making a new image to pass to the preview window
            System.Windows.Controls.Image passImage = new System.Windows.Controls.Image();
            passImage.Source = imageControl.Source;

            //show the preview window, passing the image
            Preview previewWindow = new Preview(imageControl);
            Nullable<bool> result2 = previewWindow.ShowDialog();

            //save
            if (result2 == true)
            {
                utilities.Save(bitmap);
            }
        }

        //close application
        private void ItemExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            //Environment.Exit(0);
        }

        //shows "About" window
        private void ItemAbout_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }
    }
}
