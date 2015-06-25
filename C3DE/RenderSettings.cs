using Microsoft.Xna.Framework;
using System;

namespace C3DE
{
    public enum FogMode
    {
        None = 0, Linear, Exp, Exp2
    }

    /// <summary>
    /// Define settings used by the renderer.
    /// </summary>
    [Serializable]
    public class RenderSettings
    {
        internal Vector3 fogColor;
        internal Vector3 ambientColor;
        internal Vector4 fogData;
        internal Skybox skybox;

        /// <summary>
        /// Gets or sets the global ambient color.
        /// </summary>
        public Color AmbientColor
        {
            get { return new Color(ambientColor); }
            set { ambientColor = value.ToVector3(); }
        }

        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public Color FogColor
        {
            get { return new Color(fogColor); }
            set { fogColor = value.ToVector3(); }
        }

        /// <summary>
        /// Enable or disable the fog.
        /// </summary>
        public bool FogEnabled { get; set; }

        /// <summary>
        /// Gets or sets the type of fog.
        /// </summary>
        public FogMode FogMode
        {
            get { return (FogMode)(int)fogData.X; }
            set { fogData.X = (float)value; }
        }

        /// <summary>
        /// Gets or sets the fog density.
        /// </summary>
        public float FogDensity
        {
            get { return fogData.Y; }
            set { fogData.Y = value; }
        }

        /// <summary>
        /// Gets or sets the fog start.
        /// </summary>
        public float FogStart
        {
            get { return fogData.Z; }
            set { fogData.Z = value; }
        }

        /// <summary>
        /// Gets or sets the fog end.
        /// </summary>
        public float FogEnd
        {
            get { return fogData.W; }
            set { fogData.W = value; }
        }

        public Skybox Skybox
        {
            get { return skybox; }
        }

        /// <summary>
        /// Create the render setting with default values.
        /// </summary>
        public RenderSettings()
        {
            ambientColor = new Vector3(0.1f, 0.1f, 0.1f);
            fogData = Vector4.Zero;
            FogEnabled = false;
            FogMode = FogMode.None;
            FogDensity = 0.01f;
            FogStart = 20.0f;
            FogEnd = 150.0f;
            fogColor = Vector3.One;
            skybox = new Skybox();
        }
    }
}
