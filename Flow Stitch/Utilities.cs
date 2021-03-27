using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;


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
            grPhoto.InterpolationMode = InterpolationMode.NearestNeighbor;


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
