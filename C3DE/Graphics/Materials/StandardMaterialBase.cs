using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public abstract class StandardMaterialBase : Material
    {
        protected Vector3 m_SpecularColor = new Vector3(0.6f, 0.6f, 0.6f);

        public Texture2D NormalMap { get; set; }
        public Texture2D SpecularTexture { get; set; }

        public Color SpecularColor
        {
            get => new Color(m_SpecularColor);
            set => m_SpecularColor = value.ToVector3(); 
        }

        public float SpecularIntensity { get; set; } = 1.0f;

        public float Shininess { get; set; } = 250.0f;
    }
}
