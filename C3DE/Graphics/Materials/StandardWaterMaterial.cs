using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public class StandardWaterMaterial : Material
    {
        public Texture2D NormalMap { get; set; }
        public Texture2D SpecularMap { get; set; }
        public TextureCube ReflectionMap { get; set; }

        public Color SpecularColor { get; set; } = new Color(0.7f, 0.7f, 0.7f);
        public int SpecularPower { get; set; } = 16;
        public float SpecularIntensity { get; set; } = 1.0f;
        public float Speed = 0.5f;
        public float ReflectionIntensity { get; set; } = 0.35f;
        public float Alpha { get; set; } = 0.65f;

        public StandardWaterMaterial()
            : base()
        {
            _hasAlpha = true;
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                _shaderMaterial = new ForwardStandardWater(this);
            else if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredStandardWater(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
