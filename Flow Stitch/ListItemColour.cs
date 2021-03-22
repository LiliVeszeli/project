using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Flow_Stitch
{
    public class ListItemColour : INotifyPropertyChanged
    {
        public string _Name { get; set; } //description

        public string _Number { get; set; } //floss
        public System.Windows.Media.Color _Color { get; set; } //rgb
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        public ListItemColour()
        {
        }

        public System.Windows.Media.Color color
        {
            get { return _Color; }
            set
            {
                _Color = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        public string Number
        {
            get { return _Number; }
            set
            {
                _Number = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
