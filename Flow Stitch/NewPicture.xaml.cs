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
using System.Windows.Shapes;

namespace Flow_Stitch
{
    /// <summary>
    /// Interaction logic for NewPicture.xaml
    /// </summary>
    /// opening new image
    public partial class NewPicture : Window
    {
        public NewPicture()
        {
            InitializeComponent();
            //makes window open in the centre
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        //if OK button is clicked, closes window and returns true
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        //if Cnacel button is clicked, closes window
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }      
    }
}
