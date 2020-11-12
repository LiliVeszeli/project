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





namespace Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
         
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            //Class1 class1 = new Class1();
            // class1.pixelate();
            // class1.i = 2;
           

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
                if(selectedFileName.Contains(".jpg") || selectedFileName.Contains(".png") || selectedFileName.Contains(".PNG") || selectedFileName.Contains(".jpeg"))
                {
                    bitmap.EndInit();
                    image.Source = bitmap;
                }
                //if it isn't an image it displays an error message, the program doens't crash
                else
                {
                    MessageBox.Show("Error: Select a file with .jpg or .png extension.", "ERROR");
                }
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
