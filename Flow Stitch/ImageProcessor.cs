using AForge.Imaging.ColorReduction;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Flow_Stitch
{
    public class ImageProcessor
    {
       
        public static void MakePattern(ref WriteableBitmap wBitmap, ref int heightOfPattern, ref float upscalePercentage, ref int numberOfColours, ref int patternHeight,
                                     ref System.Drawing.Bitmap img, ref Image image, ref List<DMC> DMCitems, ref List<DMC> DMCColors, ref ObservableCollection<ListItemColour> DMCColoursList,
                                     ref ObservableCollection<ListItemColour> items, ref System.Drawing.Color[] palette)
        {
            Utilities utilities = new Utilities();

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

            //requantize
            System.Drawing.Bitmap requantizedImage = new System.Drawing.Bitmap(quantizer.ReduceColors(scaledImage, numberOfColours)); //original

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

            

            //changing the colours in the pattern to the DMC colours
            System.Drawing.Bitmap requantizedImage2 = new System.Drawing.Bitmap(quantizer.ReduceColors(requantizedImage, palette)); //CHANGE BACK
            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(requantizedImage2);

            //putting it back into the image and the writable bitmap
            wBitmap = utilities.BitmapToImageSource(newBitmap);
            image.Source = wBitmap;
        }
    }
}
