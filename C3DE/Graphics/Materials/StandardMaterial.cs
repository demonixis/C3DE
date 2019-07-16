using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Graphics.Rendering;
using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Shaders.Forward;

namespace C3DE.Graphics.Materials
{
    public class StandardMaterial : Material
    {
        internal Vector3 _emissiveColor = Vector3.Zero;

        public Texture2D NormalMap { get; set; }
        public Texture2D SpecularTexture { get; set; }
        public Color SpecularColor { get; set; } = new Color(0.25f, 0.25f, 0.25f);
        public float SpecularPower { get; set; } = 150.0f;
        public bool CutoutEnabled { get; set; }
        public float Cutout { get; set; }
        public float EmissiveIntensity { get; set; } = 1.0f;
        public float ReflectionIntensity { get; set; } = 0.0f;

        public Texture2D EmissiveMap { get; set; }

        public Color EmissiveColor
        {
            get => new Color(_emissiveColor);
            set => _emissiveColor = value.ToVector3();
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
