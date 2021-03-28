using CsvHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Forms;
using System.Linq;
using ColorMine.ColorSpaces.Comparisons;
using ColorMine.ColorSpaces;
using Dsafa.WpfColorPicker;

namespace Flow_Stitch
{
    //helper methods for the program
    public class Utilities
    {
        //converts bitmaps to writable bitmaps
        public WriteableBitmap BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return new WriteableBitmap(bitmapimage);
            }
        }

        //converts writable bitmap to bitmap
        public Bitmap ConvertToBitmap(BitmapSource wBitmap)
        {
            //convert to bitmap using a stream
            MemoryStream outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(wBitmap));
            enc.Save(outStream);
            return new System.Drawing.Bitmap(outStream);
        }

        //saves the pattern. writable bitmap can be passed to it too
        public void Save(BitmapSource bitmap)
        {
            //save
            //brings up save file dialog
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document";
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            Nullable<bool> result = dlg.ShowDialog();
            string fileName = "";

            //if OK is pressed, the image is saved as a PNG
            if (result == true)
            {
                fileName = dlg.FileName;
                PngBitmapEncoder jpg = new PngBitmapEncoder(); 
                jpg.Frames.Add(BitmapFrame.Create(bitmap));
                using (Stream stm = File.Create(fileName))
                {
                    jpg.Save(stm);
                }
            }
        }

        //reads in DMC colours from a file. Using csv helper
        public void ReadInColours(ref List<DMC>DMCColors)
        {
            using (var reader = new StreamReader("dmc.csv"))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
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

        public void OpenFile(ref WriteableBitmap wBitmap, ref System.Windows.Controls.Image image)
        {
            //opening files
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
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
                    System.Windows.Forms.MessageBox.Show("Error: Select a file with .jpg or .png extension.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void InputErrorMessages(ref int numberOfColours, ref int heightOfPattern)
        {
            if (numberOfColours == 0 || heightOfPattern == 0)
                System.Windows.Forms.MessageBox.Show("Error: Please input a value.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (numberOfColours < 2 || numberOfColours > 256)
                System.Windows.Forms.MessageBox.Show("Error: Input a number between 2 and 256 for Number of colours.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (heightOfPattern < 1)
                System.Windows.Forms.MessageBox.Show("Error: Input a value bigger than 0 for Height of pattern.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        public void RGBToDMC(ref List<DMC> DMCColors, ref DMC closestColor, ref ColorPickerDialog dialog)
        {
           //starting distance between the colours
            double distance = 1000;

            //getting closest DMC coolour to selected colour
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

                var comparison = new CmcComparison();
                var deltaE = comparison.Compare(dialogRgb, DMCRgb);

                //finding smallest distance
                if (deltaE < distance)
                {
                    closestColor = DMCColors[j];
                    distance = deltaE;
                }
            }
        }


        public void ClosestDMC(ref List<DMC> DMCColors, ref List<DMC> DMCitems, ref BitmapPalette myPalette)
        {
            DMC closestColor = new DMC();
            double distance = 1000; //initial distance between colours, set to a way too big value

            DMCitems.Clear();  //clearing the palette, so that the right colours could be added back in

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
        }


        public void ClosestDMCToRGB(ref List<DMC> DMCColors, ref DMC closestColor, ref System.Windows.Media.Color currentColour)
        {
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
        }

        //scaling down image
        public static System.Drawing.Bitmap ScaleByPercent(System.Drawing.Image imgPhoto, float Percent, int Height)
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
            //using bicubic downsampling
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //wrapping the image, so it doesn't sample from outside of it. 
            //Solving the problem of the black line
            using (ImageAttributes wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);

                grPhoto.DrawImage(imgPhoto,
                    new System.Drawing.Rectangle(destX, destY, destWidth, destHeight),
                    sourceX, sourceY, sourceWidth, sourceHeight,
                    GraphicsUnit.Pixel, wrapMode);
            }

            grPhoto.Dispose();
            return bmPhoto;
        }



        //scaling up image
        public static System.Drawing.Bitmap ScaleByPercentUp(System.Drawing.Image imgPhoto, float Percent)
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
            //using nearest neighbour sampling, so that the pattern stays pixelated
            grPhoto.InterpolationMode = InterpolationMode.NearestNeighbor;

            //wrapping the image to not get the black line
            using (ImageAttributes wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);

                grPhoto.DrawImage(imgPhoto,
                    new System.Drawing.Rectangle(destX, destY, destWidth, destHeight),
                    sourceX, sourceY, sourceWidth, sourceHeight,
                    GraphicsUnit.Pixel, wrapMode);
            }

            grPhoto.Dispose();
            return bmPhoto;
        }
    }
}
