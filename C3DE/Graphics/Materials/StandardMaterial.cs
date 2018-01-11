using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using C3DE.Graphics.Rendering;
using C3DE.Graphics.Materials.Shaders;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class StandardMaterial : StandardMaterialBase
    {
        protected Vector3 m_EmissiveColor = Vector3.Zero;

        public TextureCube ReflectionTexture { get; set; }

        [DataMember]
        public Color EmissiveColor
        {
            get { return new Color(m_EmissiveColor); }
            set { m_EmissiveColor = value.ToVector3(); }
        }

        [DataMember]
        public bool EmissiveEnabled { get; set; } = false;

        [DataMember]
        public float EmissiveIntensity { get; set; } = 1.0f;
        
        public Texture2D EmissiveTexture { get; set; }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                m_ShaderMaterial = new ForwardStandard(this);
            else if (renderer is DeferredRenderer)
                m_ShaderMaterial = new DeferredStandard(this);
            else if (renderer is LightPrePassRenderer)
                m_ShaderMaterial = new LPPStandard(this);
            else
                throw new System.NotSupportedException("Unlit is not supported with this renderer");

            m_ShaderMaterial.LoadEffect(Application.Content);
        }
    }
}
