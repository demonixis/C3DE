using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public class LavaMaterial : Material
    {
        public Texture2D NormalMap { get; set; }
        public float EmissiveIntensity { get; set; } = 2.0f;
        public bool EmissiveEnabled => false;
        public float Speed { get; set; } = 0.25f;

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredLava(this);
            else
                _shaderMaterial = new ForwardLava(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
