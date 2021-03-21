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
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //bitmap = new WriteableBitmap(bitmapPass);
            image.Source = imagePass.Source;
        }

        private void Savebutton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
    }
}
