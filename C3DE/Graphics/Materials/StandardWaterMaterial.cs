using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class StandardWaterMaterial : StandardMaterialBase
    {
        protected Vector3 _reflectionColor = Vector3.One;

        public Texture2D NormalMap { get; set; }
        public TextureCube ReflectionTexture { get; set; }
        public float Speed = 0.5f;

        [DataMember]
        public float ReflectionIntensity { get; set; } = 0.35f;

        [DataMember]
        public Color ReflectionColor
        {
            get => new Color(_reflectionColor);
            set { _reflectionColor = value.ToVector3(); }
        }

        public StandardWaterMaterial()
            : base()
        {
            m_hasAlpha = true;
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                m_ShaderMaterial = new ForwardStandardWater(this);

            m_ShaderMaterial.LoadEffect(Application.Content);
        }
    }
}
