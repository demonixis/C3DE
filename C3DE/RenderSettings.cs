using Microsoft.Xna.Framework;

namespace C3DE
{
    public enum FogMode
    {
        None = 0, Linear, Exp, Exp2
    }

    public class RenderSettings
    {
        internal Vector4 fogColor;
        internal Vector4 ambientColor;

        public Color AmbientColor
        {
            get { return new Color(ambientColor); }
            set { ambientColor = value.ToVector4(); }
        }

        public Color FogColor
        {
            get { return new Color(fogColor); }
            set { fogColor = value.ToVector4(); }
        }

        public bool FogEnabled { get; set; }
        public FogMode FogMode { get; set; }
        public float FogDensity { get; set; }
        public float FogStart { get; set; }
        public float FogEnd { get; set; }

        public RenderSettings()
        {
            ambientColor = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
            FogEnabled = false;
            FogMode = FogMode.Linear;
            FogDensity = 0.1f;
            FogStart = 10.0f;
            FogEnd = 250.0f;
        }
    }
}
