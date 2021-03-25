using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow_Stitch
{
    class PreviousBlendingMethods
    {
        //colour blending
        //for (int b = 0; b < XStitch.Height; b++)
        //{
        //    for (int c = 0; c < XStitch.Width; c++)
        //    {
        //        System.Drawing.Color XColor = XStitch.GetPixel(c, b);

        //        int red = XColor.R * stitchColor.R / 255;
        //        int blue = XColor.B * stitchColor.B / 255;
        //        int green = XColor.G * stitchColor.G / 255;
        //        System.Drawing.Color ResultColor = System.Drawing.Color.FromArgb(red, green, blue);

        //        if (XColor.A > 0.5)
        //            XStitch.SetPixel(c, b, ResultColor);

        //        XStitch.MakeTransparent();
        //    }
        //}




        //faster colour blending
        //int width = XStitch.Width;
        //int height = XStitch.Height;
        //var rect = new Rectangle(0, 0, width, height);
        //BitmapData lowerData = XStitch.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

        //BitmapData lowerData = XStitch.LockBits(XStitch, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //BitmapData dstData = tile.LockBits(new Rectangle(0, 0, tile.Width, tile.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);



        //unsafe
        //{
        //    byte* lowerPointer = (byte*)lowerData.Scan0;


        //    for (int d = 0; d < height; d++)
        //    {
        //        for (int f = 0; f < width; f++)
        //        {

        //            int red = lowerPointer[2] * stitchColor.R / 255;
        //            int blue = lowerPointer[0] * stitchColor.B / 255;
        //            int green = lowerPointer[1] * stitchColor.G / 255;
        //            System.Drawing.Color ResultColor = System.Drawing.Color.FromArgb(red, green, blue);



        //            lowerPointer[0] = ResultColor.B;
        //            lowerPointer[1] = ResultColor.G;
        //            lowerPointer[2] = ResultColor.R;
        //            lowerPointer[3] = ResultColor.A;

        //            // Moving the pointers by 3 bytes per pixel
        //            lowerPointer += 3;

        //        }

        //        // Moving the pointers to the next pixel row
        //        lowerPointer += lowerData.Stride - (width * 3);

        //    }
        //}//end of unsafe

        //XStitch.UnlockBits(lowerData);
    }
}
