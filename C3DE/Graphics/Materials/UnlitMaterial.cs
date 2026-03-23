using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders.Forward;

namespace C3DE.Graphics.Materials
{
    public class UnlitMaterial : Material
    {
        public bool CutoutEnabled { get; set; }
        public float Cutout { get; set; } = 0.25f;

        public UnlitMaterial() : base() { }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            _shaderMaterial = new ForwardUnlit(this);
            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
