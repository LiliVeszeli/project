using Dsafa.WpfColorPicker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using AForge.Imaging.ColorReduction;
using System.Collections.ObjectModel;

namespace Flow_Stitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
       
        //**********************************************************
        //*                    VARIABLES                           *
        //**********************************************************
     
        
        WriteableBitmap wBitmap; //stores the image in bitmap format
        bool isEraser = false; //indicates if the eraser is being used
        float upscalePercentage; //stores how much to upsacle image to go back to original size
        int patternWidth; //width of pattern in stitches
        int patternHeight; //height of pattern in stitches
        System.Windows.Media.Color currentColour = System.Windows.Media.Color.FromRgb(0, 0, 0); //stores the color that is currently used
        System.Drawing.Color[] palette; //stores the colours in the pattern
        ObservableCollection<ListItemColour> items = new ObservableCollection<ListItemColour>(); //stores listbox items
        List<DMC> DMCitems = new List<DMC>(); //stores lisbox items, but as DMC colors
        ObservableCollection<ListItemColour> DMCColoursList = new ObservableCollection<ListItemColour>(); //stores all DMC colors but as ListItemColours
        List<DMC> DMCColors = new List<DMC>(); //stores all DMC colours that were read in from the file

        //instances of classes used in the pattern
        private Pattern Pattern = new Pattern(); //undo/redo functionality
        private Utilities utilities = new Utilities(); //helper methods
       
        
        //number of colours in the pattern
        public string numberColours 
        {
            get { return (string)GetValue(numberColoursProperty); }
            set { SetValue(numberColoursProperty, value); }
        }       
        public static readonly DependencyProperty numberColoursProperty =
            DependencyProperty.Register("numberColours", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));


        //contructor for main window
        public MainWindow()
        {
            InitializeComponent();

            //reading in DMC colors
            utilities.ReadInColours(ref DMCColors);
        }




        //**********************************************************
        //*                 EVENT HANDLERS                         *
        //**********************************************************


        //loads in new image, converts it into a pattern
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
                utilities.OpenFile(ref wBitmap, ref image);

                // resetting the undo states               
                Pattern.Clear();
               

                //making sure a picture was loaded in before doing operations
                if (wBitmap != null)
                {
                    //convert to bitmap
                    System.Drawing.Bitmap img = utilities.ConvertToBitmap(wBitmap);

                    if (img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb || img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb || img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb || img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                    {
                        //creating pattern
                        ImageProcessor.MakePattern(ref wBitmap, ref heightOfPattern, ref upscalePercentage, ref numberOfColours, ref patternHeight,
                                     ref img, ref image, ref DMCitems, ref DMCColors, ref DMCColoursList, ref items, ref palette);

                        //binding listbox to items
                        listBox.ItemsSource = items;
                        
                        //store state of image
                        Pattern.patternStatesAdd(ref wBitmap);

                        patternWidth = (int)image.Source.Width;
                        //outputting width to properties
                        WidthTextBlock.Text = " Width: " + patternWidth.ToString();

                        //displaying height of pattern in properties
                        HeightTextBlock.Text = " Height: " + heightOfPattern.ToString();

                        //bindig number of colours
                        this.DataContext = this;
                        this.numberColours = items.Count().ToString();
                    }
                    else
                    {
                        image.Source = null;
                        System.Windows.Forms.MessageBox.Show("Error: Select a file with 24 or 32 bit depth.", "ERROR", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }

                }//if wbitmap not null
            }
            else
            {
                //input validation and error messages
                if (result2 == true)
                {
                    utilities.InputErrorMessages(ref numberOfColours, ref heightOfPattern);
                }
            }
        }

       
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
            }        
        }

        //clicking on the draw button
        private void drawButtonImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //isDrawing = true;
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
                //converting the selected RGB colour to the closest DMC thread colour
                DMC closestColor = new DMC();
                utilities.RGBToDMC(ref DMCColors, ref closestColor, ref dialog);

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
                //getting closest DMC colour to the selected colour
                DMC closestColor = new DMC();
                utilities.RGBToDMC(ref DMCColors, ref closestColor, ref dialog);
                
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

        //clicking a colour in the palette - the user can draw with that color
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
                System.Windows.Forms.MessageBox.Show("Error: Cannot delete all colours from pattern.", "ERROR", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
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
                wBitmap = Pattern.GetPreviousState();
                image.Source = wBitmap;

                //*reamking the palette, in case the colours changed with the undo opeartion*

                //getting colours in the pattern in RGB
                BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);

                //getting DMC colours of RGB palette
                utilities.ClosestDMC(ref DMCColors, ref DMCitems, ref myPalette);

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
            if (Pattern.hasNextState())
            {
                wBitmap = Pattern.GetNextState();
                image.Source = wBitmap;


                //*reamking the palette, in case the colours changed with the redo opeartion*

                //getting colours in the pattern in RGB
                BitmapPalette myPalette = new BitmapPalette(wBitmap, 256);

                //getting DMC colours of RGB palette
                utilities.ClosestDMC(ref DMCColors, ref DMCitems, ref myPalette);

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
                    //calls undo and redo functions
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

            imageDrawings.Children.Add(pattern);

            //drawing symbols
            for (int j = 0; j < img.Height; j++)
            {
                //changing Y position
                stitchPositionY = stitchStartPosition + j * stitchSize;
                for (int i = 0; i < img.Width; i++)
                {
                    System.Drawing.Color stitchColor = img.GetPixel(i, j);

                    //check if next square is white, meaning it is erased, so no stitch
                    if (stitchColor != System.Drawing.Color.FromArgb(255, 255, 255, 255))
                    {
                        for (int k = 0; k < items.Count(); k++)
                        {
                            if (stitchColor == System.Drawing.Color.FromArgb(items[k].color.R, items[k].color.G, items[k].color.B))
                            {
                                string iconName = (k + 0).ToString() + ".PNG";

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
           

            // DrawingImage -> DrawingVisual -> Render -> (RenderTarget)Bitmap
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();
            Rect rect = new Rect(0, 0, destWidth, destHeight);
            context.DrawImage(drawingImageSource, rect);
            context.Close();
             
            //render image on the screen
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)destWidth, (int)destHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image();
            imageControl.Stretch = Stretch.None;
            imageControl.Source = drawingImageSource;


            //making a new image to pass to the symbol window
            System.Windows.Controls.Image passImage = new System.Windows.Controls.Image();
            passImage.Source = imageControl.Source;

            //show the symbol window, passing the image
            Symbol symbolWindow = new Symbol(imageControl);
            Nullable<bool> result2 = symbolWindow.ShowDialog();

            if(result2 == true)
            {
                //save pattern with symbols
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
            Pattern.patternStatesAdd(ref wBitmap);

            //if drawing tool is used
            if (!isEraser)
            {
                //adding new color to palette
               
                DMC closestColor = new DMC();

                ////getting closest DMC colours to RGB    
                utilities.ClosestDMCToRGB(ref DMCColors, ref closestColor, ref currentColour);

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
                utilities.ClosestDMC(ref DMCColors, ref DMCitems, ref myPalette);

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
                background.Rect = new Rect(0, 0, ((stitchSize*1.1) * patternWidth)*1.2, (stitchSize * patternHeight)+50);
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
            RenderTargetBitmap bitmapTemp;
            RenderTargetBitmap rtb;
            int it = 1;
           

            //drawing stitches
            for (int j = 0; j < img.Height; j++)
            {
                //changing Y position
                stitchPositionY = stitchStartPositionY + (j * stitchSizeY);
                for (int i = 0; i < img.Width; i++)
                {
                    //getting colour from pattern
                    System.Drawing.Color stitchColor = img.GetPixel(i, j);

                    //check if next square is white, meaning it is erased, so no stitch
                    if (stitchColor != System.Drawing.Color.FromArgb(255, 255, 255, 255))
                    {
                        var XStitch = new System.Drawing.Bitmap("stitch4WhiteS.png");

                        ////pointer blending 2
                        //LockBitmap lockBitmap = new LockBitmap(XStitch);
                        //lockBitmap.LockBits();


                        //for (int y = 0; y < lockBitmap.Height; y++)
                        //{
                        //    for (int x = 0; x < lockBitmap.Width; x++)
                        //    {
                        //        System.Drawing.Color XColor = lockBitmap.GetPixel(x, y);

                        //        int red = XColor.R * stitchColor.R / 255;
                        //        int blue = XColor.B * stitchColor.B / 255;
                        //        int green = XColor.G * stitchColor.G / 255;
                        //        System.Drawing.Color ResultColor = System.Drawing.Color.FromArgb(red, green, blue);

                        //        if (XColor.A > 0.5)
                        //            lockBitmap.SetPixel(x, y, ResultColor);
                        //    }
                        //}
                        //lockBitmap.UnlockBits();

                        //XStitch.MakeTransparent();



                        //shader setup
                        inputColor = System.Windows.Media.Color.FromArgb(255, stitchColor.R, stitchColor.G, stitchColor.B);
                        effect.BlankColor = inputColor;

                        //bitmap to fill the rectangle with
                        bitmapX = bitmap2;
                        r.Fill = new ImageBrush(bitmapX);
                        r.Effect = effect; // set rectangle effect to shader

                        //get size of image
                        System.Windows.Size sz = new System.Windows.Size(bitmapX.PixelWidth, bitmapX.PixelHeight);
                        r.Measure(sz);
                        r.Arrange(new Rect(sz));
                        {
                            //render rectangle with shader effect
                            rtb = new RenderTargetBitmap((int)sz.Width, (int)sz.Height, 96, 96, PixelFormats.Pbgra32);
                            rtb.Render(r);

                            //rtb = RenderImage(sz, r);
                            if (it == 20 || it == 40 || it == 60 || it == 80 || it == 100)
                            {
                                GC.Collect();
                            }

                            //ImageDrawing icon1 = new ImageDrawing(); 
                            //creating image drawing at the right stitch position 
                            icon1.Rect = new Rect(stitchStartPosition + (stitchSizeX * i), stitchPositionY, stitchSize, stitchSize);
                            icon1.ImageSource = rtb; //setting the rendered rectangle as the source
                                                     //icon1.ImageSource = utilities.BitmapToImageSource(XStitch);
                            imageDrawings.Children.Add(icon1.Clone());
                            //imageDrawings.Children.Add(icon1);
                            it++;
                        }
                        //if (it == 20 || it == 40 || it == 60 || it == 80 || it == 100)
                        //{


                        //    drawingImageSourceTemp = new DrawingImage(imageDrawings);

                        //    double destWidth2 = (int)drawingImageSourceTemp.Width;
                        //    double destHeight2 = (int)drawingImageSourceTemp.Height;

                        //    //DrawingVisual visual2 = new DrawingVisual();
                        //    context2 = visual2.RenderOpen();
                        //    Rect rect2 = new Rect(0, 0, destWidth2, destHeight2);
                        //    context2.DrawImage(drawingImageSourceTemp, rect2);
                        //    context2.Close();

                        //    {
                        //        bitmapTemp = new RenderTargetBitmap((int)destWidth2, (int)destHeight2, 96, 96, PixelFormats.Pbgra32);
                        //        bitmapTemp.Render(visual2);
                        //        //bitmapTemp = RenderImage2(visual2, destWidth2, destHeight2);

                        //        imageDrawings.Children.Clear();

                        //        //ImageDrawing icon2 = new ImageDrawing();
                        //        icon2.Rect = new Rect(0, 0, destWidth2, destHeight2);
                        //        icon2.ImageSource = bitmapTemp;
                        //        imageDrawings.Children.Add(icon2.Clone());

                        //    }
                        //    GC.Collect();
                        //}
                    }
                }
            }

            //creating drawingImage from the drawingImage group
            DrawingImage drawingImageSource = new DrawingImage(imageDrawings);

            // Freeze the DrawingImage for performance benefits.
            drawingImageSource.Freeze();

            double destWidth = (int)drawingImageSource.Width;
            double destHeight = (int)drawingImageSource.Height;


            // DrawingImage -> DrawingVisual -> Render -> (RenderTarget)Bitmap
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

            //making a new image to pass to the preview window
            System.Windows.Controls.Image passImage = new System.Windows.Controls.Image();
            passImage.Source = imageControl.Source;

            //show the preview window, passing the image
            Preview previewWindow = new Preview(imageControl);
            Nullable<bool> result2 = previewWindow.ShowDialog();

            //save preview
            if (result2 == true)
            {
                utilities.Save(bitmap);
            }
        }

        //close application
        private void ItemExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        //shows "About" window
        private void ItemAbout_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }
    }
}
