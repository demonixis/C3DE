using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class LavaMaterial : Material
    {
        public Texture2D NormalTexture { get; set; }
        public float EmissiveIntensity { get; set; } = 2.0f;
        public bool EmissiveEnabled => false;
        public float Speed { get; set; } = 0.25f;

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                m_ShaderMaterial = new ForwardLava(this);

            m_ShaderMaterial.LoadEffect(Application.Content);
        }
    }
}
