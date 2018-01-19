using C3DE.Components.Lighting;
using System.Windows.Controls;
using System.Windows.Media;

namespace C3DE.Editor.Controls
{
    /// <summary>
    /// Interaction logic for LightControl.xaml
    /// </summary>
    public partial class LightControl : UserControl
    {
        private Light light;

        public bool Enabled
        {
            get
            {
                if (light != null)
                    return light.Enabled;

                return false;
            }
            set
            {
                if (light != null)
                    light.Enabled = value;
            }
        }

        public int LightTypeLight
        {
            get
            {
                if (light != null)
                    return (int)light.TypeLight;

                return 0;
            }
            set
            {
                if (light != null)
                    light.TypeLight = (LightType)value;
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

        public Color LightColor
        {
            get
            {
                if (light != null)
                    return Color.FromArgb(light.Color.A, light.Color.R, light.Color.G, light.Color.B);
                    
                return Color.FromArgb(1, 0, 0, 0);
            }
            set
            {
                if (light != null)
                    light.Color = new Microsoft.Xna.Framework.Color(value.R, value.G, value.B, value.A);
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
