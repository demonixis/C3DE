using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;

namespace C3DE.Graphics.Materials
{
    public class TransparentMaterial : Material
    {
        public TransparentMaterial()
            : base()
        {
            _hasAlpha = true;
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredTransparent(this);
            else
                _shaderMaterial = new ForwardTransparent(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
