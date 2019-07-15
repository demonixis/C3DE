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

        public bool CutoutEnabled { get; set; }
        public float Cutout { get; set; }
        public float EmissiveIntensity { get; set; } = 1.0f;
        public Texture2D EmissiveMap { get; set; }

        public Color EmissiveColor
        {
            get => new Color(m_EmissiveColor);
            set => m_EmissiveColor = value.ToVector3();
        }

        public StandardMaterial() : base() { }

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
