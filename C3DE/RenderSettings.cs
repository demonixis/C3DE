using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace C3DE
{
    public enum FogMode
    {
        None = 0, Linear, Exp, Exp2
    }

    /// <summary>
    /// Define settings used by the renderer.
    /// </summary>
    [DataContract]
    public class RenderSettings
    {
        internal Vector3 fogColor;
        internal Vector3 ambientColor;
        internal Vector4 fogData;
        internal Skybox skybox;

        /// <summary>
        /// Gets or sets the global ambient color.
        /// </summary>
        [DataMember]
        public Color AmbientColor
        {
            get { return new Color(ambientColor); }
            set { ambientColor = value.ToVector3(); }
        }

        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        [DataMember]
        public Color FogColor
        {
            get { return new Color(fogColor); }
            set { fogColor = value.ToVector3(); }
        }

        /// <summary>
        /// Enable or disable the fog.
        /// </summary>
        [DataMember]
        public bool FogEnabled { get; set; }

        /// <summary>
        /// Gets or sets the type of fog.
        /// </summary>
        [DataMember]
        public FogMode FogMode
        {
            get { return (FogMode)(int)fogData.X; }
            set { fogData.X = (float)value; }
        }

        /// <summary>
        /// Gets or sets the fog density.
        /// </summary>
        [DataMember]
        public float FogDensity
        {
            get { return fogData.Y; }
            set { fogData.Y = value; }
        }

        /// <summary>
        /// Gets or sets the fog start.
        /// </summary>
        [DataMember]
        public float FogStart
        {
            get { return fogData.Z; }
            set { fogData.Z = value; }
        }

        /// <summary>
        /// Gets or sets the fog end.
        /// </summary>
        [DataMember]
        public float FogEnd
        {
            get { return fogData.W; }
            set { fogData.W = value; }
        }

        [DataMember]
        public Skybox Skybox
        {
            get { return skybox; }
            set { skybox = value; }
        }

        /// <summary>
        /// Create the render setting with default values.
        /// </summary>
        public RenderSettings()
        {
            ambientColor = new Vector3(0.05f, 0.05f, 0.05f);
            fogData = Vector4.Zero;
            FogEnabled = false;
            FogMode = FogMode.None;
            FogDensity = 0.01f;
            FogStart = 20.0f;
            FogEnd = 150.0f;
            fogColor = Vector3.One;
            skybox = new Skybox();
        }

        public void Set(RenderSettings settings)
        {
            ambientColor = settings.ambientColor;
            fogData = settings.fogData;
            fogColor = settings.fogColor;
            FogEnabled = settings.FogEnabled;
            skybox = new Skybox(); // FIXME
        }
    }
}
