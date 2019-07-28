using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders.Forward;

namespace C3DE.Graphics.Materials
{
    public class ParticleMaterial : Material
    {
        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (_shaderMaterial == null)
            {
                _shaderMaterial = new ForwardParticles(this);
                _shaderMaterial.LoadEffect(Application.Content);
            }
        }
    }
}
