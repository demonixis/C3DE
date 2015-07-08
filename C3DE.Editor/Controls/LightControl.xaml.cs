using C3DE.Components.Lights;
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

namespace C3DE.Editor.Controls
{
    /// <summary>
    /// Interaction logic for LightControl.xaml
    /// </summary>
    public partial class LightControl : UserControl
    {
        private Light light;

        public int LightTypeLight
        {
            get
            {
                if (light != null)
                    return (int)light.Type;

                return 0;
            }
            set
            {
                if (light != null)
                    light.Type = (LightType)value;
            }
        }

        public float LightRange
        {
            get
            {
                if (light != null)
                    return light.Range;

                return 0.0f;
            }
            set
            {
                if (light != null)
                    light.Range = value;
            }
        }

        public string LightColor
        {
            get
            {
                if (light != null)
                {
                    var color = Color.FromArgb(light.DiffuseColor.A, light.DiffuseColor.R, light.DiffuseColor.G, light.DiffuseColor.B);
                    return string.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
                }

                return "#ffffff";
            }
            set
            {
                if (light != null)
                {
                    var strColor = value;
                    var color = (Color)ColorConverter.ConvertFromString(strColor);
                    light.DiffuseColor = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
                    LightColorRect.Fill = new SolidColorBrush(color);
                }
            }
        }

        public float LightIntensity
        {
            get
            {
                if (light != null)
                    return light.Intensity;

                return 0.0f;
            }
            set
            {
                if (light != null)
                    light.Intensity = value;
            }
        }

        public int LightShadowType
        {
            get
            {
                if (light != null)
                    return light.ShadowGenerator.Enabled ? 1 : 0;

                return 0;
            }
            set
            {
                if (light != null)
                    light.ShadowGenerator.Enabled = value > 0;
            }
        }

        public int LightmapType
        {
            get
            {
                if (light != null)
                    return (int)light.Backing;

                return 0;
            }
            set
            {
                if (light != null)
                    light.Backing = (LightRenderMode)value;
            }
        }

        public LightControl()
        {
            InitializeComponent();
        }

        public LightControl(Light plight)
            : this()
        {
            light = plight;
            DataContext = this;
        }
    }
}
