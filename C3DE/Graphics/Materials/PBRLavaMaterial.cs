using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public class PBRLavaMaterial : Material
    {
        public Texture2D NormalMap { get; set; }
        public float Speed { get; set; } = 0.25f;
        public float Metallic { get; set; } = 0.0f;
        public float Roughness { get; set; } = 1.0f;

        public PBRLavaMaterial() : base() { }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            _shaderMaterial = new ForwardPBRLava(this);
            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
