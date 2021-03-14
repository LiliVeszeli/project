using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Flow_Stitch
{
    /// <summary>
    /// Interaction logic for Symbol.xaml
    /// </summary>
    public partial class Symbol : Window
    {
        WriteableBitmap bitmap;

        public Symbol(Image imagePass)
        {
            InitializeComponent();
            //bitmap = new WriteableBitmap(bitmapPass);
            image.Source = imagePass.Source;
        }

        private void Savebutton_Click(object sender, RoutedEventArgs e)
        {
            //save
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document";
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            Nullable<bool> result = dlg.ShowDialog();
            string fileName = "";

            if (result == true)
            {
                fileName = dlg.FileName;
                System.Windows.Size size = image.RenderSize;
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
                image.Measure(size);
                image.Arrange(new Rect(size)); // This is important
                rtb.Render(image);
                JpegBitmapEncoder jpg = new JpegBitmapEncoder(); //pngbit
                jpg.Frames.Add(BitmapFrame.Create(rtb));
                using (Stream stm = File.Create(fileName))
                {
                    jpg.Save(stm);
                }
            }

            Close();
        }
    }
}
