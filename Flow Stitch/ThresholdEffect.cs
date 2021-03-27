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
    //shader wrapper class
    public class ThresholdEffect : ShaderEffect
    {
        //creates an instance of the pixel shader class
        PixelShader _pixelShader = new PixelShader();// { UriSource = MakePackUri("shader1.ps") };

        //loads in pixel shader file
       // Uri uri = new Uri(System.IO.Directory.GetCurrentDirectory() + @"\..\..\shader1.ps", UriKind.Relative);
        Uri uri = new Uri(@"/Flow Stitch;component/shader1.ps", UriKind.Relative);

        //constructor
        public ThresholdEffect()
        {
            _pixelShader.UriSource = uri;

            PixelShader = _pixelShader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(BlankColorProperty);
        }

        // MakePackUri is a utility method for computing a pack uri
        // for the given resource. 
        public static Uri MakePackUri(string relativeFile)
        {
            Assembly a = typeof(ThresholdEffect).Assembly;

            // Extract the short name.
            string assemblyShortName = a.ToString().Split(',')[0];

            string uriString = "pack://application:,,,/" +
                assemblyShortName +
                ";component/" +
                relativeFile;

            return new Uri(uriString);
        }

        ///////////////////////////////////////////////////////////////////////
        #region Input dependency property

        //goes in the slot for input image/texture. This is for the stitch image (S0)
        public Brush Inpu
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ThresholdEffect), 0);

        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region BlankColor dependency property

        //goes in the slot for a passed constant. This is the colour to blend the texture with (C0)
        public Color BlankColor
        {
            get { return (Color)GetValue(BlankColorProperty); }
            set { SetValue(BlankColorProperty, value); }
        }

        public static readonly DependencyProperty BlankColorProperty =
            DependencyProperty.Register("BlankColor", typeof(Color), typeof(ThresholdEffect),
                    new UIPropertyMetadata(PixelShaderConstantCallback(0)));

        #endregion
    }
}
