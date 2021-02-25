using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for DMCWindow.xaml
    /// </summary>
    /// 
   

    public partial class DMCWindow : Window
    {

        ObservableCollection<ListItemColour> DMCColoursList = new ObservableCollection<ListItemColour>();
        public Color currentColourW = new Color();
        public DMCWindow(ObservableCollection<ListItemColour> list)
        {
            InitializeComponent();

            DMCColoursList = list;
            listBox.ItemsSource = DMCColoursList;
        }

        //clicking a colour in the palette - you can draw with that color
        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //getting the selected listbox item
            ListItemColour selectedItem = (ListItemColour)listBox.SelectedItem;
            currentColourW = selectedItem.color;
            Close();
        }
    }


}
