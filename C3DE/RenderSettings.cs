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
        internal Vector4 fogData;

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

        public FogMode FogMode
        {
            get { return (FogMode)(int)fogData.X; }
            set { fogData.X = (float)value; }
        }

        public float FogDensity
        {
            get { return fogData.Y; }
            set { fogData.Y = value; }
        }

        public float FogStart
        {
            get { return fogData.Z; }
            set { fogData.Z = value; }
        }

        public float FogEnd
        {
            get { return fogData.W; }
            set { fogData.W = value; }
        }
        
        public RenderSettings()
        {
            ambientColor = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
            fogData = Vector4.Zero;
            FogEnabled = false;
            FogMode = FogMode.Linear;
            FogDensity = 0.1f;
            FogStart = 10.0f;
            FogEnd = 250.0f;
            fogColor = Vector4.One;
        }
    }
}
