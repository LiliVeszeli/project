using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Reflection;

namespace Flow_Stitch
{
    public class BlankEffect : ShaderEffect

    {

        public BlankEffect()

        {

            PixelShader pixelShader =

                         new PixelShader();

            Uri uri = new Uri(System.IO.Directory.GetCurrentDirectory() + @"\..\..\shader1.ps", UriKind.Absolute);

            pixelShader.UriSource = uri;

            this.PixelShader = pixelShader;

        }

    }
}
