using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow_Stitch
{
    //class to store DMC thread colour properties
    public class DMC
    {
        public string Floss { get; set; } //identifying number of colour
        public string Description { get; set; } //name of colour

        //RGB values of colour
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

    }
}
