using System.Windows;
using System.Windows.Media;

namespace C3DE.Editor
{
    public partial class RenderSettingsWindow : Window
    {
        private RenderSettings renderSettings;

        public string AmbientColor
        {
            get
            {
                var color = Color.FromArgb(renderSettings.AmbientColor.A, renderSettings.AmbientColor.R, renderSettings.AmbientColor.G, renderSettings.AmbientColor.B);
                return string.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
            }
            set
            {
                var strColor = value;
                var color = (Color)ColorConverter.ConvertFromString(strColor);
                renderSettings.AmbientColor = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
                AmbientColorRect.Fill = new SolidColorBrush(color);
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

        public string FogColor
        {
            get
            {
                var color = Color.FromArgb(renderSettings.FogColor.A, renderSettings.FogColor.R, renderSettings.FogColor.G, renderSettings.FogColor.B);
                return string.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
            }
            set
            {
                var strColor = value;
                var color = (Color)ColorConverter.ConvertFromString(strColor);
                renderSettings.FogColor = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
                FogColorRect.Fill = new SolidColorBrush(color);
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
