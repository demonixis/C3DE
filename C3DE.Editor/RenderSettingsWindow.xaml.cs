using System.Windows;
using System.Windows.Media;

namespace C3DE.Editor
{
    public partial class RenderSettingsWindow : Window
    {
        private RenderSettings renderSettings;

        public Color AmbientColor
        {
            get
            {
                return Color.FromArgb(renderSettings.AmbientColor.A, renderSettings.AmbientColor.R, renderSettings.AmbientColor.G, renderSettings.AmbientColor.B);
            }
            set
            {
                renderSettings.AmbientColor = new Microsoft.Xna.Framework.Color(value.R, value.G, value.B, value.A);
            }
        }

        public bool FogEnabled
        {
            get { return renderSettings.FogEnabled; }
            set { renderSettings.FogEnabled = value; }
        }

        public int FogMode
        {
            get { return (int)renderSettings.FogMode; }
            set { renderSettings.FogMode = (FogMode)value; }
        }

        public Color FogColor
        {
            get
            {
                return Color.FromArgb(renderSettings.FogColor.A, renderSettings.FogColor.R, renderSettings.FogColor.G, renderSettings.FogColor.B);
            }
            set
            {
                renderSettings.FogColor = new Microsoft.Xna.Framework.Color(value.R, value.G, value.B, value.A);
            }
        }

        public float FogDensity
        {
            get { return renderSettings.FogDensity; }
            set { renderSettings.FogDensity = value; }
        }

        public float FogStart
        {
            get { return renderSettings.FogStart; }
            set { renderSettings.FogStart = value; }
        }

        public float FogEnd
        {
            get { return renderSettings.FogEnd; }
            set { renderSettings.FogEnd = value; }
        }

        public RenderSettingsWindow(RenderSettings rs)
        {
            renderSettings = rs;
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
