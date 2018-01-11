using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public abstract class StandardMaterialBase : Material
    {
        protected Vector3 m_SpecularColor = new Vector3(0.6f, 0.6f, 0.6f);

        public Texture2D NormalTexture { get; set; }
        public Texture2D SpecularTexture { get; set; }

        [DataMember]
        public Color SpecularColor
        {
            get { return new Color(m_SpecularColor); }
            set { m_SpecularColor = value.ToVector3(); }
        }

        [DataMember]
        public float SpecularIntensity { get; set; } = 1.0f;

        [DataMember]
        public float Shininess { get; set; } = 250.0f;
    }
}
