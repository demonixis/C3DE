using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Graphics.Rendering;
using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Shaders.Forward;

namespace C3DE.Graphics.Materials
{
    public class StandardMaterial : StandardMaterialBase
    {
        protected Vector3 m_EmissiveColor = Vector3.Zero;

        public TextureCube ReflectionTexture { get; set; }

        public Color EmissiveColor
        {
            get { return new Color(m_EmissiveColor); }
            set { m_EmissiveColor = value.ToVector3(); }
        }

        public bool EmissiveEnabled { get; set; } = false;

        public float EmissiveIntensity { get; set; } = 1.0f;

        public Texture2D EmissiveTexture { get; set; }

        public StandardMaterial() : base() { }

        public StandardMaterial(string name) : base()
        {
            Name = name;
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredStandard(this);
            else
                _shaderMaterial = new ForwardStandard(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
