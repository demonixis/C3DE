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

        public StandardMaterial() : base() { }

        public StandardMaterial(string name) : base()
        {
            Name = name;
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                _shaderMaterial = new ForwardStandard(this);
            else if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredStandard(this);
            else if (renderer is LightPrePassRenderer)
                _shaderMaterial = new LPPStandard(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
